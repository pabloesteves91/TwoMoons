using UnityEngine;

namespace TwoMoons
{
    /// <summary>Hauptmenü mit Logo, Speicherplätzen, Einstellungen, Credits – und die Figurenwahl für ein neues Spiel.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        enum MenuScreen { Main, NewGameSlots, LoadSlots, Settings, Credits, ConfirmOverwrite, ConfirmDelete }

        MenuScreen screen = MenuScreen.Main;
        int selectedSlot = -1;
        int continueSlot = -1;
        readonly SaveData[] slots = new SaveData[SaveSystem.SlotCount];

        string nameInput = "";
        int figure;

        static GameManager Game => GameManager.Instance;

        void Start() => RefreshSlots();

        void Update()
        {
            if (Game.Phase != GamePhase.MainMenu || screen == MenuScreen.Main) return;
            if (GameInput.ConsumeCancel()) Back();
        }

        void RefreshSlots()
        {
            for (int i = 0; i < slots.Length; i++) slots[i] = SaveSystem.Load(i);
            continueSlot = SaveSystem.LatestSlot();
        }

        void Back()
        {
            SoundManager.Play(Sfx.UiClick);
            screen = screen == MenuScreen.ConfirmOverwrite ? MenuScreen.NewGameSlots
                : screen == MenuScreen.ConfirmDelete ? MenuScreen.LoadSlots
                : MenuScreen.Main;
        }

        void OnGUI()
        {
            if (Game.Phase != GamePhase.MainMenu && Game.Phase != GamePhase.CharacterSelect) return;

            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.45f);

            if (Game.Phase == GamePhase.CharacterSelect)
            {
                DrawCharacterSelect(width);
                return;
            }

            switch (screen)
            {
                case MenuScreen.Main: DrawMain(width); break;
                case MenuScreen.NewGameSlots: DrawSlots(width, true); break;
                case MenuScreen.LoadSlots: DrawSlots(width, false); break;
                case MenuScreen.Settings: DrawSettings(width); break;
                case MenuScreen.Credits: DrawCredits(width); break;
                case MenuScreen.ConfirmOverwrite:
                    DrawConfirm(width, $"Platz {selectedSlot + 1} überschreiben?",
                        $"Der Spielstand von {slots[selectedSlot]?.playerName} geht dabei verloren.",
                        "Überschreiben", () => GameBootstrap.NewGame(selectedSlot));
                    break;
                case MenuScreen.ConfirmDelete:
                    DrawConfirm(width, $"Platz {selectedSlot + 1} löschen?",
                        $"Der Spielstand von {slots[selectedSlot]?.playerName} wird endgültig gelöscht.",
                        "Löschen", () =>
                        {
                            SaveSystem.Delete(selectedSlot);
                            RefreshSlots();
                            screen = MenuScreen.LoadSlots;
                        });
                    break;
            }
        }

        void DrawMain(float width)
        {
            UiKit.DrawLogo(new Rect((width - 520f) / 2f, 20f, 520f, 190f));

            // Höhe passend zur Anzahl der Knöpfe.
            int bigButtons = continueSlot >= 0 ? 4 : 3;
            var area = UiKit.Centered(width, 300f, 34f + bigButtons * 46f + 42f, 222f);
            UiKit.BeginWindow(area);

            if (continueSlot >= 0 && UiKit.BigButtonLayout($"Fortsetzen ({slots[continueSlot].playerName})"))
                GameBootstrap.LoadGame(continueSlot);
            if (UiKit.BigButtonLayout("Neues Spiel")) screen = MenuScreen.NewGameSlots;

            GUI.enabled = HasAnySave();
            if (UiKit.BigButtonLayout("Laden")) screen = MenuScreen.LoadSlots;
            GUI.enabled = true;

            if (UiKit.BigButtonLayout("Einstellungen")) screen = MenuScreen.Settings;
            GUILayout.BeginHorizontal();
            if (UiKit.Button("Credits", GUILayout.Height(34f))) screen = MenuScreen.Credits;
            if (UiKit.Button("Beenden", GUILayout.Height(34f))) UiKit.QuitGame();
            GUILayout.EndHorizontal();

            UiKit.EndWindow();
        }

        bool HasAnySave()
        {
            foreach (var slot in slots) if (slot != null) return true;
            return false;
        }

        void DrawSlots(float width, bool newGame)
        {
            var area = UiKit.Centered(width, 520f, 400f);
            UiKit.BeginWindow(area);
            GUILayout.Label(newGame ? "Neues Spiel – Speicherplatz wählen" : "Spielstand laden", UiKit.Title);
            GUILayout.Space(8f);

            for (int i = 0; i < slots.Length; i++)
            {
                var data = slots[i];
                GUILayout.BeginHorizontal(UiKit.Panel, GUILayout.Height(80f));

                var iconRect = GUILayoutUtility.GetRect(64f, 64f, GUILayout.Width(64f), GUILayout.Height(64f));
                if (data != null) UiKit.DrawSprite(Art.Get($"player{data.figure + 1}"), iconRect);

                GUILayout.BeginVertical();
                GUILayout.Label($"Platz {i + 1}", UiKit.Hint);
                if (data != null)
                {
                    GUILayout.Label(data.playerName, UiKit.Bold);
                    GUILayout.Label($"Tag {data.day} · Kapitel {data.chapter} · {data.SavedAt:dd.MM.yyyy HH:mm}", UiKit.Text);
                }
                else
                {
                    GUILayout.Label("Leerer Platz", UiKit.Text);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();

                if (newGame)
                {
                    if (UiKit.Button(data == null ? "Starten" : "Überschreiben", GUILayout.Width(130f), GUILayout.Height(40f)))
                    {
                        selectedSlot = i;
                        if (data == null) GameBootstrap.NewGame(i);
                        else screen = MenuScreen.ConfirmOverwrite;
                    }
                }
                else if (data != null)
                {
                    if (UiKit.Button("Laden", GUILayout.Width(90f), GUILayout.Height(40f))) GameBootstrap.LoadGame(i);
                    if (UiKit.Button("Löschen", GUILayout.Width(80f), GUILayout.Height(40f)))
                    {
                        selectedSlot = i;
                        screen = MenuScreen.ConfirmDelete;
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Zurück [Esc]", GUILayout.Height(34f))) screen = MenuScreen.Main;
            UiKit.EndWindow();
        }

        void DrawSettings(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 400f, 260f));
            GUILayout.Label("Einstellungen", UiKit.Title);
            GUILayout.Space(10f);
            UiKit.SettingsContent();
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Zurück [Esc]", GUILayout.Height(34f))) screen = MenuScreen.Main;
            UiKit.EndWindow();
        }

        void DrawCredits(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 440f, 300f));
            GUILayout.Label("Credits", UiKit.Title);
            GUILayout.Space(10f);
            GUILayout.Label("TwoMoons – ein Spiel von …", UiKit.Bold);
            GUILayout.Space(6f);
            GUILayout.Label("Idee, Story & Grafik: …", UiKit.Text);
            GUILayout.Label("Programmierung: … mit Claude", UiKit.Text);
            GUILayout.Label("Musik & Sounds: Platzhalter (8-Bit, per Code erzeugt)", UiKit.Text);
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Zurück [Esc]", GUILayout.Height(34f))) screen = MenuScreen.Main;
            UiKit.EndWindow();
        }

        void DrawConfirm(float width, string question, string detail, string confirmLabel, System.Action onConfirm)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 420f, 190f));
            GUILayout.Label(question, UiKit.Title);
            GUILayout.Label(detail, UiKit.Text);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (UiKit.Button("Abbrechen", GUILayout.Height(36f))) Back();
            if (UiKit.Button(confirmLabel, GUILayout.Height(36f))) onConfirm();
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
        }

        void DrawCharacterSelect(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 520f, 400f), 20f);

            GUILayout.Label($"Neues Spiel – Platz {Game.CurrentSlot + 1}", UiKit.Title);
            GUILayout.Label("Ein TCG-Laden, ein Traum – und zwei Monde.", UiKit.Text);
            GUILayout.Space(14f);

            GUILayout.Label("Wie heißt du?", UiKit.Bold);
            nameInput = GUILayout.TextField(nameInput, 20, GUILayout.Height(28f));
            GUILayout.Space(14f);

            GUILayout.Label("Wähle deine Figur:", UiKit.Bold);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 3; i++)
            {
                bool selected = figure == i;
                GUI.color = selected ? Color.white : UiKit.Dimmed;
                if (UiKit.Button(GUIContent.none, UiKit.ButtonStyle, GUILayout.Width(110f), GUILayout.Height(110f))) figure = i;
                var buttonRect = GUILayoutUtility.GetLastRect();
                UiKit.DrawSprite(Art.Get($"player{i + 1}"), new Rect(buttonRect.x + 15f, buttonRect.y + 10f, 80f, 80f));
                if (selected) GUI.Label(new Rect(buttonRect.x, buttonRect.yMax - 22f, buttonRect.width, 20f), "ausgewählt", UiKit.Hint);
                GUI.color = Color.white;
                GUILayout.Space(10f);
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (UiKit.Button("Zurück", GUILayout.Width(140f), GUILayout.Height(40f))) GameBootstrap.ShowMainMenu();
            GUILayout.Space(10f);
            GUI.enabled = nameInput.Trim().Length > 0;
            if (UiKit.Button("Los geht's!", GUILayout.Height(40f))) Game.StartGame(nameInput, figure);
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            UiKit.EndWindow();
        }
    }
}
