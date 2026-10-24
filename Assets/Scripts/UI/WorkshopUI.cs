using UnityEngine;

namespace TwoMoons
{
    /// <summary>Fenster für den 3D-Drucker (Modell wählen), den Ankauf an der Kasse und die Grading-Station.</summary>
    public class WorkshopUI : MonoBehaviour
    {
        string counterMessage;
        Vector2 gradingScroll;

        static GameManager Game => GameManager.Instance;

        void OnGUI()
        {
            if (Game.Phase == GamePhase.MainMenu) return;
            if (Game.OpenPrinter == null && Game.SellerOffer == null && Game.OpenGrading == null && Game.GradeReveal == null)
            {
                counterMessage = null;
                return;
            }
            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.5f);
            if (Game.SellerOffer != null) DrawOffer(width);
            else if (Game.GradeReveal != null) DrawGradeReveal(width);
            else if (Game.OpenGrading != null) DrawGrading(width);
            else DrawPrinter(width);
        }

        void DrawGrading(float width)
        {
            var station = Game.OpenGrading;
            UiKit.BeginWindow(UiKit.Centered(width, 640f, 440f));
            GUILayout.Label("Grading-Station", UiKit.Title);
            GUILayout.Label($"Karte wählen · {Money.Format(Grading.Cost)} pro Karte · Prüfung dauert 1 Spielstunde · " +
                            "Note 10 ×5, 9 ×3, 8 ×2, 7 ×1,5", UiKit.Hint);
            GUILayout.Space(6f);

            var owned = new System.Collections.Generic.List<CardDef>();
            foreach (var card in CardDatabase.All)
                if (Game.Collection.Count(card.Id) > 0) owned.Add(card);
            owned.Sort((a, b) => b.Value != a.Value ? b.Value.CompareTo(a.Value) : a.Number.CompareTo(b.Number));
            if (owned.Count == 0)
            {
                GUILayout.Label("Du hast keine Karten in der Sammlung. Öffne Packs am Pack-Tisch.", UiKit.Text);
            }
            else
            {
                gradingScroll = GUILayout.BeginScrollView(gradingScroll);
                const int columns = 7;
                float cardWidth = 72f, cardHeight = 96f, gap = 10f;
                var grid = GUILayoutUtility.GetRect(columns * (cardWidth + gap), Mathf.Ceil(owned.Count / (float)columns) * (cardHeight + gap + 30f));
                for (int i = 0; i < owned.Count; i++)
                {
                    var card = owned[i];
                    var rect = new Rect(grid.x + (i % columns) * (cardWidth + gap), grid.y + (i / columns) * (cardHeight + gap + 30f), cardWidth, cardHeight);
                    CardUI.Draw(rect, card, true);
                    GUI.Label(new Rect(rect.x, rect.yMax, rect.width, 16f), $"×{Game.Collection.Count(card.Id)} · {Money.Format(card.Value)}", UiKit.Hint);
                    int best = Game.Collection.BestGrade(card.Id);
                    if (best > 0) GUI.Label(new Rect(rect.x, rect.yMax + 14f, rect.width, 16f), $"beste Note {best}", UiKit.Hint);
                    GUI.enabled = Game.MoneyCents >= Grading.Cost;
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        station.StartGrading(card);
                        Game.OpenGrading = null;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndScrollView();
            }
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Schließen [Esc]", GUILayout.Height(32f))) Game.OpenGrading = null;
            UiKit.EndWindow();
        }

        /// <summary>Die große Enthüllung: welche Note hat die Karte bekommen?</summary>
        void DrawGradeReveal(float width)
        {
            var slab = Game.GradeReveal;
            var card = slab.Card;
            UiKit.BeginWindow(UiKit.Centered(width, 420f, 440f));
            GUILayout.Label("Grading-Ergebnis", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter));
            var area = GUILayoutUtility.GetRect(380f, 260f);
            CardUI.DrawSlab(new Rect(area.center.x - 90f, area.y, 180f, 250f), card, slab.grade);
            GUI.color = slab.grade >= 10 ? UiKit.Gold : slab.grade >= 8 ? UiKit.Good : Color.white;
            GUILayout.Label($"Note {slab.grade} – {Grading.Describe(slab.grade)}!", UiKit.Title.WithFontSize(22).WithAlignment(TextAnchor.MiddleCenter));
            GUI.color = Color.white;
            GUILayout.Label($"{card.Name} ist jetzt {Money.Format(slab.Value)} wert (×{Grading.Multiplier(slab.grade):0.#}). " +
                            "Leg sie in eine Vitrine – Sammler lieben bewertete Karten.", UiKit.Text.WithAlignment(TextAnchor.MiddleCenter));
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Super! [Esc]", GUILayout.Height(34f))) Game.GradeReveal = null;
            UiKit.EndWindow();
        }

        void DrawPrinter(float width)
        {
            var printer = Game.OpenPrinter;
            UiKit.BeginWindow(UiKit.Centered(width, 480f, 330f));
            GUILayout.Label("3D-Drucker", UiKit.Title);
            GUILayout.Label($"Filament: {printer.Filament} Spulen · fertige Figuren ins Druck-Regal stellen", UiKit.Hint);
            GUILayout.Space(8f);
            foreach (var model in Printer3D.Models)
            {
                GUILayout.BeginHorizontal(UiKit.Panel);
                GUILayout.BeginVertical();
                GUILayout.Label(model.Product.Name, UiKit.Bold);
                GUILayout.Label($"{model.Hours:0} Std. · {model.Filament} Spule(n) · Wert {Money.Format(model.Product.MarketValue)}", UiKit.Text);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUI.enabled = printer.Filament >= model.Filament;
                if (UiKit.Button("Drucken", GUILayout.Width(90f), GUILayout.Height(36f)))
                {
                    printer.StartPrint(model);
                    Game.OpenPrinter = null;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Schließen [Esc]", GUILayout.Height(32f))) Game.OpenPrinter = null;
            UiKit.EndWindow();
        }

        void DrawOffer(float width)
        {
            var seller = Game.SellerOffer;
            UiKit.BeginWindow(UiKit.Centered(width, 520f, 400f));
            GUILayout.Label("Ankauf", UiKit.Title);
            GUILayout.Label("Ein Kunde möchte dir diese Karten verkaufen:", UiKit.Text);
            GUILayout.Space(6f);

            int value = 0;
            var row = GUILayoutUtility.GetRect(480f, 110f);
            for (int i = 0; i < seller.OfferCards.Count; i++)
            {
                var card = seller.OfferCards[i];
                value += card.Value;
                CardUI.Draw(new Rect(row.x + i * 92f, row.y, 78f, 104f), card, true);
            }
            foreach (var card in seller.OfferCards)
            {
                GUI.color = CardDatabase.RarityColor(card.Rarity);
                GUILayout.Label($"{card.Name} ({Catalog.GameName(card.Game)}) · {CardDatabase.RarityName(card.Rarity)} · {Money.Format(card.Value)}{(Game.Collection.IsDiscovered(card.Id) ? "" : "  · NEU")}", UiKit.Text);
            }
            GUI.color = Color.white;
            GUILayout.Label($"Marktwert zusammen: {Money.Format(value)} · Verlangt: {Money.Format(seller.AskingPrice)}", UiKit.Bold);
            if (counterMessage != null) GUILayout.Label(counterMessage, UiKit.Hint);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.enabled = Game.MoneyCents >= seller.AskingPrice;
            if (UiKit.Button($"Kaufen ({Money.Format(seller.AskingPrice)})", GUILayout.Height(36f))) Close(seller, true);
            GUI.enabled = !seller.CounterTried;
            if (UiKit.Button("Gegenangebot −15 %", GUILayout.Height(36f)))
                counterMessage = seller.TryCounterOffer() ? "Der Kunde ist einverstanden!" : "Der Kunde bleibt bei seinem Preis.";
            GUI.enabled = true;
            if (UiKit.Button("Ablehnen", GUILayout.Height(36f))) Close(seller, false);
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
        }

        void Close(Customer seller, bool bought)
        {
            Game.SellerOffer = null;
            counterMessage = null;
            seller.ResolveOffer(bought);
        }
    }
}
