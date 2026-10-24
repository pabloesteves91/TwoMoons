using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// 3D-Drucker (ab Level 4): Filament-Spulen einlegen, Modell wählen, drucken lassen (mit Fortschrittsbalken),
    /// fertige Figur abholen und ins Druck-Regal stellen. Ab und zu verheddert sich das Filament.
    /// </summary>
    public class Printer3D : Interactable
    {
        public class Model
        {
            public Product Product;
            public int Filament;
            /// <summary>Druckzeit in Spielstunden.</summary>
            public float Hours;
        }

        public static readonly Model[] Models =
        {
            new Model { Product = Catalog.PrintSmall, Filament = 1, Hours = 1f },
            new Model { Product = Catalog.PrintMedium, Filament = 2, Hours = 2f },
            new Model { Product = Catalog.PrintLarge, Filament = 4, Hours = 3f },
        };

        /// <summary>Echtzeit-Sekunden pro Spielstunde (7 Minuten für 9 Stunden).</summary>
        static float SecondsPerHour => GameManager.DayLengthSeconds / 9f;
        const float JamChancePerPrint = 0.12f;

        public Vector2Int Cell { get; private set; }
        public int Filament;
        public Model Printing { get; private set; }
        public float Progress { get; private set; }
        public bool Jammed { get; private set; }
        public bool Done => Printing != null && Progress >= 1f;

        float jamAt = -1f;
        SpriteRenderer barBack, barFill, statusIcon;

        public void Init(Vector2Int cell)
        {
            Cell = cell;
            var position = ShopGrid.CellToWorld(cell) + new Vector3(0f, 0.75f, 0f);
            barBack = WorldFactory.CreateSprite("Balken", "white", position, 40, transform);
            barBack.color = new Color(0.1f, 0.08f, 0.08f, 0.9f);
            barBack.transform.localScale = new Vector3(0.9f, 0.14f, 1f);
            barFill = WorldFactory.CreateSprite("Fortschritt", "white", position, 41, transform);
            barFill.color = UiKit.Gold;
            statusIcon = WorldFactory.CreateSprite("Status", "moodAngry", position + new Vector3(0f, 0.35f, 0f), 42, transform);
            statusIcon.transform.localScale = Vector3.one * 0.5f;
            GetComponent<YSort>().Collect();
            Refresh();
        }

        public void StartPrint(Model model)
        {
            if (Printing != null || Filament < model.Filament) return;
            Filament -= model.Filament;
            Printing = model;
            Progress = 0f;
            Jammed = false;
            float jamChance = GameManager.Instance.Skills.Has(Skill.Tinkerer) ? JamChancePerPrint / 2f : JamChancePerPrint;
            jamAt = Random.value < jamChance ? Random.Range(0.2f, 0.8f) : -1f;
            SoundManager.Play(Sfx.Build);
            Refresh();
        }

        /// <summary>Zustand aus dem Spielstand.</summary>
        public void Restore(int filament, int model, float progress, bool jammed)
        {
            Filament = filament;
            Printing = model >= 0 && model < Models.Length ? Models[model] : null;
            Progress = progress;
            Jammed = jammed;
            Refresh();
        }

        public int ModelIndex => Printing == null ? -1 : System.Array.IndexOf(Models, Printing);

        void Update()
        {
            if (Printing == null || Jammed || Done) return;
            // Ab Level 6 druckt der Drucker schneller.
            float speed = (GameManager.Instance.StoreLevel >= 6 ? 1.5f : 1f) * (GameManager.Instance.Skills.Has(Skill.Tinkerer) ? 1.25f : 1f);
            Progress = Mathf.Min(1f, Progress + speed * Time.deltaTime / (Printing.Hours * SecondsPerHour));
            if (jamAt >= 0f && Progress >= jamAt)
            {
                Jammed = true;
                jamAt = -1f;
                GameManager.Instance.ShowToast("Druckfehler! Das Filament hat sich verheddert – [E] am 3D-Drucker.", 5f);
                SoundManager.Play(Sfx.Error);
            }
            if (Done)
            {
                GameManager.Instance.ShowToast($"3D-Druck fertig: {Printing.Product.Name}!", 4f);
                SoundManager.Play(Sfx.Quest);
            }
            Refresh();
        }

        void Refresh()
        {
            bool busy = Printing != null;
            barBack.enabled = barFill.enabled = busy;
            if (busy)
            {
                barFill.transform.localScale = new Vector3(0.9f * Progress, 0.1f, 1f);
                barFill.transform.position = barBack.transform.position + new Vector3(-0.45f * (1f - Progress), 0f, 0f);
                barFill.color = Jammed ? UiKit.Bad : Done ? UiKit.Good : UiKit.Gold;
            }
            statusIcon.enabled = Jammed || Done;
            if (statusIcon.enabled) statusIcon.sprite = Art.Get(Jammed ? "moodAngry" : Printing.Product.SpriteKey);
        }

        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried != null && carried.Product.Category == ProductCategory.Supply)
                return $"{carried.Amount} Filament-Spulen einlegen";
            if (Jammed) return "Druckfehler beheben";
            if (Done) return $"{Printing.Product.Name} abholen";
            if (Printing != null) return $"Druckt {Printing.Product.Name}: {Mathf.RoundToInt(Progress * 100f)} %";
            return Filament > 0 ? $"3D-Drucker: Modell wählen ({Filament} Spulen)" : "3D-Drucker: Filament fehlt (am Computer bestellen)";
        }

        public override bool CanInteract(PlayerController player)
        {
            var carried = player.Carried;
            if (carried != null) return carried.Product.Category == ProductCategory.Supply;
            return Jammed || Done || (Printing == null && Filament > 0);
        }

        public override void Interact(PlayerController player)
        {
            var game = GameManager.Instance;
            var carried = player.Carried;
            if (carried != null)
            {
                Filament += carried.Amount;
                player.DestroyCarried();
                SoundManager.Play(Sfx.Stock);
                return;
            }
            if (Jammed)
            {
                Jammed = false;
                SoundManager.Play(Sfx.Clean);
                Refresh();
                return;
            }
            if (Done)
            {
                var product = Printing.Product;
                Printing = null;
                Progress = 0f;
                Refresh();
                player.PickUp(WorldFactory.SpawnBox(Cell, product, 1));
                game.NotifyPrinted();
                return;
            }
            game.OpenPrinter = this;
        }
    }
}
