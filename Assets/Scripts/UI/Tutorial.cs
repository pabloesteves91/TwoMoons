using System;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Kleines Tutorial am ersten Tag: Schritt für Schritt (Laufen, Putzen, Computer, Bauen, Einräumen, Öffnen, Kassieren),
    /// ein Pfeil zeigt auf das Ziel. Beim neuen Spiel wird gefragt, ob man es spielen möchte; überspringen geht jederzeit.
    /// </summary>
    public class Tutorial : MonoBehaviour
    {
        class Step
        {
            public string Text;
            /// <summary>Wohin der Pfeil zeigt (null = nirgends).</summary>
            public Func<Vector3?> Target;
            /// <summary>Schritt geschafft? (Wird jedes Frame geprüft.)</summary>
            public Func<bool> Done;
            /// <summary>Letzter Schritt: nur mit dem Knopf „Fertig“ weiter.</summary>
            public bool Final;
        }

        public bool Asking { get; private set; }
        public bool Active => stepIndex >= 0;

        int stepIndex = -1;
        Vector3 startPosition;
        bool cleaned, shelfPlaced, stocked, opened, served;
        Step[] steps;

        static GameManager Game => GameManager.Instance;

        void Start()
        {
            var game = Game;
            game.DirtCleaned += () => cleaned = true;
            game.ShelfPlaced += () => shelfPlaced = true;
            game.ShelfStocked += _ => stocked = true;
            game.ShopOpened += () => opened = true;
            game.CustomerServed += () => served = true;

            steps = new[]
            {
                new Step
                {
                    Text = "Laufen: mit [W] [A] [S] [D] oder den Pfeiltasten. Lauf ein paar Schritte durch deinen Laden.",
                    Done = () => (Game.Player.transform.position - startPosition).sqrMagnitude > 4f,
                },
                new Step
                {
                    Text = "Der Laden ist staubig. Geh zu einem Fleck und drück [E], um ihn wegzuwischen.\nFlecken machen Kunden ungeduldig.",
                    Target = () => DirtSpot.Existing.Count > 0 ? Nearest(DirtSpot.Existing.ConvertAll(d => d.transform.position)) : (Vector3?)null,
                    Done = () => cleaned || DirtSpot.Existing.Count == 0,
                },
                new Step
                {
                    Text = "Das ist dein Computer. Geh hin und drück [E].",
                    Target = () => ShopGrid.CellToWorld(ShopGrid.ComputerCell),
                    Done = () => Game.ComputerOpen,
                },
                new Step
                {
                    Text = "Am Computer bestellst du Ware und Möbel (sofort geliefert), stellst Preise ein und siehst, was dein nächstes Level bringt.\nSchau dich um und schließ ihn dann mit [Esc].",
                    Done = () => !Game.ComputerOpen,
                },
                new Step
                {
                    Text = "Stell Papas altes Regal auf: Drück [B] für den Bau-Modus und klick mit der linken Maustaste auf einen freien Platz.\nRechtsklick bricht ab.",
                    Done = () => shelfPlaced || Game.Shelves.Count > 0,
                },
                new Step
                {
                    Text = "Am Eingang steht ein Karton mit Monstera-Packs. Geh hin und heb ihn mit [E] auf.",
                    Target = () => DeliveryBox.Existing.Count > 0 ? Nearest(DeliveryBox.Existing.ConvertAll(b => b.transform.position)) : (Vector3?)null,
                    Done = () => Game.Player.Carried != null || stocked,
                },
                new Step
                {
                    Text = "Trag den Karton zum Regal und drück dort [E], um die Packs einzuräumen.\n([Q] stellt einen Karton wieder ab.)",
                    Target = () => Game.Shelves.Count > 0 ? Game.Shelves[0].transform.position : (Vector3?)null,
                    Done = () => stocked,
                },
                new Step
                {
                    Text = "Alles bereit! Geh zum Schild neben der Tür und drück [E], um den Laden zu öffnen.",
                    Target = () => ShopGrid.CellToWorld(ShopGrid.SignCell),
                    Done = () => opened || Game.Phase != GamePhase.Preparation,
                },
                new Step
                {
                    Text = "Kunden nehmen Packs und stellen sich an der Kasse an. Stell dich hinter die Theke und drück [E] an der Kasse.\nWarte nicht zu lange: Das Gesicht über dem Kopf wird erst gelb, dann rot – danach gehen sie.",
                    Target = () => ShopGrid.CellToWorld(ShopGrid.RegisterCell),
                    Done = () => served,
                },
                new Step
                {
                    Text = "Super, dein erster Verkauf!\n\n• [C] Sammelbuch · [Tab] Aufgaben aufklappen · [Esc] Pause & Speichern\n• Booster öffnen: am Regal mit [E] einen herausnehmen und zum Pack-Tisch hinter der Theke bringen.\n• Um 18 Uhr schließt der Laden, danach gibt's die Abrechnung.\n\nDie Aufgaben oben rechts führen dich weiter. Viel Spaß!",
                    Final = true,
                },
            };
        }

        /// <summary>Nach dem Intro: fragen, ob man das Tutorial möchte.</summary>
        public void Offer() => Asking = true;

        /// <summary>Aus der Hilfe heraus noch einmal von vorn.</summary>
        public void Restart() => Begin();

        void Begin()
        {
            Asking = false;
            stepIndex = 0;
            startPosition = Game.Player.transform.position;
            cleaned = shelfPlaced = stocked = opened = served = false;
        }

        public void Stop()
        {
            Asking = false;
            stepIndex = -1;
        }

        void Update()
        {
            if (!Active || Game.Phase == GamePhase.MainMenu) return;
            var step = steps[stepIndex];
            if (step.Final || step.Done == null || !step.Done()) return;
            stepIndex++;
            SoundManager.Play(Sfx.Quest);
        }

        static Vector3 Nearest(System.Collections.Generic.List<Vector3> positions)
        {
            var player = Game.Player.transform.position;
            positions.Sort((a, b) => (a - player).sqrMagnitude.CompareTo((b - player).sqrMagnitude));
            return positions[0];
        }

        void OnGUI()
        {
            if (Game == null || Game.Phase == GamePhase.MainMenu || Game.CutscenePlaying) return;
            if (!Asking && !Active) return;
            float width = UiKit.Begin();

            if (Asking)
            {
                UiKit.DarkenScreen(width, 0.45f);
                UiKit.BeginWindow(UiKit.Centered(width, 440f, 200f));
                GUILayout.Label("Kleines Tutorial?", UiKit.Title);
                GUILayout.Label("Ich zeige dir Schritt für Schritt die Steuerung und wie dein Laden funktioniert: " +
                                "laufen, putzen, Computer, Regal aufstellen, einräumen, öffnen und kassieren.", UiKit.Text);
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (UiKit.Button("Nein, ich kenn mich aus", GUILayout.Height(36f))) Stop();
                if (UiKit.Button("Ja, zeig es mir!", GUILayout.Height(36f))) Begin();
                GUILayout.EndHorizontal();
                UiKit.EndWindow();
                return;
            }

            // Während Dialogen, Pause oder Level-Up ausblenden.
            if (Game.DialogLines != null || Game.PauseOpen || Game.LevelUpShown > 0 || Game.Phase == GamePhase.Summary) return;

            var step = steps[stepIndex];
            // Wenn möglich unten links; ist ein großes Fenster offen, als breiter Balken unten links darunter.
            bool below = GameHUD.BigWindowOpen;
            var area = new Rect(UiKit.Margin, 0f, below ? 580f : 320f, 0f);
            var content = new GUIContent(step.Text);
            float textHeight = UiKit.Text.CalcHeight(content, area.width - 28f);
            area.height = textHeight + 82f;
            if (below) area.y = UiKit.VirtualHeight - area.height - UiKit.Margin;
            else area = Place(area, width, step.Target?.Invoke());
            GUI.Box(area, GUIContent.none, UiKit.Panel);
            GUI.color = UiKit.Gold;
            GUI.Label(new Rect(area.x + 14f, area.y + 8f, area.width - 28f, 22f), $"Tutorial · Schritt {stepIndex + 1}/{steps.Length}", UiKit.Bold);
            GUI.color = Color.white;
            GUI.Label(new Rect(area.x + 14f, area.y + 32f, area.width - 28f, textHeight), content, UiKit.Text);
            var buttonRect = new Rect(area.x + 14f, area.yMax - 40f, area.width - 28f, 30f);
            if (step.Final)
            {
                if (GUI.Button(buttonRect, "Fertig", UiKit.ButtonStyle)) { SoundManager.Play(Sfx.UiClick); Stop(); }
            }
            else if (GUI.Button(buttonRect, "Tutorial überspringen", UiKit.ButtonStyle))
            {
                SoundManager.Play(Sfx.UiClick);
                Stop();
            }

            if (step.Target != null && !Game.ComputerOpen) DrawArrow(step.Target());
        }

        /// <summary>
        /// Unten links, wenn möglich. Verdeckt es dort das Ziel (z. B. die Kasse), weicht es nach
        /// oben links, unten rechts oder in die Mitte links aus.
        /// </summary>
        static Rect Place(Rect area, float width, Vector3? target)
        {
            float w = area.width, h = area.height;
            // Über der Hinweiszeile unten in der Mitte bleiben, außer daneben ist genug Platz.
            float promptLeft = (width - 640f) / 2f;
            float bottom = UiKit.VirtualHeight - h - (promptLeft >= w + 2f * UiKit.Margin ? UiKit.Margin : 60f);
            var candidates = new[]
            {
                new Rect(UiKit.Margin, bottom, w, h),
                new Rect(UiKit.Margin, 104f, w, h),
                new Rect(width - w - UiKit.Margin, bottom, w, h),
                new Rect(UiKit.Margin, (UiKit.VirtualHeight - h) / 2f, w, h),
            };
            var point = ToGui(target);
            if (point == null) return candidates[0];
            foreach (var candidate in candidates)
            {
                // Etwas Abstand rund ums Ziel, damit auch der Pfeil darüber frei bleibt.
                var padded = new Rect(candidate.x - 50f, candidate.y - 50f, candidate.width + 100f, candidate.height + 110f);
                if (!padded.Contains(point.Value)) return candidate;
            }
            return candidates[0];
        }

        /// <summary>Weltposition in Oberflächen-Koordinaten (null, wenn es nichts zu zeigen gibt).</summary>
        static Vector2? ToGui(Vector3? target)
        {
            var camera = Camera.main;
            if (target == null || camera == null) return null;
            float scale = Screen.height / UiKit.VirtualHeight;
            var screen = camera.WorldToScreenPoint(target.Value);
            return new Vector2(screen.x / scale, (Screen.height - screen.y) / scale);
        }

        /// <summary>Hüpfender Pfeil über dem Ziel in der Welt.</summary>
        static void DrawArrow(Vector3? target)
        {
            var camera = Camera.main;
            if (target == null || camera == null) return;
            float scale = Screen.height / UiKit.VirtualHeight;
            var screen = camera.WorldToScreenPoint(target.Value + new Vector3(0f, 0.9f, 0f));
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 8f;
            float x = screen.x / scale, y = (Screen.height - screen.y) / scale - bob;
            // Pfeil nach unten aus Rechtecken: Schaft oben, Spitze aus immer schmaleren Streifen.
            var outline = new Color(0.15f, 0.08f, 0.06f);
            UiKit.DrawRect(new Rect(x - 6f, y - 34f, 12f, 16f), outline);
            UiKit.DrawRect(new Rect(x - 4f, y - 32f, 8f, 14f), UiKit.Gold);
            for (int i = 0; i < 9; i++)
            {
                float half = 13f - i * 1.5f;
                UiKit.DrawRect(new Rect(x - half - 1f, y - 18f + i * 2f, half * 2f + 2f, 2f), outline);
                UiKit.DrawRect(new Rect(x - half + 1f, y - 18f + i * 2f, Mathf.Max(0f, half * 2f - 2f), 2f), UiKit.Gold);
            }
            GUI.color = Color.white;
        }
    }
}
