using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Anzeige während des Spiels (Unity IMGUI): Kopfzeile, Aufgaben, Hinweise, Computer, Abrechnung, Dialoge und Esc-Menü.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        enum PauseScreen { Main, Settings, ConfirmMainMenu, ConfirmQuit }

        const float VirtualHeight = UiKit.VirtualHeight;
        const float Margin = UiKit.Margin;
        const float PanelWidth = 290f;

        GUIStyle panel, text, bold, title, prompt, hint, button;
        Vector2 orderScroll;
        bool questsExpanded;
        bool showPrices;
        PauseScreen pauseScreen;

        static GameManager Game => GameManager.Instance;

        static bool InGame =>
            Game.Phase != GamePhase.MainMenu && Game.Phase != GamePhase.CharacterSelect && Game.Phase != GamePhase.Intro;

        void Update()
        {
            if (GameInput.FullscreenTogglePressed) Screen.fullScreen = !Screen.fullScreen;
            if (GameInput.QuestTogglePressed && InGame && !Game.PauseOpen) ToggleQuests();
            if (!Game.PauseOpen) pauseScreen = PauseScreen.Main;
        }

        void OnGUI()
        {
            if (!InGame) return;

            float width = UiKit.Begin();
            panel = UiKit.Panel;
            text = UiKit.Text;
            bold = UiKit.Bold;
            title = UiKit.Title;
            prompt = UiKit.Prompt;
            hint = UiKit.Hint;
            button = UiKit.ButtonStyle;

            DrawTopBar(width);
            float questsBottom = DrawQuests(width);
            DrawToasts(width, questsBottom + Margin);
            DrawPrompt(width);

            if (Game.ComputerOpen) DrawComputer(width);
            if (Game.Phase == GamePhase.Summary) DrawSummary(width);
            if (Game.DialogLines != null) DrawDialog(width);
            if (Game.PauseOpen) DrawPause(width);
        }

        static bool Button(string label, params GUILayoutOption[] options) => UiKit.Button(label, options);

        void DrawPause(float width)
        {
            UiKit.DarkenScreen(width, 0.4f);
            switch (pauseScreen)
            {
                case PauseScreen.Main:
                    UiKit.BeginWindow(UiKit.Centered(width, 300f, 300f));
                    GUILayout.Label("Pause", title);
                    GUILayout.Space(6f);
                    if (UiKit.BigButtonLayout("Weiter [Esc]")) Game.PauseOpen = false;
                    if (UiKit.BigButtonLayout("Speichern")) Game.SaveGame();
                    if (UiKit.BigButtonLayout("Einstellungen")) pauseScreen = PauseScreen.Settings;
                    if (UiKit.BigButtonLayout("Zum Hauptmenü")) pauseScreen = PauseScreen.ConfirmMainMenu;
                    if (UiKit.BigButtonLayout("Spiel beenden")) pauseScreen = PauseScreen.ConfirmQuit;
                    UiKit.EndWindow();
                    break;

                case PauseScreen.Settings:
                    UiKit.BeginWindow(UiKit.Centered(width, 400f, 260f));
                    GUILayout.Label("Einstellungen", title);
                    GUILayout.Space(10f);
                    UiKit.SettingsContent();
                    GUILayout.FlexibleSpace();
                    if (Button("Zurück", GUILayout.Height(34f))) pauseScreen = PauseScreen.Main;
                    UiKit.EndWindow();
                    break;

                case PauseScreen.ConfirmMainMenu:
                    DrawLeaveConfirm(width, "Zum Hauptmenü?", GameBootstrap.ShowMainMenu);
                    break;

                case PauseScreen.ConfirmQuit:
                    DrawLeaveConfirm(width, "Spiel beenden?", UiKit.QuitGame);
                    break;
            }
        }

        /// <summary>Vor dem Verlassen fragen, ob gespeichert werden soll.</summary>
        void DrawLeaveConfirm(float width, string question, System.Action leave)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 440f, 200f));
            GUILayout.Label(question, title);
            GUILayout.Label("Nicht gespeicherter Fortschritt seit dem letzten Speichern geht verloren.", text);
            GUILayout.FlexibleSpace();
            if (Button("Speichern und verlassen", GUILayout.Height(34f)))
            {
                Game.SaveGame();
                leave();
            }
            GUILayout.BeginHorizontal();
            if (Button("Ohne Speichern", GUILayout.Height(34f))) leave();
            if (Button("Abbrechen", GUILayout.Height(34f))) pauseScreen = PauseScreen.Main;
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
        }

        void DrawTopBar(float width)
        {
            string phase;
            switch (Game.Phase)
            {
                case GamePhase.Preparation: phase = "Vorbereitung (geschlossen)"; break;
                case GamePhase.Open: phase = "Geöffnet"; break;
                case GamePhase.Closing: phase = "Ladenschluss"; break;
                default: phase = "Feierabend"; break;
            }
            string line = $"Tag {Game.Day}  ·  {Game.ClockText}  ·  {phase}  ·  Geld: {Money.Format(Game.MoneyCents)}  ·  Ruf: {Game.ReputationName}  ·  [Esc] Menü";
            // Platz lassen für die Aufgabenliste rechts.
            float barWidth = Mathf.Min(760f, width - PanelWidth - 3f * Margin);
            var content = new GUIContent(line);
            float barHeight = Mathf.Max(34f, panel.CalcHeight(content, barWidth));
            GUI.Box(new Rect(Margin, Margin, barWidth, barHeight), content, panel);
        }

        /// <summary>
        /// Aufgabenliste oben rechts. Zugeklappt nur die aktuelle Aufgabe, aufgeklappt (Pfeil oder [Tab]) alle.
        /// Gibt die Unterkante zurück.
        /// </summary>
        float DrawQuests(float width)
        {
            var chapter = GetComponent<ChapterOne>();
            if (chapter == null) return Margin;

            var quests = chapter.Quests;
            int done = quests.FindAll(q => q.Done).Count;
            var current = quests.Find(q => !q.Done);
            int rows = questsExpanded ? quests.Count : 1;

            var area = new Rect(width - PanelWidth - Margin, Margin, PanelWidth, 34f + rows * 22f);
            GUI.Box(area, GUIContent.none, panel);
            GUI.Label(new Rect(area.x + 10f, area.y + 6f, area.width - 90f, 22f), ChapterOne.Title, bold);
            GUI.Label(new Rect(area.xMax - 84f, area.y + 6f, 44f, 22f), $"{done}/{quests.Count}", hint);
            if (GUI.Button(new Rect(area.xMax - 36f, area.y + 5f, 28f, 24f), questsExpanded ? "▲" : "▼", button))
                ToggleQuests();

            float y = area.y + 30f;
            if (questsExpanded)
            {
                foreach (var quest in quests)
                {
                    DrawQuestRow(quest, area.x + 10f, y, area.width - 20f);
                    y += 22f;
                }
            }
            else if (current != null)
            {
                DrawQuestRow(current, area.x + 10f, y, area.width - 20f);
            }
            else
            {
                GUI.color = UiKit.Good;
                GUI.Label(new Rect(area.x + 10f, y, area.width - 20f, 22f), "Kapitel geschafft! Alle Aufgaben: [Tab]", text);
                GUI.color = Color.white;
            }
            return area.yMax;
        }

        void DrawQuestRow(Quest quest, float x, float y, float rowWidth)
        {
            string progress = quest.Target > 1 ? $" ({quest.Progress}/{quest.Target})" : "";
            string mark = quest.Done ? "[x]" : "[  ]";
            GUI.color = quest.Done ? UiKit.Good : Color.white;
            GUI.Label(new Rect(x, y, rowWidth, 22f), $"{mark} {quest.Text}{progress}", text);
            GUI.color = Color.white;
        }

        void ToggleQuests()
        {
            questsExpanded = !questsExpanded;
            SoundManager.Play(Sfx.UiClick);
        }

        /// <summary>Meldungen rechts unter der Aufgabenliste, damit sie den Laden nicht verdecken.</summary>
        void DrawToasts(float width, float top)
        {
            float y = top;
            foreach (var toast in Game.Toasts)
            {
                var content = new GUIContent(toast.Text);
                float height = Mathf.Max(28f, panel.CalcHeight(content, PanelWidth));
                GUI.Box(new Rect(width - PanelWidth - Margin, y, PanelWidth, height), content, panel);
                y += height + 4f;
            }
        }

        void DrawPrompt(float width)
        {
            var player = Game.Player;
            string line = null;

            if (Game.BuildMode)
            {
                bool shelf = BuildController.Current == BuildController.Furniture.Shelf;
                string item = shelf ? $"Regal ({Game.ShelvesToPlace})" : $"Vitrine ({Game.ShowcasesToPlace})";
                bool canSwitch = shelf ? Game.ShowcasesToPlace > 0 : Game.ShelvesToPlace > 0;
                line = $"Bau-Modus: {item}   Linksklick = Aufstellen   " +
                       (canSwitch ? "[B] = Wechseln   Rechtsklick = Abbrechen" : "Rechtsklick / [B] = Abbrechen");
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
                else if (Game.ShelvesToPlace + Game.ShowcasesToPlace > 0 && line == null)
                    line = $"[B] Möbel aufstellen ({Game.ShelvesToPlace + Game.ShowcasesToPlace} übrig)";
                else if (line == null)
                    line = $"[C] Sammelbuch ({Game.Collection.DiscoveredCount}/{CardDatabase.Monstera.Count})";
            }

            if (line == null) return;
            GUI.Box(new Rect((width - 640f) / 2f, VirtualHeight - 44f - Margin, 640f, 40f), line, prompt);
        }

        void DrawComputer(float width)
        {
            // Höchstens so hoch wie der Bildschirm; der Inhalt lässt sich scrollen.
            float height = Mathf.Min(VirtualHeight - 120f, 330f);
            var area = new Rect((width - 480f) / 2f, 60f, 480f, height);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 16f, area.y + 12f, area.width - 32f, area.height - 24f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Computer", title);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Geld: {Money.Format(Game.MoneyCents)}", bold);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.color = showPrices ? UiKit.Dimmed : Color.white;
            if (Button("Bestellen", GUILayout.Height(30f))) showPrices = false;
            GUI.color = showPrices ? Color.white : UiKit.Dimmed;
            if (Button("Preise", GUILayout.Height(30f))) showPrices = true;
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);

            orderScroll = GUILayout.BeginScrollView(orderScroll);
            if (showPrices) DrawPriceList();
            else DrawOrderList();
            GUILayout.EndScrollView();

            GUILayout.Space(6f);
            if (Button("Schließen [Esc]", GUILayout.Height(32f))) Game.ComputerOpen = false;
            GUILayout.EndArea();
        }

        void DrawOrderList()
        {
            GUILayout.Label("Lieferung: sofort an den Eingang", hint);
            foreach (var option in Catalog.Orders)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label($"{option.Name} – {Money.Format(option.Cost)}", bold);
                GUILayout.Label(option.Description, text);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUI.enabled = Game.MoneyCents >= option.Cost;
                if (Button("Kaufen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.Order(option);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(8f);
            }
        }

        void DrawPriceList()
        {
            GUILayout.Label("Teurer als der Marktwert schreckt Kunden ab, günstiger bringt mehr Verkäufe und Ruf.", hint);
            GUILayout.Space(4f);
            foreach (var product in Catalog.Products)
            {
                int price = Game.GetPrice(product);
                int profit = price - product.BuyPrice;

                GUILayout.Label(product.Name, bold);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Einkauf: {Money.Format(product.BuyPrice)}", text);
                GUILayout.Label($"Marktwert: {Money.Format(product.MarketValue)}", text);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dein Preis", text, GUILayout.Width(80f));
                // Schieberegler in 10-Cent-Schritten.
                float euros = GUILayout.HorizontalSlider(price / 100f, GameManager.MinPrice / 100f, GameManager.MaxPrice / 100f, GUILayout.Height(20f));
                int newPrice = Mathf.RoundToInt(euros * 10f) * 10;
                if (newPrice != price) Game.SetPrice(product, newPrice);
                GUILayout.Label(Money.Format(price), bold, GUILayout.Width(70f));
                GUILayout.EndHorizontal();

                GUI.color = profit > 0 ? UiKit.Good : UiKit.Bad;
                GUILayout.Label($"Gewinn pro Pack: {Money.Format(profit)}   ·   {PriceVerdict(price, product.MarketValue)}", text);
                GUI.color = Color.white;
                GUILayout.Space(10f);
            }
        }

        static string PriceVerdict(int price, int marketValue)
        {
            float ratio = price / (float)marketValue;
            if (ratio <= 0.85f) return "Schnäppchen!";
            if (ratio <= 1f) return "Fairer Preis";
            if (ratio <= 1.3f) return "Etwas teuer";
            return "Viel zu teuer";
        }

        void DrawSummary(float width)
        {
            var stats = Game.Today;
            var area = new Rect((width - 420f) / 2f, 80f, 420f, 310f);
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

            GUILayout.FlexibleSpace();
            if (Button($"Schlafen → Tag {Game.Day + 1}", GUILayout.Height(40f))) Game.Sleep();
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
    }
}
