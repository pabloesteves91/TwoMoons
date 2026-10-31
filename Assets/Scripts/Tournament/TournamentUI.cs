using UnityEngine;

namespace TwoMoons
{
    /// <summary>Fenster am Turnier-Counter: Prizepool einteilen bzw. Siegerehrung mit Preisübergabe.</summary>
    public class TournamentUI : MonoBehaviour
    {
        static readonly string[] PlaceNames = { "1. Platz", "2. Platz", "3. Platz", "Teilnahme" };

        /// <summary>Siegerehrung noch nicht begonnen (eigener Wert, damit „fertig“ = -1 nicht wieder von vorn anfängt).</summary>
        const int CeremonyNotStarted = -2;
        /// <summary>Siegerehrung: welcher Platz als Nächstes seinen Preis bekommt (2 = 3. Platz … -1 = fertig).</summary>
        int ceremonyStep = CeremonyNotStarted;
        bool pickingCard;
        Vector2 scroll;

        static GameManager Game => GameManager.Instance;
        static TournamentManager Tournament => Game.Tournament;

        void Update()
        {
            if (!Game.PrizeCounterOpen)
            {
                ceremonyStep = CeremonyNotStarted;
                pickingCard = false;
            }
        }

        void OnGUI()
        {
            if (!Game.PrizeCounterOpen || Game.Phase == GamePhase.MainMenu) return;
            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.5f);

            if (Tournament.Current == TournamentManager.State.Ceremony) DrawCeremony(width);
            else if (pickingCard) DrawCardPicker(width);
            else DrawPrizepool(width);
        }

        void DrawPrizepool(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 460f, 420f));
            GUILayout.Label("Prizepool", UiKit.Title);
            GUILayout.Label(Tournament.StatusLine ?? "Turniere finden ab Level 3 jeden 2. Tag statt (Spieltisch + Turnier-Counter nötig).", UiKit.Hint);
            GUILayout.Space(8f);

            GUILayout.Label($"Packs im Prizepool: {Tournament.PrizePackTotal}", UiKit.Bold);
            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(PlaceNames[i], UiKit.Text, GUILayout.Width(110f));
                // − gibt ein Pack an die Teilnahmepreise ab, + nimmt eins von dort.
                GUI.enabled = i < 3 && Tournament.Prizes[i] > 0;
                if (UiKit.Button("−", GUILayout.Width(36f), GUILayout.Height(28f))) Tournament.MovePack(i, 3);
                GUI.enabled = true;
                GUILayout.Label($"{Tournament.Prizes[i]} Packs", UiKit.Bold, GUILayout.Width(80f));
                GUI.enabled = i < 3 && Tournament.Prizes[3] > 0;
                if (UiKit.Button("+", GUILayout.Width(36f), GUILayout.Height(28f))) Tournament.MovePack(3, i);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            var bonus = CardDatabase.Get(Tournament.BonusCard);
            GUILayout.BeginHorizontal();
            GUILayout.Label(bonus != null ? $"Bonus für Platz 1: {bonus.Name} ({CardDatabase.RarityName(bonus.Rarity)})" : "Bonus-Karte für Platz 1: keine", UiKit.Text);
            GUILayout.FlexibleSpace();
            if (bonus != null && UiKit.Button("Zurücknehmen", GUILayout.Height(28f))) Tournament.SetBonusCard(-1);
            if (bonus == null && UiKit.Button("Karte wählen", GUILayout.Height(28f))) pickingCard = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.Label($"Erwartete Spieler: {Tournament.ExpectedPlayers} (Plätze: {Tournament.Seats}) · Mehr Preise und eine gute Bonus-Karte locken mehr Spieler.", UiKit.Hint);

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Fertig [Esc]", GUILayout.Height(34f))) Game.PrizeCounterOpen = false;
            UiKit.EndWindow();
        }

        void DrawCardPicker(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 460f, 420f));
            GUILayout.Label("Bonus-Karte wählen", UiKit.Title);
            scroll = GUILayout.BeginScrollView(scroll);
            bool any = false;
            foreach (var card in CardDatabase.All)
            {
                int count = Game.Collection.Count(card.Id);
                if (count <= 0) continue;
                any = true;
                GUILayout.BeginHorizontal();
                GUI.color = CardDatabase.RarityColor(card.Rarity);
                GUILayout.Label($"{card.Name} · {CardDatabase.RarityName(card.Rarity)} · ×{count}", UiKit.Text);
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                if (UiKit.Button("Nehmen", GUILayout.Width(80f), GUILayout.Height(26f)))
                {
                    Tournament.SetBonusCard(card.Id);
                    pickingCard = false;
                }
                GUILayout.EndHorizontal();
            }
            if (!any) GUILayout.Label("Du hast noch keine Karten. Öffne Packs am Pack-Tisch.", UiKit.Text);
            GUILayout.EndScrollView();
            if (UiKit.Button("Zurück", GUILayout.Height(32f))) pickingCard = false;
            UiKit.EndWindow();
        }

        void DrawCeremony(float width)
        {
            var players = Tournament.Players;
            if (ceremonyStep == CeremonyNotStarted) ceremonyStep = Mathf.Min(2, players.Count - 1);

            UiKit.BeginWindow(UiKit.Centered(width, 460f, 400f));
            GUILayout.Label("Siegerehrung!", UiKit.BigTitle.WithFontSize(32));
            GUILayout.Space(6f);
            for (int place = 0; place < Mathf.Min(3, players.Count); place++)
            {
                bool given = place > ceremonyStep;
                GUI.color = given ? UiKit.Good : place == ceremonyStep ? UiKit.Gold : UiKit.Dimmed;
                string prize = $"{Tournament.Prizes[place]} Packs";
                if (place == 0 && Tournament.BonusCard >= 0) prize += $" + {CardDatabase.Get(Tournament.BonusCard).Name}";
                GUILayout.Label($"{PlaceNames[place]}: {players[place].Name} – {prize}{(given ? "  [x]" : "")}", UiKit.Bold);
            }
            GUI.color = Color.white;
            if (players.Count > 3) GUILayout.Label($"Alle anderen bekommen Teilnahmepreise ({Tournament.Prizes[3]} Packs).", UiKit.Text);
            if (!Tournament.HasPrizepool) GUILayout.Label("Oh nein – der Prizepool ist leer!", UiKit.Text);

            GUILayout.FlexibleSpace();
            if (ceremonyStep >= 0)
            {
                if (UiKit.BigButtonLayout($"Preis an den {PlaceNames[ceremonyStep]} übergeben"))
                {
                    SoundManager.Play(ceremonyStep == 0 ? Sfx.Fanfare : Sfx.Quest);
                    ceremonyStep--;
                }
            }
            else if (UiKit.BigButtonLayout("Turnier beenden"))
            {
                Game.PrizeCounterOpen = false;
                Tournament.CeremonyDone();
            }
            UiKit.EndWindow();
        }
    }
}
