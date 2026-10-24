using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Pack öffnen (Karten durchklicken, beste zuletzt), Sammelbuch und Vitrine einräumen.</summary>
    public class CardScreensUI : MonoBehaviour
    {
        const float FlyOutSeconds = 0.25f;

        // Pack öffnen: -1 = Pack noch zu, 0–9 = aufgedeckte Karte, 10 = Übersicht.
        int revealIndex = -1;
        float lastRevealTime;
        int packOpenedFrame;
        List<CardDef> currentPack;

        int selectedCard = -1;
        TcgGame collectionGame = TcgGame.Monstera;
        Vector2 collectionScroll;

        int pickingSlot = -1;
        Vector2 pickerScroll;
        Vector2 showcaseScroll;

        static GameManager Game => GameManager.Instance;

        void Update()
        {
            var pack = Game.OpenedPack;
            if (pack != currentPack)
            {
                currentPack = pack;
                revealIndex = -1;
                packOpenedFrame = Time.frameCount;
            }
            if (pack == null)
            {
                if (Game.OpenShowcase == null) pickingSlot = -1;
                return;
            }

            if (revealIndex < pack.Count && Time.frameCount != packOpenedFrame && GameInput.ConfirmPressed) Reveal();
            if (revealIndex >= pack.Count && GameInput.ConsumeCancel()) Game.ClosePack();
        }

        void Reveal()
        {
            revealIndex++;
            lastRevealTime = Time.unscaledTime;
            if (revealIndex >= currentPack.Count)
            {
                SoundManager.Play(Sfx.UiClick);
                return;
            }
            switch (currentPack[revealIndex].Rarity)
            {
                case Rarity.Ultra: SoundManager.Play(Sfx.ShinyCard); break;
                case Rarity.Rare: SoundManager.Play(Sfx.RareCard); break;
                default: SoundManager.Play(Sfx.CardFlip); break;
            }
        }

        void OnGUI()
        {
            if (Game.Phase == GamePhase.MainMenu || Game.Phase == GamePhase.CharacterSelect) return;
            if (Game.OpenedPack == null && !Game.CollectionOpen && Game.OpenShowcase == null) return;

            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.6f);

            if (Game.OpenedPack != null) DrawPack(width);
            else if (Game.OpenShowcase != null) DrawShowcase(width);
            else DrawCollection(width);
        }

        // ---------- Pack öffnen ----------

        void DrawPack(float width)
        {
            var pack = Game.OpenedPack;
            var cardSize = new Vector2(216f, 288f);
            var center = new Rect((width - cardSize.x) / 2f, 70f, cardSize.x, cardSize.y);

            if (revealIndex < 0)
            {
                var product = Game.OpenedPackProduct ?? Catalog.MonsteraPack;
                GUI.Label(new Rect(0f, 24f, width, 36f), product.Name, UiKit.BigTitle);
                UiKit.DrawSprite(Art.Get(product.SpriteKey), new Rect(center.x - 20f, center.y, center.width + 40f, center.height + 40f));
                GUI.Label(new Rect(0f, center.yMax + 60f, width, 24f), "Klicken oder [Leertaste] zum Aufreißen", UiKit.Hint);
                return;
            }

            if (revealIndex >= pack.Count)
            {
                DrawPackSummary(width, pack);
                return;
            }

            GUI.Label(new Rect(0f, 20f, width, 30f), $"Karte {revealIndex + 1} von {pack.Count}", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter));

            // Verdeckter Stapel der restlichen Karten dahinter.
            int remaining = pack.Count - revealIndex - 1;
            for (int i = Mathf.Min(remaining, 3); i > 0; i--)
                CardUI.DrawBack(new Rect(center.x + i * 6f, center.y + i * 4f, center.width, center.height));

            var card = pack[revealIndex];
            CardUI.Draw(center, card);

            // Vorherige Karte fliegt nach links raus.
            float t = (Time.unscaledTime - lastRevealTime) / FlyOutSeconds;
            if (revealIndex > 0 && t < 1f)
            {
                var previous = new Rect(center.x - t * 420f, center.y - t * 40f, center.width, center.height);
                GUI.color = new Color(1f, 1f, 1f, 1f - t);
                CardUI.Draw(previous, pack[revealIndex - 1]);
                GUI.color = Color.white;
            }

            string info = CardDatabase.RarityName(card.Rarity);
            if (Game.OpenedPackNew[revealIndex]) info += "  ·  NEU!";
            GUI.color = CardDatabase.RarityColor(card.Rarity);
            GUI.Label(new Rect(0f, center.yMax + 10f, width, 30f), info, UiKit.Title.WithAlignment(TextAnchor.MiddleCenter));
            GUI.color = Color.white;
            GUI.Label(new Rect(0f, center.yMax + 44f, width, 24f),
                revealIndex < pack.Count - 1 ? "Weiter: Klick / [Leertaste]" : "Zur Übersicht: Klick / [Leertaste]", UiKit.Hint);
        }

        void DrawPackSummary(float width, List<CardDef> pack)
        {
            var area = UiKit.Centered(width, 640f, 420f);
            UiKit.BeginWindow(area);
            GUILayout.Label("Dein Pack", UiKit.Title);
            int fresh = 0;
            foreach (bool isNew in Game.OpenedPackNew) if (isNew) fresh++;
            var game = pack[0].Game;
            GUILayout.Label($"{fresh} neue Karte(n) · {Catalog.GameName(game)}: {Game.Collection.DiscoveredIn(game)}/{CardDatabase.CardsPerGame} im Sammelbuch", UiKit.Text);
            GUILayout.Space(6f);

            var grid = GUILayoutUtility.GetRect(area.width - 36f, 250f);
            float cardWidth = 96f, cardHeight = 128f, gap = 8f;
            for (int i = 0; i < pack.Count; i++)
            {
                int column = i % 5, row = i / 5;
                var rect = new Rect(grid.x + column * (cardWidth + gap) + 30f, grid.y + row * (cardHeight + gap), cardWidth, cardHeight);
                CardUI.Draw(rect, pack[i], true);
                if (Game.OpenedPackNew[i]) GUI.Label(new Rect(rect.x, rect.y + 2f, rect.width, 18f), "NEU", UiKit.Hint);
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (UiKit.Button("Sammelbuch", GUILayout.Height(36f)))
            {
                Game.ClosePack();
                Game.CollectionOpen = true;
            }
            if (UiKit.Button("Fertig [Esc]", GUILayout.Height(36f))) Game.ClosePack();
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
        }

        // ---------- Sammelbuch ----------

        void DrawCollection(float width)
        {
            var area = UiKit.Centered(width, 760f, 480f);
            UiKit.BeginWindow(area);
            var collection = Game.Collection;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sammelbuch", UiKit.Title);
            foreach (var tab in CardDatabase.Games)
            {
                GUI.color = collectionGame == tab ? Color.white : UiKit.Dimmed;
                if (UiKit.Button(Catalog.GameName(tab), GUILayout.Width(110f), GUILayout.Height(28f)))
                {
                    collectionGame = tab;
                    selectedCard = -1;
                }
                GUI.color = Color.white;
            }
            if (Game.HasTwoMoons)
            {
                GUI.color = UiKit.Gold;
                if (UiKit.Button("Two Moons", GUILayout.Width(110f), GUILayout.Height(28f))) selectedCard = CardDatabase.TwoMoons.Id;
                GUI.color = Color.white;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{collection.DiscoveredIn(collectionGame)}/{CardDatabase.CardsPerGame} entdeckt", UiKit.Bold);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            collectionScroll = GUILayout.BeginScrollView(collectionScroll, GUILayout.Width(480f));
            var cards = CardDatabase.ForGame(collectionGame);
            const int columns = 6;
            float cardWidth = 66f, cardHeight = 88f, gap = 8f;
            var grid = GUILayoutUtility.GetRect(columns * (cardWidth + gap), Mathf.Ceil(cards.Count / (float)columns) * (cardHeight + gap));
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var rect = new Rect(grid.x + (i % columns) * (cardWidth + gap), grid.y + (i / columns) * (cardHeight + gap), cardWidth, cardHeight);
                if (collection.IsDiscovered(card.Id))
                {
                    CardUI.Draw(rect, card, true);
                    GUI.Label(new Rect(rect.x, rect.yMax - 20f, rect.width - 4f, 18f), $"×{collection.Count(card.Id)}", UiKit.Bold.WithAlignment(TextAnchor.LowerRight));
                    int best = collection.BestGrade(card.Id);
                    if (best > 0)
                    {
                        GUI.color = UiKit.Gold;
                        GUI.Label(new Rect(rect.x + 3f, rect.y + 1f, rect.width, 18f), $"{best}/10", UiKit.Bold);
                        GUI.color = Color.white;
                    }
                }
                else
                {
                    GUI.color = UiKit.Dimmed;
                    CardUI.DrawBack(rect);
                    GUI.color = Color.white;
                    GUI.Label(rect, $"#{card.Number:00}", UiKit.Hint);
                }
                if (selectedCard == card.Id) GUI.Box(rect, GUIContent.none);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    selectedCard = card.Id;
                    SoundManager.Play(Sfx.UiClick);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(16f);
            GUILayout.BeginVertical();
            var selected = CardDatabase.Get(selectedCard);
            if (selected == CardDatabase.TwoMoons && Game.HasTwoMoons)
            {
                var big = GUILayoutUtility.GetRect(180f, 240f, GUILayout.Width(180f), GUILayout.Height(240f));
                CardUI.Draw(big, selected);
                GUI.color = UiKit.Gold;
                GUILayout.Label("Two Moons – die Legende", UiKit.Bold);
                GUI.color = Color.white;
                GUILayout.Label("Es gibt sie nur ein einziges Mal. Sie gehört dem besten Laden der Stadt – und ist unverkäuflich.", UiKit.Text);
            }
            else if (selected != null && collection.IsDiscovered(selected.Id))
            {
                var big = GUILayoutUtility.GetRect(180f, 240f, GUILayout.Width(180f), GUILayout.Height(240f));
                CardUI.Draw(big, selected);
                GUILayout.Label($"#{selected.Number:00} {selected.Name}", UiKit.Bold);
                GUI.color = CardDatabase.RarityColor(selected.Rarity);
                GUILayout.Label(CardDatabase.RarityName(selected.Rarity), UiKit.Bold);
                GUI.color = Color.white;
                GUILayout.Label($"Marktwert: {Money.Format(selected.Value)}", UiKit.Text);
                GUILayout.Label($"Im Besitz: {collection.Count(selected.Id)}", UiKit.Text);
                int best = collection.BestGrade(selected.Id);
                if (best > 0) GUILayout.Label($"Beste Note: {best} ({Grading.Describe(best)})", UiKit.Text);
                int slabs = Game.Slabs.FindAll(slab => slab.card == selected.Id).Count;
                if (slabs > 0) GUILayout.Label($"Bewertet im Lager: {slabs}", UiKit.Text);
            }
            else
            {
                GUILayout.Label(selected != null ? "Diese Karte hast du noch nicht gezogen." : "Klick auf eine Karte für Details.", UiKit.Text);
                GUILayout.Space(8f);
                GUILayout.Label("Packs öffnest du am Pack-Tisch hinter der Theke: einen Booster aus dem Regal nehmen ([E]), hintragen und [E] drücken.", UiKit.Text);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Schließen [Esc]", GUILayout.Height(34f))) Game.CollectionOpen = false;
            UiKit.EndWindow();
        }

        // ---------- Vitrine ----------

        void DrawShowcase(float width)
        {
            var showcase = Game.OpenShowcase;
            if (pickingSlot >= 0)
            {
                DrawCardPicker(width, showcase);
                return;
            }

            var area = UiKit.Centered(width, 720f, 440f);
            UiKit.BeginWindow(area);
            GUILayout.Label(showcase.Premium ? "Premium-Vitrine" : "Vitrine", UiKit.Title);
            GUILayout.Label(showcase.Premium
                ? "Beleuchtet: Sammler zahlen hier bis zu 10 % über dem Wert. Bewertete Karten sind mehr wert."
                : "Sammler kaufen Einzelkarten. Über dem Wert kaufen weniger, darunter verkaufst du schneller.", UiKit.Hint);
            GUILayout.Space(8f);

            showcaseScroll = GUILayout.BeginScrollView(showcaseScroll);
            const int columns = 4;
            for (int slot = 0; slot < showcase.SlotCount; slot++)
            {
                if (slot % columns == 0) GUILayout.BeginHorizontal();
                DrawShowcaseSlot(showcase, slot);
                if (slot % columns == columns - 1 || slot == showcase.SlotCount - 1) GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Schließen [Esc]", GUILayout.Height(34f))) Game.OpenShowcase = null;
            UiKit.EndWindow();
        }

        void DrawShowcaseSlot(Showcase showcase, int slot)
        {
            GUILayout.BeginVertical(UiKit.Panel, GUILayout.Width(160f), GUILayout.Height(300f));
            var card = CardDatabase.Get(showcase.Cards[slot]);
            var cardRect = GUILayoutUtility.GetRect(96f, 128f, GUILayout.Width(136f), GUILayout.Height(128f));
            cardRect = new Rect(cardRect.center.x - 48f, cardRect.y, 96f, 128f);
            if (card != null)
            {
                int grade = showcase.Grades[slot];
                if (grade > 0) CardUI.DrawSlab(cardRect, card, grade);
                else CardUI.Draw(cardRect, card, true);
                GUILayout.Label(card.Name, UiKit.Bold);
                int value = showcase.ValueAt(slot);
                GUILayout.Label(grade > 0 ? $"Note {grade} · Wert {Money.Format(value)}" : $"Wert {Money.Format(value)}", UiKit.Text);

                // Preis per Schieberegler: 10 % bis 300 % vom Wert, in 10-Cent-Schritten.
                int price = showcase.Prices[slot];
                float euros = GUILayout.HorizontalSlider(price / 100f, value * 0.1f / 100f, value * 3f / 100f);
                int newPrice = Mathf.Max(10, Mathf.RoundToInt(euros * 10f) * 10);
                if (newPrice != price) showcase.Put(slot, card, newPrice, grade);
                GUILayout.Label($"Preis: {Money.Format(showcase.Prices[slot])}", UiKit.Bold);

                if (UiKit.Button("Zurücknehmen", GUILayout.Height(28f))) showcase.ReturnToOwner(slot);
            }
            else
            {
                GUI.Box(cardRect, "leer", UiKit.Hint);
                GUILayout.FlexibleSpace();
                if (UiKit.Button("Karte einlegen", GUILayout.Height(28f))) pickingSlot = slot;
            }
            GUILayout.EndVertical();
        }

        void DrawCardPicker(float width, Showcase showcase)
        {
            var area = UiKit.Centered(width, 640f, 440f);
            UiKit.BeginWindow(area);
            GUILayout.Label($"Karte für Fach {pickingSlot + 1} wählen", UiKit.Title);

            var owned = new List<CardDef>();
            foreach (var card in CardDatabase.All)
                if (Game.Collection.Count(card.Id) > 0) owned.Add(card);
            owned.Sort((a, b) => b.Rarity != a.Rarity ? b.Rarity.CompareTo(a.Rarity) : a.Number.CompareTo(b.Number));
            var slabs = new List<Slab>(Game.Slabs);
            slabs.Sort((a, b) => b.Value.CompareTo(a.Value));
            if (owned.Count == 0 && slabs.Count == 0)
            {
                GUILayout.Label("Du hast noch keine Karten. Öffne Packs am Pack-Tisch hinter der Theke.", UiKit.Text);
            }
            else
            {
                pickerScroll = GUILayout.BeginScrollView(pickerScroll);
                const int columns = 7;
                float cardWidth = 72f, cardHeight = 96f, gap = 10f;
                if (slabs.Count > 0)
                {
                    GUILayout.Label("Bewertete Karten", UiKit.Bold);
                    var slabGrid = GUILayoutUtility.GetRect(columns * (cardWidth + gap), Mathf.Ceil(slabs.Count / (float)columns) * (cardHeight + gap + 16f));
                    for (int i = 0; i < slabs.Count; i++)
                    {
                        var slab = slabs[i];
                        var rect = new Rect(slabGrid.x + (i % columns) * (cardWidth + gap), slabGrid.y + (i / columns) * (cardHeight + gap + 16f), cardWidth, cardHeight);
                        CardUI.DrawSlab(rect, slab.Card, slab.grade);
                        GUI.Label(new Rect(rect.x, rect.yMax, rect.width, 16f), Money.Format(slab.Value), UiKit.Hint);
                        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                        {
                            Game.Slabs.Remove(slab);
                            showcase.Put(pickingSlot, slab.Card, slab.Value, slab.grade);
                            pickingSlot = -1;
                            SoundManager.Play(Sfx.Stock);
                        }
                    }
                    GUILayout.Label("Karten aus der Sammlung", UiKit.Bold);
                }
                var grid = GUILayoutUtility.GetRect(columns * (cardWidth + gap), Mathf.Ceil(owned.Count / (float)columns) * (cardHeight + gap + 16f));
                for (int i = 0; i < owned.Count; i++)
                {
                    var card = owned[i];
                    var rect = new Rect(grid.x + (i % columns) * (cardWidth + gap), grid.y + (i / columns) * (cardHeight + gap + 16f), cardWidth, cardHeight);
                    CardUI.Draw(rect, card, true);
                    GUI.Label(new Rect(rect.x, rect.yMax, rect.width, 16f), $"×{Game.Collection.Count(card.Id)}", UiKit.Hint);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        Game.Collection.Remove(card.Id);
                        showcase.Put(pickingSlot, card, card.Value);
                        pickingSlot = -1;
                        SoundManager.Play(Sfx.Stock);
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Zurück", GUILayout.Height(34f))) pickingSlot = -1;
            UiKit.EndWindow();
        }
    }

    static class GUIStyleExtensions
    {
        public static GUIStyle WithAlignment(this GUIStyle style, TextAnchor alignment) =>
            new GUIStyle(style) { alignment = alignment };

        public static GUIStyle WithFontSize(this GUIStyle style, int size) =>
            new GUIStyle(style) { fontSize = size };

        public static GUIStyle WithItalic(this GUIStyle style) =>
            new GUIStyle(style) { fontStyle = FontStyle.Italic };
    }
}
