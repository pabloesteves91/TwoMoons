using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Grading-Station (ab Level 5): eine Karte aus der Sammlung einlegen, eine Spielstunde prüfen lassen,
    /// dann die versiegelte Karte mit Note abholen und in die Vitrine legen.
    /// </summary>
    public class GradingStation : Interactable
    {
        static float SecondsPerHour => GameManager.DayLengthSeconds / 9f;

        public Vector2Int Cell { get; private set; }
        /// <summary>Karte in der Prüfung, -1 = leer.</summary>
        public int CardId { get; private set; } = -1;
        public float Progress { get; private set; }
        public bool Busy => CardId >= 0;
        public bool Done => Busy && Progress >= 1f;

        SpriteRenderer barBack, barFill, statusIcon;

        public void Init(Vector2Int cell)
        {
            Cell = cell;
            var position = ShopGrid.CellToWorld(cell) + new Vector3(0f, 0.75f, 0f);
            barBack = WorldFactory.CreateSprite("Balken", "white", position, 40, transform);
            barBack.color = new Color(0.1f, 0.08f, 0.08f, 0.9f);
            barBack.transform.localScale = new Vector3(0.9f, 0.14f, 1f);
            barFill = WorldFactory.CreateSprite("Fortschritt", "white", position, 41, transform);
            statusIcon = WorldFactory.CreateSprite("Status", "slabIcon", position + new Vector3(0f, 0.35f, 0f), 42, transform);
            statusIcon.transform.localScale = Vector3.one * 0.5f;
            GetComponent<YSort>().Collect();
            Refresh();
        }

        /// <summary>Karte aus der Sammlung einlegen und das Grading bezahlen.</summary>
        public void StartGrading(CardDef card)
        {
            var game = GameManager.Instance;
            if (Busy || !game.Spend(Grading.Cost)) return;
            game.Collection.Remove(card.Id);
            CardId = card.Id;
            Progress = 0f;
            SoundManager.Play(Sfx.Stock);
            Refresh();
        }

        public void Restore(int card, float progress)
        {
            CardId = CardDatabase.Get(card) != null ? card : -1;
            Progress = progress;
            Refresh();
        }

        void Update()
        {
            if (!Busy || Done) return;
            Progress = Mathf.Min(1f, Progress + Time.deltaTime / (Grading.Hours * SecondsPerHour));
            if (Done)
            {
                GameManager.Instance.ShowToast($"Grading fertig: {CardDatabase.Get(CardId).Name} – [E] an der Grading-Station.", 4f);
                SoundManager.Play(Sfx.Quest);
            }
            Refresh();
        }

        void Refresh()
        {
            barBack.enabled = barFill.enabled = Busy;
            if (Busy)
            {
                barFill.transform.localScale = new Vector3(0.9f * Progress, 0.1f, 1f);
                barFill.transform.position = barBack.transform.position + new Vector3(-0.45f * (1f - Progress), 0f, 0f);
                barFill.color = Done ? UiKit.Good : new Color(0.55f, 0.75f, 1f);
            }
            statusIcon.enabled = Done;
        }

        public override string GetPrompt(PlayerController player)
        {
            if (Done) return "Bewertete Karte abholen";
            if (Busy) return $"Prüft {CardDatabase.Get(CardId).Name}: {Mathf.RoundToInt(Progress * 100f)} %";
            return $"Grading-Station: Karte bewerten lassen ({Money.Format(Grading.Cost)})";
        }

        public override bool CanInteract(PlayerController player) => player.Carried == null && (Done || !Busy);

        public override void Interact(PlayerController player)
        {
            var game = GameManager.Instance;
            if (Done)
            {
                var slab = new Slab { card = CardId, grade = Grading.Roll() };
                CardId = -1;
                Progress = 0f;
                Refresh();
                game.AddSlab(slab, true);
                return;
            }
            game.OpenGrading = this;
        }
    }
}
