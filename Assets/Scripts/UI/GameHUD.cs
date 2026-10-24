using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Anzeige während des Spiels (Unity IMGUI): Kopfzeile, Aufgaben, Hinweise, Computer, Abrechnung, Dialoge und Esc-Menü.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        enum PauseScreen { Main, Settings, ConfirmMainMenu, ConfirmQuit, TestUnlock, TestUnlockSure }

        /// <summary>Test-Werkzeuge im Esc-Menü. Vor der Veröffentlichung auf false setzen.</summary>
        const bool ShowTestTools = true;

        const float VirtualHeight = UiKit.VirtualHeight;
        const float Margin = UiKit.Margin;
        const float PanelWidth = 330f;

        GUIStyle panel, text, bold, title, prompt, hint, button;
        Vector2 orderScroll;
        bool questsExpanded;
        /// <summary>Computer-Tab: 0 = Ware, 1 = Einrichtung, 2 = Preise, 3 = Level.</summary>
        int computerTab;
        PauseScreen pauseScreen;

        static GameManager Game => GameManager.Instance;

        static bool InGame =>
            Game.Phase != GamePhase.MainMenu && Game.Phase != GamePhase.CharacterSelect && Game.Phase != GamePhase.Intro &&
            !Game.CutscenePlaying;

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
            if (Game.LevelUpShown > 0) DrawLevelUp(width);
            if (Game.PauseOpen) DrawPause(width);
        }

        static bool Button(string label, params GUILayoutOption[] options) => UiKit.Button(label, options);

        void DrawPause(float width)
        {
            UiKit.DarkenScreen(width, 0.4f);
            switch (pauseScreen)
            {
                case PauseScreen.Main:
                    UiKit.BeginWindow(UiKit.Centered(width, 300f, ShowTestTools ? 350f : 300f));
                    GUILayout.Label("Pause", title);
                    GUILayout.Space(6f);
                    if (UiKit.BigButtonLayout("Weiter [Esc]")) Game.PauseOpen = false;
                    if (UiKit.BigButtonLayout("Speichern")) Game.SaveGame();
                    if (UiKit.BigButtonLayout("Einstellungen")) pauseScreen = PauseScreen.Settings;
                    if (UiKit.BigButtonLayout("Zum Hauptmenü")) pauseScreen = PauseScreen.ConfirmMainMenu;
                    if (UiKit.CanQuit && UiKit.BigButtonLayout("Spiel beenden")) pauseScreen = PauseScreen.ConfirmQuit;
                    if (ShowTestTools)
                    {
                        GUILayout.Space(6f);
                        GUI.enabled = Game.StoreLevel < StoreLevels.Max;
                        if (Button($"TEST: Level {Mathf.Min(Game.StoreLevel + 1, StoreLevels.Max)} freischalten", GUILayout.Height(30f)))
                            pauseScreen = PauseScreen.TestUnlock;
                        GUI.enabled = true;
                    }
                    UiKit.EndWindow();
                    break;

                case PauseScreen.TestUnlock:
                    DrawTestConfirm(width, $"Level {Game.StoreLevel + 1} freischalten?",
                        "Nur zum Testen: Das überspringt XP und Kapitel-Aufgaben.", "Ja", PauseScreen.TestUnlockSure);
                    break;

                case PauseScreen.TestUnlockSure:
                    DrawTestConfirm(width, "Wirklich ganz sicher?",
                        "Der Spielstand bekommt das neue Level dauerhaft, sobald gespeichert wird.", "Ja, freischalten", PauseScreen.Main, unlock: true);
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

        /// <summary>Doppelte Nachfrage für das Test-Freischalten.</summary>
        void DrawTestConfirm(float width, string question, string detail, string confirm, PauseScreen next, bool unlock = false)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 420f, 190f));
            GUILayout.Label(question, title);
            GUILayout.Label(detail, text);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (Button("Abbrechen", GUILayout.Height(34f))) pauseScreen = PauseScreen.Main;
            if (Button(confirm, GUILayout.Height(34f)))
            {
                pauseScreen = next;
                if (unlock)
                {
                    Game.PauseOpen = false;
                    Game.TestUnlockNextLevel();
                }
            }
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
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
            line += "\n" + LevelLine();
            // Platz lassen für die Aufgabenliste rechts.
            float barWidth = Mathf.Min(760f, width - PanelWidth - 3f * Margin);
            var content = new GUIContent(line);
            float barHeight = Mathf.Max(34f, panel.CalcHeight(content, barWidth));
            GUI.Box(new Rect(Margin, Margin, barWidth, barHeight), content, panel);
        }

        /// <summary>Store-Level, XP-Fortschritt und was für den nächsten Aufstieg fehlt.</summary>
        static string LevelLine()
        {
            int level = Game.StoreLevel;
            string line = $"Store-Level {level}: {StoreLevels.Title(level)}  ·  Gemütlichkeit {Game.Coziness}";
            if (level >= StoreLevels.Max) return line + "  ·  Höchstes Level!";

            int from = StoreLevels.XpFor(level), to = StoreLevels.XpFor(level + 1);
            int filled = Mathf.Clamp(Mathf.FloorToInt(10f * (Game.Xp - from) / (to - from)), 0, 10);
            line += $"  ·  XP {new string('|', filled)}{new string('.', 10 - filled)} {Game.Xp}/{to}";
            if (Game.Xp >= to && !Game.ChapterComplete)
                line += "  ·  Kapitel-Aufgaben erledigen!";
            return line;
        }

        void DrawLevelUp(float width)
        {
            UiKit.DarkenScreen(width, 0.6f);
            int level = Game.LevelUpShown;
            UiKit.BeginWindow(UiKit.Centered(width, 460f, 320f));
            GUILayout.Label("Level-Aufstieg!", UiKit.BigTitle.WithFontSize(34));
            GUILayout.Label($"TwoMoons ist jetzt Store-Level {level}: {StoreLevels.Title(level)}", title.WithAlignment(TextAnchor.MiddleCenter).WithFontSize(18));
            GUILayout.Space(10f);
            GUILayout.Label("Neu freigeschaltet:", bold);
            foreach (string unlock in StoreLevels.UnlocksFor(level)) GUILayout.Label($"•  {unlock}", text);
            GUILayout.FlexibleSpace();
            if (UiKit.BigButtonLayout("Weiter")) Game.CloseLevelUp();
            UiKit.EndWindow();
        }

        /// <summary>
        /// Aufgabenliste oben rechts. Zugeklappt nur die aktuelle Aufgabe, aufgeklappt (Pfeil oder [Tab]) alle.
        /// Gibt die Unterkante zurück.
        /// </summary>
        float DrawQuests(float width)
        {
            var chapters = Game.ChapterProgress;
            if (chapters == null) return Margin;

            // Kapitel geschafft: Liste ausblenden, höchstens noch zeigen, wie viele XP fehlen.
            if (chapters.Completed)
            {
                int level = Game.StoreLevel;
                if (level >= StoreLevels.Max || Game.Xp >= StoreLevels.XpFor(level + 1)) return Margin;
                var done = new Rect(width - PanelWidth - Margin, Margin, PanelWidth, 34f);
                GUI.Box(done, $"Kapitel {chapters.Chapter} geschafft! Noch {StoreLevels.XpFor(level + 1) - Game.Xp} XP bis Level {level + 1}", panel);
                return done.yMax;
            }

            var open = chapters.OpenQuests;
            int rows = questsExpanded ? open.Count : Mathf.Min(1, open.Count);

            var area = new Rect(width - PanelWidth - Margin, Margin, PanelWidth, 34f + rows * 22f);
            GUI.Box(area, GUIContent.none, panel);
            GUI.Label(new Rect(area.x + 10f, area.y + 6f, area.width - 90f, 22f), chapters.Header, bold);
            GUI.Label(new Rect(area.xMax - 84f, area.y + 6f, 44f, 22f), $"{open.Count} offen", hint);
            if (GUI.Button(new Rect(area.xMax - 36f, area.y + 5f, 28f, 24f), questsExpanded ? "▲" : "▼", button))
                ToggleQuests();

            float y = area.y + 30f;
            for (int i = 0; i < rows; i++)
            {
                DrawQuestRow(open[i], area.x + 10f, y, area.width - 20f);
                y += 22f;
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
                var current = BuildController.Current;
                var def = FurnitureCatalog.Get(current);
                string item = $"{def.Name} ({Game.ToPlace(current)})";
                bool canSwitch = Game.TotalToPlace > Game.ToPlace(current);
                string where = def.Placement == Placement.Wall ? " an die hintere Wand" : "";
                line = $"Bau-Modus: {item}{where}   Linksklick = Aufstellen   " +
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
                else if (Game.TotalToPlace > 0 && line == null)
                    line = $"[B] Möbel aufstellen ({Game.TotalToPlace} übrig)";
                else if (line == null)
                    line = $"[C] Sammelbuch ({Game.Collection.DiscoveredCount}/{CardDatabase.Monstera.Count})";
            }

            if (line == null) return;
            GUI.Box(new Rect((width - 640f) / 2f, VirtualHeight - 44f - Margin, 640f, 40f), line, prompt);
        }

        void DrawComputer(float width)
        {
            // Höchstens so hoch wie der Bildschirm; der Inhalt lässt sich scrollen.
            float height = Mathf.Min(VirtualHeight - 100f, 380f);
            var area = new Rect((width - 500f) / 2f, 60f, 500f, height);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 16f, area.y + 12f, area.width - 32f, area.height - 24f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Computer", title);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Geld: {Money.Format(Game.MoneyCents)}", bold);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            string[] tabs = { "Ware", "Einrichtung", "Preise", "Level" };
            for (int i = 0; i < tabs.Length; i++)
            {
                GUI.color = computerTab == i ? Color.white : UiKit.Dimmed;
                if (Button(tabs[i], GUILayout.Height(30f))) computerTab = i;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);

            orderScroll = GUILayout.BeginScrollView(orderScroll);
            if (computerTab == 2) DrawPriceList();
            else if (computerTab == 3) DrawLevelList();
            else DrawOrderList(computerTab == 0 ? Catalog.Orders : Catalog.Furnishings);
            GUILayout.EndScrollView();

            GUILayout.Space(6f);
            if (Button("Schließen [Esc]", GUILayout.Height(32f))) Game.ComputerOpen = false;
            GUILayout.EndArea();
        }

        void DrawOrderList(OrderOption[] options)
        {
            GUILayout.Label(options == Catalog.Orders
                ? "Lieferung: sofort an den Eingang"
                : "Lieferung sofort · mit [B] aufstellen · Deko macht den Laden gemütlicher", hint);
            foreach (var option in options)
            {
                bool locked = option.MinLevel > Game.StoreLevel;
                GUILayout.BeginHorizontal();
                GUI.color = locked ? UiKit.Dimmed : Color.white;
                GUILayout.BeginVertical();
                GUILayout.Label($"{option.Name} – {Money.Format(option.Cost)}", bold);
                GUILayout.Label(option.Description, text);
                GUILayout.EndVertical();
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                if (locked)
                {
                    GUILayout.Label($"ab Level {option.MinLevel}", hint, GUILayout.Width(90f), GUILayout.Height(36f));
                }
                else
                {
                    GUI.enabled = Game.MoneyCents >= option.Cost;
                    if (Button("Kaufen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.Order(option);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(8f);
            }
        }

        /// <summary>Alle Store-Level mit ihren Freischaltungen: erreicht, aktuell, noch gesperrt.</summary>
        void DrawLevelList()
        {
            int current = Game.StoreLevel;
            GUILayout.Label($"Laden-Erfahrung: {Game.Xp} XP", hint);
            for (int level = 1; level <= StoreLevels.Max; level++)
            {
                bool reached = level <= current;
                string status;
                if (level == current) status = "aktuelles Level";
                else if (reached) status = "erreicht";
                else status = $"ab {StoreLevels.XpFor(level)} XP + Kapitel {level - 1}";

                GUILayout.BeginVertical(panel);
                GUI.color = level == current ? UiKit.Gold : reached ? UiKit.Good : UiKit.Dimmed;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Level {level}: {StoreLevels.Title(level)}", bold);
                GUILayout.FlexibleSpace();
                GUILayout.Label(status, text);
                GUILayout.EndHorizontal();
                foreach (string unlock in StoreLevels.UnlocksFor(level))
                    GUILayout.Label($"{(reached ? "[x]" : "[  ]")}  {unlock}", text);
                GUI.color = Color.white;
                GUILayout.EndVertical();
                GUILayout.Space(4f);
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
