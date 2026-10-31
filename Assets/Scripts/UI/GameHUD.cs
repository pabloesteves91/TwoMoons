using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Anzeige während des Spiels (Unity IMGUI): Kopfzeile, Aufgaben, Hinweise, Computer, Abrechnung, Dialoge und Esc-Menü.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        enum PauseScreen { Main, Settings, ConfirmMainMenu, ConfirmQuit, TestUnlock, TestUnlockSure, Help }

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

            // Große Fenster (Sammelbuch, Vitrine, Pack öffnen …) bekommen den ganzen Bildschirm:
            // Kopfzeile, Aufgaben und Hinweise verschwinden so lange.
            bool windowOpen = Game.CollectionOpen || Game.OpenShowcase != null || Game.OpenedPack != null ||
                              Game.PrizeCounterOpen || Game.OpenPrinter != null || Game.OpenGrading != null ||
                              Game.GradeReveal != null || Game.SellerOffer != null || Game.ComputerOpen || Game.PauseOpen ||
                              Game.StorageOpen;
            BigWindowOpen = windowOpen;
            if (!windowOpen)
            {
                DrawSpeechBubbles();
                DrawTopBar(width);
                float questsBottom = DrawQuests(width);
                DrawToasts(width, questsBottom + Margin);
                DrawPrompt(width);
                DrawHoverInfo();
            }
            else
            {
                topBarTip = null;
                DrawToasts(width, Margin);
            }

            if (Game.ComputerOpen) DrawComputer(width);
            if (Game.Phase == GamePhase.Summary) DrawSummary(width);
            if (Game.DialogLines != null) DrawDialog(width);
            if (Game.LevelUpShown > 0) DrawLevelUp(width);
            if (Game.PauseOpen) DrawPause(width);
            if (!Game.InputLocked) DrawTopBarTip(width);
        }

        static bool Button(string label, params GUILayoutOption[] options) => UiKit.Button(label, options);

        /// <summary>Maus über einem Regal, einer Vitrine oder einem Karton: kleines Schild mit dem Inhalt.</summary>
        void DrawHoverInfo()
        {
            var camera = Camera.main;
            if (camera == null || Game.InputLocked) return;
            var mouse = GameInput.MousePosition;
            var cell = ShopGrid.WorldToCell(camera.ScreenToWorldPoint(mouse));

            string info = null;
            var shelf = Game.Shelves.Find(s => s.Cell == cell);
            if (shelf != null)
            {
                if (shelf.Stock == 0) info = $"{shelf.Label}: leer (0 / {Shelf.Capacity})";
                else if (shelf.Mixed)
                {
                    info = $"{shelf.Label}: {shelf.Stock} / {Shelf.Capacity} Stück";
                    foreach (var item in shelf.Contents) info += $"\n{item.Value}× {item.Key.Name}";
                }
                else info = $"{shelf.Label}: {shelf.Product.Name}\n{shelf.Stock} / {Shelf.Capacity} Stück";
            }
            var showcase = Game.Showcases.Find(s => s.Cell == cell);
            if (showcase != null)
            {
                int filled = 0;
                foreach (int card in showcase.Cards) if (card >= 0) filled++;
                info = $"{(showcase.Premium ? "Premium-Vitrine" : "Vitrine")}: {filled} / {showcase.SlotCount} Karten";
            }
            if (Game.StorageBuilt && cell == ShopGrid.StorageCell) info = StorageDoor.Summary();
            var printerDeco = Game.Decos.Find(d => d.Type == Furniture.Printer3D && d.Cell == cell);
            if (printerDeco != null)
            {
                var printer = printerDeco.GetComponent<Printer3D>();
                info = printer.Printing != null
                    ? $"3D-Drucker: druckt {printer.Printing.Product.Name} ({Mathf.RoundToInt(printer.Progress * 100f)} %)\nFilament: {printer.Filament} Spulen"
                    : printer.Filament > 0 ? $"3D-Drucker: bereit\nFilament: {printer.Filament} Spulen"
                    : "3D-Drucker: kein Filament mehr!\nAm Computer unter „Ware“ nachbestellen";
            }
            var gameTable = Game.Decos.Find(d => d.Type == Furniture.GameTable && d.Cell == cell);
            if (gameTable != null)
            {
                int seats = gameTable.GetComponent<GameTable>().Seats().Count;
                info = seats >= 2 ? "Spieltisch: 2 Turnierplätze (links und rechts)"
                    : $"Spieltisch: nur {seats} Platz – links/rechts ist etwas im Weg (Möbel, Wand oder Kassen-Schlange). Morgens mit [B] verschieben!";
            }
            var tableDeco = Game.Decos.Find(d => d.Type == Furniture.Table && d.Cell == cell);
            if (tableDeco != null)
            {
                int trash = tableDeco.GetComponent<TableTrash>().Count;
                info = trash == 0 ? "Tisch: sauber"
                    : trash >= TableTrash.TooMessy ? $"Tisch: {trash}× Müll – so setzt sich keiner hin!"
                    : $"Tisch: {trash}× Müll";
            }
            var binDeco = Game.Decos.Find(d => d.Type == Furniture.TrashBin && d.Cell == cell);
            if (binDeco != null)
            {
                var bin = binDeco.GetComponent<TrashBin>();
                info = bin.IsFull ? "Mülleimer: voll! Sack mit [E] herausnehmen" : $"Mülleimer: {bin.Fill} / {TrashBin.Capacity}";
            }
            if (info == null)
            {
                foreach (var box in DeliveryBox.Existing)
                    if (box.enabled && ShopGrid.WorldToCell(box.transform.position) == cell)
                        info = box.IsSingle ? $"1× {box.Product.Name}" : $"Karton: {box.Amount}× {box.Product.Name}";
            }
            if (info == null) return;

            float scale = Screen.height / UiKit.VirtualHeight;
            var content = new GUIContent(info);
            var size = UiKit.Bold.CalcSize(content);
            var rect = new Rect(mouse.x / scale + 14f, (Screen.height - mouse.y) / scale + 10f, size.x + 16f, size.y + 8f);
            GUI.Box(rect, GUIContent.none, panel);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 4f, size.x + 4f, size.y), content, UiKit.Bold);
        }

        /// <summary>Sprechblasen über Kunden, die gerade etwas sagen (verärgert, zu teuer …).</summary>
        void DrawSpeechBubbles()
        {
            var camera = Camera.main;
            if (camera == null) return;
            float scale = Screen.height / UiKit.VirtualHeight;
            var style = new GUIStyle(UiKit.Bold) { alignment = TextAnchor.MiddleCenter, wordWrap = false, fontSize = 13 };
            style.normal.textColor = new Color(0.15f, 0.08f, 0.06f);
            foreach (var customer in Customer.Active)
            {
                string speech = customer.Speech;
                if (speech == null) continue;
                var screen = camera.WorldToScreenPoint(customer.transform.position + new Vector3(0f, 1.35f, 0f));
                float x = screen.x / scale, y = (Screen.height - screen.y) / scale;
                var size = style.CalcSize(new GUIContent(speech));
                var bubble = new Rect(x - size.x / 2f - 8f, y - size.y - 10f, size.x + 16f, size.y + 8f);
                UiKit.DrawRect(new Rect(bubble.x - 2f, bubble.y - 2f, bubble.width + 4f, bubble.height + 4f), new Color(0.15f, 0.08f, 0.06f));
                UiKit.DrawRect(bubble, new Color(1f, 0.97f, 0.9f));
                // Kleine Spitze zum Kopf
                UiKit.DrawRect(new Rect(x - 3f, bubble.yMax, 6f, 4f), new Color(1f, 0.97f, 0.9f));
                UiKit.DrawRect(new Rect(x - 1f, bubble.yMax + 4f, 2f, 3f), new Color(1f, 0.97f, 0.9f));
                GUI.Label(bubble, speech, style);
            }
        }

        void DrawPause(float width)
        {
            UiKit.DarkenScreen(width, 0.4f);
            switch (pauseScreen)
            {
                case PauseScreen.Main:
                    UiKit.BeginWindow(UiKit.Centered(width, 320f, ShowTestTools ? 480f : 390f));
                    GUILayout.Label("Pause", title);
                    GUILayout.Space(6f);
                    if (UiKit.BigButtonLayout("Weiter [Esc]")) Game.PauseOpen = false;
                    if (UiKit.BigButtonLayout("Speichern")) Game.SaveGame();
                    if (UiKit.BigButtonLayout("Einstellungen")) pauseScreen = PauseScreen.Settings;
                    if (UiKit.BigButtonLayout("Hilfe")) pauseScreen = PauseScreen.Help;
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

                case PauseScreen.Help:
                    if (HelpUI.Draw(width)) pauseScreen = PauseScreen.Main;
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

        /// <summary>
        /// Schlanke Kopfzeile: Tag & Uhrzeit, Geld, Ruf und ein XP-Balken. Details (Gemütlichkeit, genaue XP)
        /// erscheinen erst, wenn die Maus darüber ist. Turnier und Tagesereignis in einer kleinen zweiten Zeile.
        /// </summary>
        void DrawTopBar(float width)
        {
            string phase;
            switch (Game.Phase)
            {
                case GamePhase.Preparation: phase = "geschlossen"; break;
                case GamePhase.Open: phase = "geöffnet"; break;
                case GamePhase.Closing: phase = "Ladenschluss"; break;
                default: phase = "Feierabend"; break;
            }

            int level = Game.StoreLevel;
            bool maxLevel = level >= StoreLevels.Max;
            int from = StoreLevels.XpFor(level), to = maxLevel ? from : StoreLevels.XpFor(level + 1);
            float xpFill = maxLevel ? 1f : Mathf.Clamp01((Game.Xp - from) / (float)Mathf.Max(1, to - from));
            string xpTip = maxLevel
                ? $"Store-Level {level}: {StoreLevels.Title(level)} – höchstes Level!"
                : $"Store-Level {level}: {StoreLevels.Title(level)}\n{Game.Xp} / {to} XP bis Level {level + 1}" +
                  (Game.Xp >= to && !Game.ChapterComplete ? "\nXP reichen – jetzt noch die Kapitel-Aufgaben!" : "");

            var dayStyle = bold;
            // Geld grün, wenn der Tag bisher Gewinn bringt (zählt für „Tage mit Gewinn“), rot bei Verlust.
            int todayProfit = Game.Today.Income - Game.Today.Expenses;
            var moneyColor = todayProfit > 0 ? UiKit.Good : todayProfit < 0 ? UiKit.Bad : UiKit.Gold;
            var moneyStyle = new GUIStyle(bold) { normal = { textColor = moneyColor } };
            string moneyTip = $"Heute: Einnahmen {Money.Format(Game.Today.Income)} · Ausgaben {Money.Format(Game.Today.Expenses)}\n" +
                              (todayProfit >= 0 ? $"Gewinn bisher: {Money.Format(todayProfit)}" : $"Verlust bisher: {Money.Format(-todayProfit)}");
            var items = new (string text, GUIStyle style, string tip)[]
            {
                ($"Tag {Game.Day}  {Game.ClockText}", dayStyle, null),
                (phase, text, null),
                (Money.Format(Game.MoneyCents), moneyStyle, moneyTip),
                ($"Ruf: {Game.ReputationName}", text, $"Ruf {Game.Reputation} · Gemütlichkeit {Game.Coziness}\nZufriedene Kunden und Deko bringen Ruf."),
                ($"Level {level}", bold, xpTip),
            };

            const float gap = 16f, barWidth = 90f, padding = 12f, height = 32f;
            float total = padding * 2f + barWidth + 8f;
            foreach (var item in items) total += item.style.CalcSize(new GUIContent(item.text)).x + gap;

            var area = new Rect(Margin, Margin, total, height);
            GUI.Box(area, GUIContent.none, panel);
            float x = area.x + padding;
            string hoverTip = null;
            var mouse = Event.current.mousePosition;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var size = item.style.CalcSize(new GUIContent(item.text));
                var rect = new Rect(x, area.y + (height - size.y) / 2f, size.x, size.y);
                GUI.Label(rect, item.text, item.style);
                x += size.x;
                if (i == items.Length - 1)
                {
                    // XP-Balken direkt hinter „Level X“
                    var bar = new Rect(x + 8f, area.y + height / 2f - 4f, barWidth, 8f);
                    UiKit.DrawRect(bar, new Color(0f, 0f, 0f, 0.5f));
                    bool ready = !maxLevel && Game.Xp >= to;
                    UiKit.DrawRect(new Rect(bar.x, bar.y, bar.width * xpFill, bar.height), ready ? UiKit.Good : UiKit.Gold);
                    rect = new Rect(rect.x, area.y, bar.xMax - rect.x, height);
                }
                if (item.tip != null && rect.Contains(mouse)) hoverTip = item.tip;
                x += gap;
                if (i < items.Length - 1) UiKit.DrawRect(new Rect(x - gap / 2f, area.y + 9f, 1f, height - 18f), new Color(1f, 1f, 1f, 0.15f));
            }

            // Zweite, kleine Zeile nur wenn heute etwas los ist.
            string status = Game.Tournament.StatusLine;
            string eventLine = DailyEvents.Short(Game.TodayEvent);
            if (eventLine != null) status = status == null ? $"Heute: {eventLine}" : $"{status}   ·   Heute: {eventLine}";
            if (status != null)
            {
                var statusSize = hint.CalcSize(new GUIContent(status));
                var statusRect = new Rect(Margin, area.yMax + 4f, statusSize.x + 20f, 24f);
                GUI.Box(statusRect, GUIContent.none, panel);
                GUI.Label(statusRect, status, hint);
            }

            topBarTip = hoverTip;
        }

        /// <summary>Tooltip der Kopfzeile – ganz zum Schluss gezeichnet, damit nichts darüber liegt.</summary>
        string topBarTip;

        void DrawTopBarTip(float width)
        {
            if (topBarTip == null) return;
            var mouse = Event.current.mousePosition;
            var tipContent = new GUIContent(topBarTip);
            float tipWidth = Mathf.Min(320f, text.CalcSize(tipContent).x + 4f);
            float tipHeight = text.CalcHeight(tipContent, tipWidth);
            var tip = new Rect(Mathf.Clamp(mouse.x + 12f, Margin, width - tipWidth - 30f), mouse.y + 18f, tipWidth + 20f, tipHeight + 12f);
            UiKit.DrawRect(tip, new Color(0.16f, 0.1f, 0.08f, 0.97f));
            GUI.Box(tip, GUIContent.none, panel);
            GUI.Label(new Rect(tip.x + 10f, tip.y + 6f, tipWidth, tipHeight), tipContent, text);
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
            float innerWidth = PanelWidth - 20f;
            var wrap = new GUIStyle(text) { wordWrap = true };
            var titleStyle = new GUIStyle(bold) { wordWrap = true };

            // Höhen vorher messen, damit lange Titel und Aufgaben umbrechen statt abgeschnitten zu werden.
            string stageTitle = chapters.Stage.Title;
            float titleHeight = titleStyle.CalcHeight(new GUIContent(stageTitle), innerWidth);
            var rowHeights = new float[rows];
            float rowsHeight = 0f;
            for (int i = 0; i < rows; i++)
            {
                rowHeights[i] = Mathf.Max(22f, wrap.CalcHeight(new GUIContent(QuestLine(open[i])), innerWidth));
                rowsHeight += rowHeights[i];
            }

            var area = new Rect(width - PanelWidth - Margin, Margin, PanelWidth, 32f + titleHeight + rowsHeight + 8f);
            GUI.Box(area, GUIContent.none, panel);
            int stage = Mathf.Min(chapters.StageIndex + 1, chapters.Stages.Count);
            GUI.Label(new Rect(area.x + 10f, area.y + 6f, area.width - 100f, 22f), $"Kapitel {chapters.Chapter} · Etappe {stage}/{chapters.Stages.Count}", hint.WithAlignment(TextAnchor.MiddleLeft));
            GUI.Label(new Rect(area.xMax - 92f, area.y + 6f, 52f, 22f), $"{open.Count} offen", hint);
            var toggle = new Rect(area.xMax - 36f, area.y + 5f, 28f, 24f);
            if (GUI.Button(toggle, GUIContent.none, button)) ToggleQuests();
            DrawToggleArrow(toggle, questsExpanded);

            GUI.color = UiKit.Gold;
            GUI.Label(new Rect(area.x + 10f, area.y + 28f, innerWidth, titleHeight), stageTitle, titleStyle);
            GUI.color = Color.white;

            float y = area.y + 30f + titleHeight;
            for (int i = 0; i < rows; i++)
            {
                GUI.color = open[i].Done ? UiKit.Good : Color.white;
                GUI.Label(new Rect(area.x + 10f, y, innerWidth, rowHeights[i]), QuestLine(open[i]), wrap);
                GUI.color = Color.white;
                y += rowHeights[i];
            }
            return area.yMax;
        }

        static string QuestLine(Quest quest)
        {
            string progress = quest.Target > 1 ? $" ({quest.Progress}/{quest.Target})" : "";
            return $"{(quest.Done ? "[x]" : "[  ]")} {quest.Text}{progress}";
        }

        /// <summary>Kleines Dreieck im Aufklapp-Knopf (aus Rechtecken, weil die Schrift keine Pfeilzeichen hat).</summary>
        static void DrawToggleArrow(Rect button, bool up)
        {
            float cx = button.center.x, cy = button.center.y;
            for (int i = 0; i < 5; i++)
            {
                float half = 6f - i * 1.3f;
                float y = up ? cy + 3f - i * 1.6f : cy - 4f + i * 1.6f;
                UiKit.DrawRect(new Rect(cx - half, y, half * 2f, 1.6f), Color.white);
            }
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

            if (Game.BuildMode && BuildController.IsMoving)
            {
                line = $"Verschieben: Linksklick = Hier hinstellen   [V] = Verkaufen ({Money.Format(BuildController.SellPrice(BuildController.MovingType))})   Rechtsklick = Zurück";
            }
            else if (Game.BuildMode && BuildController.MoveOnly)
            {
                line = "Bau-Modus: Möbel anklicken = Verschieben   Maus drauf + [V] = Verkaufen (80 %)   Rechtsklick / [B] = Fertig";
            }
            else if (Game.BuildMode)
            {
                var current = BuildController.Current;
                var def = FurnitureCatalog.Get(current);
                string item = $"{def.Name} ({Game.ToPlace(current)})";
                bool canSwitch = Game.TotalToPlace > Game.ToPlace(current);
                string where = def.Placement == Placement.Wall ? " an die hintere Wand" : "";
                string move = BuildController.CanMoveNow ? "Klick auf Möbel = Verschieben   " : "";
                line = $"Bau-Modus: {item}{where}   Linksklick = Aufstellen   {move}" +
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
                    line = $"[C] Sammelbuch ({Game.Collection.DiscoveredCount}/{CardDatabase.All.Count})";
            }

            if (line == null) return;
            GUI.Box(new Rect((width - 640f) / 2f, VirtualHeight - 44f - Margin, 640f, 40f), line, prompt);
        }

        /// <summary>Ein großes Fenster (Computer, Sammelbuch …) ist offen – das Tutorial weicht dann nach unten aus.</summary>
        public static bool BigWindowOpen { get; private set; }

        void DrawComputer(float width)
        {
            // Höchstens so hoch wie der Bildschirm; der Inhalt lässt sich scrollen.
            float height = Mathf.Min(VirtualHeight - 100f, 380f);
            var area = new Rect((width - 580f) / 2f, 60f, 580f, height);
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 16f, area.y + 12f, area.width - 32f, area.height - 24f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Computer", title);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Geld: {Money.Format(Game.MoneyCents)}", bold);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            string[] tabs = { "Ware", "Einrichtung", "Preise", "Team", "Level", Game.SkillPoints > 0 ? $"Talente ({Game.SkillPoints})" : "Talente" };
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
            else if (computerTab == 3) DrawTeam();
            else if (computerTab == 4) DrawLevelList();
            else if (computerTab == 5) DrawSkills();
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
            if (options == Catalog.Furnishings)
            {
                DrawStorageOffer();
                DrawExtension();
            }
            foreach (var option in options)
            {
                bool locked = option.MinLevel > Game.StoreLevel;
                GUILayout.BeginHorizontal();
                GUI.color = locked ? UiKit.Dimmed : Color.white;
                GUILayout.BeginVertical();
                GUILayout.Label($"{option.Name} – {Money.Format(Game.CostOf(option))}", bold);
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
                    GUI.enabled = Game.MoneyCents >= Game.CostOf(option);
                    if (Button("Kaufen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.Order(option);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(8f);
            }
        }

        /// <summary>Lagerraum: eine Tür in der linken Wand, kein Möbelstück zum Aufstellen.</summary>
        void DrawStorageOffer()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label($"Lagerraum – {Money.Format(GameManager.StorageCost)}", bold);
            GUILayout.Label(Game.StorageBuilt
                ? "Gebaut: die Tür in der linken Wand."
                : "Tür in der linken Wand: Kartons einlagern, du und dein Mitarbeiter holt dort Nachschub", text);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (Game.StorageBuilt)
                GUILayout.Label("gebaut", hint, GUILayout.Width(90f), GUILayout.Height(36f));
            else
            {
                GUI.enabled = Game.MoneyCents >= GameManager.StorageCost;
                if (Button("Bauen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.BuildStorage();
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        /// <summary>Ladenerweiterung: kein Möbelstück, sondern ein Umbau – deshalb ein eigener Eintrag.</summary>
        void DrawExtension()
        {
            bool locked = Game.StoreLevel < GameManager.ExtensionLevel;
            GUILayout.BeginHorizontal();
            GUI.color = locked ? UiKit.Dimmed : Color.white;
            GUILayout.BeginVertical();
            GUILayout.Label($"Ladenerweiterung – {Money.Format(GameManager.ExtensionCost)}", bold);
            GUILayout.Label(ShopGrid.Expanded
                ? "Der Laden ist vergrößert."
                : "Der Laden wird nach rechts 10 Felder breiter · Umbau nur morgens vor Ladenöffnung", text);
            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            if (ShopGrid.Expanded)
                GUILayout.Label("gebaut", hint, GUILayout.Width(90f), GUILayout.Height(36f));
            else if (locked)
                GUILayout.Label($"ab Level {GameManager.ExtensionLevel}", hint, GUILayout.Width(90f), GUILayout.Height(36f));
            else
            {
                GUI.enabled = Game.MoneyCents >= GameManager.ExtensionCost && Game.Phase == GamePhase.Preparation;
                if (Button("Bauen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.BuildExtension();
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        void DrawTeam()
        {
            if (Game.StoreLevel < 3)
            {
                GUILayout.Label("Ab Store-Level 3 kannst du einen Mitarbeiter einstellen.", text);
                return;
            }
            var staff = Game.Staff;
            GUILayout.Label(staff == null
                ? $"{Employee.Name} sucht einen Job! Lohn: {Money.Format(Employee.DailyWage)} pro Tag (wird abends abgezogen)."
                : $"{Employee.Name} arbeitet bei dir · Lohn {Money.Format(Employee.DailyWage)} pro Tag", text);
            GUILayout.Space(8f);
            GUILayout.Label("Aufgabe:", bold);
            if (Button(staff != null && staff.Job == Employee.Role.Register ? "[x] An der Kasse kassieren und Karten ankaufen" : "An der Kasse kassieren und Karten ankaufen", GUILayout.Height(32f)))
                Game.Hire(Employee.Role.Register);
            if (Button(staff != null && staff.Job == Employee.Role.Restock ? "[x] Regale aus Kartons auffüllen" : "Regale aus Kartons auffüllen", GUILayout.Height(32f)))
                Game.Hire(Employee.Role.Restock);
            if (staff != null)
            {
                GUILayout.Space(8f);
                if (Button("Entlassen", GUILayout.Height(28f))) Game.Fire();
            }
        }

        /// <summary>Fähigkeiten: ein Punkt pro Level-Aufstieg, frei wählbar.</summary>
        void DrawSkills()
        {
            int points = Game.SkillPoints;
            GUILayout.Label(points > 0
                ? $"Du hast {points} freie(n) Fähigkeitspunkt(e). Mit jedem Level-Aufstieg kommt einer dazu."
                : "Mit jedem Level-Aufstieg bekommst du einen Fähigkeitspunkt.", hint);
            foreach (var skill in Skills.All)
            {
                bool learned = Game.Skills.Has(skill.Id);
                GUILayout.BeginHorizontal();
                GUI.color = learned ? UiKit.Good : Color.white;
                GUILayout.BeginVertical();
                GUILayout.Label(skill.Name, bold);
                GUILayout.Label(skill.Description, text);
                GUILayout.EndVertical();
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                if (learned)
                    GUILayout.Label("gelernt", hint, GUILayout.Width(90f), GUILayout.Height(36f));
                else
                {
                    GUI.enabled = points > 0;
                    if (Button("Lernen", GUILayout.Width(90f), GUILayout.Height(36f))) Game.LearnSkill(skill.Id);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }
        }

        /// <summary>Alle Store-Level mit ihren Freischaltungen: erreicht, aktuell, noch gesperrt.</summary>
        void DrawLevelList()
        {
            int current = Game.StoreLevel;
            GUILayout.Label($"Laden-Erfahrung: {Game.Xp} XP", hint);
            if (ShowTestTools && current < StoreLevels.Max)
            {
                // Gleiche doppelte Nachfrage wie im Pause-Menü.
                GUI.color = UiKit.Gold;
                if (Button($"TEST: Level {current + 1} freischalten", GUILayout.Height(30f)))
                {
                    Game.ComputerOpen = false;
                    Game.PauseOpen = true;
                    pauseScreen = PauseScreen.TestUnlock;
                }
                GUI.color = Color.white;
                GUILayout.Space(4f);
            }
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
                if (product.MinLevel > Game.StoreLevel || product.Category == ProductCategory.Promo ||
                    product.Category == ProductCategory.Supply || product.Category == ProductCategory.Trash) continue;
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
                float euros = GUILayout.HorizontalSlider(price / 100f, GameManager.MinPriceFor(product) / 100f, GameManager.MaxPriceFor(product) / 100f, GUILayout.Height(20f));
                int newPrice = Mathf.RoundToInt(euros * 10f) * 10;
                if (newPrice != price) Game.SetPrice(product, newPrice);
                GUILayout.Label(Money.Format(price), bold, GUILayout.Width(70f));
                GUILayout.EndHorizontal();

                GUI.color = profit > 0 ? UiKit.Good : UiKit.Bad;
                string unit = product.Category == ProductCategory.Pack ? "Pack" : "Stück";
                GUILayout.Label($"Gewinn pro {unit}: {Money.Format(profit)}   ·   {PriceVerdict(price, product.MarketValue)}", text);
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
            SummaryRow("Ausgaben (Bestellungen, Lohn)", Money.Format(stats.Expenses));
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
