using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Einfache Bildschirmanzeige für den Prototyp (Unity IMGUI).
    /// Wird später durch eine richtige Pixel-Oberfläche ersetzt.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        const float VirtualHeight = 540f;
        const float Margin = 10f;

        GUIStyle panel, text, bold, title, prompt, hint, button;
        string nameInput = "";
        int figure;

        static GameManager Game => GameManager.Instance;

        void OnGUI()
        {
            EnsureStyles();
            float scale = Screen.height / VirtualHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;

            if (Game.Phase == GamePhase.CharacterSelect)
            {
                DrawCharacterSelect(width);
                return;
            }

            DrawTopBar();
            float questsBottom = DrawQuests(width);
            DrawToasts(width, questsBottom + Margin);
            DrawPrompt(width);

            if (Game.ComputerOpen) DrawComputer(width);
            if (Game.Phase == GamePhase.Summary) DrawSummary(width);
            if (Game.DialogLines != null) DrawDialog(width);
        }

        void DrawCharacterSelect(float width)
        {
            var area = new Rect((width - 520f) / 2f, 70f, 520f, 400f);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 20f, area.y + 16f, area.width - 40f, area.height - 32f));

            GUILayout.Label("TwoMoons", title);
            GUILayout.Label("Ein TCG-Laden, ein Traum – und zwei Monde.", text);
            GUILayout.Space(16f);

            GUILayout.Label("Wie heißt du?", bold);
            GUI.SetNextControlName("name");
            nameInput = GUILayout.TextField(nameInput, 20, GUILayout.Height(28f));
            GUILayout.Space(16f);

            GUILayout.Label("Wähle deine Figur:", bold);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 3; i++)
            {
                var sprite = Art.Get($"player{i + 1}");
                bool selected = figure == i;
                GUI.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                if (GUILayout.Button(GUIContent.none, button, GUILayout.Width(110f), GUILayout.Height(110f))) figure = i;
                var buttonRect = GUILayoutUtility.GetLastRect();
                DrawSprite(sprite, new Rect(buttonRect.x + 15f, buttonRect.y + 10f, 80f, 80f));
                if (selected) GUI.Label(new Rect(buttonRect.x, buttonRect.yMax - 22f, buttonRect.width, 20f), "ausgewählt", hint);
                GUI.color = Color.white;
                GUILayout.Space(10f);
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = nameInput.Trim().Length > 0;
            if (GUILayout.Button("Los geht's!", button, GUILayout.Height(40f))) Game.StartGame(nameInput, figure);
            GUI.enabled = true;

            GUILayout.EndArea();
        }

        void DrawTopBar()
        {
            string phase;
            switch (Game.Phase)
            {
                case GamePhase.Preparation: phase = "Vorbereitung (geschlossen)"; break;
                case GamePhase.Open: phase = "Geöffnet"; break;
                case GamePhase.Closing: phase = "Ladenschluss"; break;
                default: phase = "Feierabend"; break;
            }
            string line = $"Tag {Game.Day}   {Game.ClockText}   {phase}      Geld: {Money.Format(Game.MoneyCents)}      Ruf: {Game.ReputationName}";
            GUI.Box(new Rect(Margin, Margin, 640f, 34f), line, panel);
        }

        /// <summary>Aufgabenliste oben rechts. Gibt die Unterkante zurück.</summary>
        float DrawQuests(float width)
        {
            var chapter = GetComponent<ChapterOne>();
            if (chapter == null) return Margin;

            float height = 46f + chapter.Quests.Count * 22f;
            var area = new Rect(width - 270f - Margin, Margin, 270f, height);
            GUI.Box(area, GUIContent.none, panel);
            GUI.Label(new Rect(area.x + 10f, area.y + 8f, area.width - 20f, 24f), ChapterOne.Title, bold);
            for (int i = 0; i < chapter.Quests.Count; i++)
            {
                var quest = chapter.Quests[i];
                string progress = quest.Target > 1 ? $" ({quest.Progress}/{quest.Target})" : "";
                string mark = quest.Done ? "[x]" : "[  ]";
                GUI.color = quest.Done ? new Color(0.6f, 1f, 0.6f) : Color.white;
                GUI.Label(new Rect(area.x + 10f, area.y + 36f + i * 22f, area.width - 20f, 22f), $"{mark} {quest.Text}{progress}", text);
            }
            GUI.color = Color.white;
            return area.yMax;
        }

        /// <summary>Meldungen rechts unter der Aufgabenliste, damit sie den Laden nicht verdecken.</summary>
        void DrawToasts(float width, float top)
        {
            float y = top;
            foreach (var toast in Game.Toasts)
            {
                var content = new GUIContent(toast.Text);
                float height = Mathf.Max(28f, panel.CalcHeight(content, 270f));
                GUI.Box(new Rect(width - 270f - Margin, y, 270f, height), content, panel);
                y += height + 4f;
            }
        }

        void DrawPrompt(float width)
        {
            var player = Game.Player;
            string line = null;

            if (Game.BuildMode)
            {
                line = $"Bau-Modus: Linksklick = Regal aufstellen   Rechtsklick / [B] = Abbrechen   (noch {Game.ShelvesToPlace})";
            }
            else if (!Game.InputLocked)
            {
                var focus = player.Focus;
                if (focus != null)
                {
                    string promptText = focus.GetPrompt(player);
                    line = focus.CanInteract(player) ? $"[E] {promptText}" : promptText;
                }
                if (player.Carried != null)
                    line = (line != null ? line + "      " : "") + $"Trägst: {player.Carried.Amount}× {player.Carried.Product.Name}   [Q] abstellen";
                else if (Game.ShelvesToPlace > 0 && line == null)
                    line = $"[B] Regal aufstellen ({Game.ShelvesToPlace} übrig)";
            }

            if (line == null) return;
            GUI.Box(new Rect((width - 640f) / 2f, VirtualHeight - 44f - Margin, 640f, 40f), line, prompt);
        }

        void DrawComputer(float width)
        {
            var area = new Rect((width - 440f) / 2f, 80f, 440f, 120f + Catalog.Orders.Length * 62f);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 16f, area.y + 12f, area.width - 32f, area.height - 24f));

            GUILayout.Label("Großhandel – Bestellung", title);
            GUILayout.Label($"Geld: {Money.Format(Game.MoneyCents)}     Lieferung: morgen früh", text);
            GUILayout.Space(8f);

            foreach (var option in Catalog.Orders)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label($"{option.Name} – {Money.Format(option.Cost)}", bold);
                GUILayout.Label(option.Description, text);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUI.enabled = Game.MoneyCents >= option.Cost;
                if (GUILayout.Button("Kaufen", button, GUILayout.Width(90f), GUILayout.Height(36f))) Game.Order(option);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }

            if (Game.PendingDeliveries.Count > 0)
                GUILayout.Label($"Bestellt für morgen: {Game.PendingDeliveries.Count} Lieferung(en)", text);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Schließen [Esc]", button, GUILayout.Height(32f))) Game.ComputerOpen = false;
            GUILayout.EndArea();
        }

        void DrawSummary(float width)
        {
            var stats = Game.Today;
            var area = new Rect((width - 420f) / 2f, 80f, 420f, 330f);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 20f, area.y + 14f, area.width - 40f, area.height - 28f));

            GUILayout.Label($"Tag {Game.Day} – Abrechnung", title);
            GUILayout.Space(8f);
            SummaryRow("Einnahmen", Money.Format(stats.Income));
            SummaryRow("Ausgaben (Bestellungen)", Money.Format(stats.Expenses));
            SummaryRow("Gewinn", Money.Format(stats.Income - stats.Expenses));
            GUILayout.Space(6f);
            SummaryRow("Verkaufte Packs", stats.PacksSold.ToString());
            SummaryRow("Zufriedene Kunden", stats.CustomersServed.ToString());
            SummaryRow("Unzufriedene Kunden", stats.CustomersUnhappy.ToString());
            SummaryRow("Ruf", Game.ReputationName);
            GUILayout.Space(6f);
            GUILayout.Label(Game.PendingDeliveries.Count > 0
                ? $"Morgen früh kommen {Game.PendingDeliveries.Count} Lieferung(en)."
                : "Für morgen ist nichts bestellt.", text);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button($"Schlafen → Tag {Game.Day + 1}", button, GUILayout.Height(40f))) Game.Sleep();
            GUILayout.EndArea();
        }

        void SummaryRow(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, text);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, bold);
            GUILayout.EndHorizontal();
        }

        void DrawDialog(float width)
        {
            var area = new Rect((width - 720f) / 2f, VirtualHeight - 150f - Margin, 720f, 150f);
            GUI.Box(area, GUIContent.none, panel);
            GUI.Label(new Rect(area.x + 20f, area.y + 16f, area.width - 40f, area.height - 50f), Game.DialogLines[Game.DialogIndex], text);
            GUI.Label(new Rect(area.x + 20f, area.yMax - 30f, area.width - 40f, 22f), "Weiter: [Leertaste] / Klick", hint);
        }

        static void DrawSprite(Sprite sprite, Rect rect)
        {
            var texture = sprite.texture;
            var r = sprite.textureRect;
            var uv = new Rect(r.x / texture.width, r.y / texture.height, r.width / texture.width, r.height / texture.height);
            GUI.DrawTextureWithTexCoords(rect, texture, uv);
        }

        void EnsureStyles()
        {
            if (panel != null) return;

            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(0.09f, 0.07f, 0.14f, 0.88f));
            background.Apply();

            var buttonBackground = new Texture2D(1, 1);
            buttonBackground.SetPixel(0, 0, new Color(0.27f, 0.22f, 0.42f, 1f));
            buttonBackground.Apply();

            var buttonHover = new Texture2D(1, 1);
            buttonHover.SetPixel(0, 0, new Color(0.38f, 0.31f, 0.58f, 1f));
            buttonHover.Apply();

            panel = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 12, 6, 6),
                wordWrap = true,
            };
            panel.normal.background = background;
            panel.normal.textColor = Color.white;

            text = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            text.normal.textColor = Color.white;

            bold = new GUIStyle(text) { fontStyle = FontStyle.Bold };
            title = new GUIStyle(text) { fontSize = 22, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(1f, 0.88f, 0.55f);

            prompt = new GUIStyle(panel) { alignment = TextAnchor.MiddleCenter };
            prompt.normal.textColor = new Color(1f, 0.92f, 0.65f);

            hint = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            hint.normal.textColor = new Color(1f, 0.92f, 0.65f);

            button = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };
            button.normal.background = buttonBackground;
            button.normal.textColor = Color.white;
            button.hover.background = buttonHover;
            button.hover.textColor = Color.white;
            button.active.background = buttonHover;
            button.active.textColor = Color.white;
        }
    }
}
