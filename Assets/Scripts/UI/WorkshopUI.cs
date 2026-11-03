using UnityEngine;

namespace TwoMoons
{
    /// <summary>Fenster für den 3D-Drucker (Modell wählen), den Ankauf an der Kasse und die Grading-Station.</summary>
    public class WorkshopUI : MonoBehaviour
    {
        static GUIStyle bestStyle;

        string counterMessage;
        Vector2 gradingScroll;

        static GameManager Game => GameManager.Instance;

        void OnGUI()
        {
            if (Game.Phase == GamePhase.MainMenu) return;
            if (Game.OpenPrinter == null && Game.SellerOffer == null && Game.OpenGrading == null && Game.GradeReveal == null &&
                Game.CurrentRobbery == null)
            {
                counterMessage = null;
                return;
            }
            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.5f);
            if (Game.CurrentRobbery != null) DrawRobbery(width);
            else if (Game.SellerOffer != null) DrawOffer(width);
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

            // Die Legende darf genau einmal bewertet werden.
            if (Game.HasTwoMoons && !Game.TwoMoonsGraded)
            {
                GUI.color = UiKit.Gold;
                GUI.enabled = Game.MoneyCents >= Grading.Cost;
                if (UiKit.Button("Two Moons bewerten lassen (einmalig)", GUILayout.Height(32f)))
                {
                    station.StartGrading(CardDatabase.TwoMoons);
                    Game.OpenGrading = null;
                }
                GUI.enabled = true;
                GUI.color = Color.white;
                GUILayout.Space(6f);
            }

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
                    if (best > 0)
                    {
                        // Kurz und in Gold, damit es auch bei einer 10 in eine Zeile passt.
                        bestStyle ??= new GUIStyle(UiKit.Hint) { fontSize = 11, fontStyle = FontStyle.Bold, wordWrap = false, clipping = TextClipping.Overflow };
                        bestStyle.normal.textColor = UiKit.Gold;
                        // Schrift so weit verkleinern, dass der Text unter die Karte passt (nichts wird abgeschnitten).
                        var bestText = new GUIContent(best >= Grading.BlackLabel ? "Beste: Black Label" : $"Beste: {Grading.Label(best)}");
                        bestStyle.fontSize = 11;
                        while (bestStyle.fontSize > 7 && bestStyle.CalcSize(bestText).x > rect.width + 6f) bestStyle.fontSize--;
                        GUI.Label(new Rect(rect.x - 3f, rect.yMax + 15f, rect.width + 6f, 16f), bestText, bestStyle);
                    }
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
        float revealTime;

        /// <summary>Note enthüllen: Sounds und (bei Black Label) die große Animation starten.</summary>
        void Reveal(Slab slab)
        {
            slab.Revealed = true;
            revealTime = Time.unscaledTime;
            bool blackLabel = slab.grade >= Grading.BlackLabel;
            if (blackLabel || slab.grade >= 10) SoundManager.Play(blackLabel || slab.Card.Rarity == Rarity.Ultra ? Sfx.Fanfare : Sfx.ShinyCard);
            else SoundManager.Play(slab.grade >= 8 ? Sfx.RareCard : Sfx.CardFlip);
            if (blackLabel || (slab.grade >= 10 && slab.Card.Rarity == Rarity.Ultra)) SoundManager.Play(Sfx.ShinyCard);
        }

        void DrawGradeReveal(float width)
        {
            var slab = Game.GradeReveal;
            var card = slab.Card;
            bool blackLabel = slab.grade >= Grading.BlackLabel;
            float t = slab.Revealed ? Time.unscaledTime - revealTime : 0f;
            var window = UiKit.Centered(width, 420f, 440f);

            // Black Label: Bildschirm blitzt auf, dann goldene Strahlen hinter dem Fenster.
            if (slab.Revealed && blackLabel) DrawBlackLabelBackdrop(width, window, t);

            UiKit.BeginWindow(window);
            GUILayout.Label("Grading-Ergebnis", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter));
            var area = GUILayoutUtility.GetRect(380f, 260f);
            var slabRect = new Rect(area.center.x - 90f, area.y, 180f, 250f);

            if (!slab.Revealed)
            {
                // Verdeckt: leichtes Wackeln macht neugierig.
                float wobble = Mathf.Sin(Time.unscaledTime * 6f) * 2f;
                CardUI.DrawHiddenSlab(new Rect(slabRect.x + wobble, slabRect.y, slabRect.width, slabRect.height));
                GUILayout.Label("Die Note ist noch verdeckt …", UiKit.Title.WithFontSize(20).WithAlignment(TextAnchor.MiddleCenter));
                GUILayout.FlexibleSpace();
                if (UiKit.Button("Enthüllen!", GUILayout.Height(38f))) Reveal(slab);
                UiKit.EndWindow();
                return;
            }

            // Aufploppen: kurz größer, dann normal (bei Black Label länger und kräftiger).
            float pop = blackLabel ? 1f + 0.25f * Mathf.Exp(-t * 3f) * Mathf.Cos(t * 14f) : 1f + 0.12f * Mathf.Exp(-t * 6f);
            var popped = new Rect(slabRect.center.x - slabRect.width * pop / 2f, slabRect.center.y - slabRect.height * pop / 2f,
                slabRect.width * pop, slabRect.height * pop);
            if (blackLabel) DrawSparkles(popped, t);
            CardUI.DrawSlab(popped, card, slab.grade);
            if (blackLabel) DrawShineSweep(popped, t);

            GUI.color = blackLabel ? Color.Lerp(UiKit.Gold, Color.white, 0.5f + 0.5f * Mathf.Sin(t * 8f))
                : slab.grade >= 10 ? UiKit.Gold : slab.grade >= 8 ? UiKit.Good : Color.white;
            GUILayout.Label(blackLabel ? "BLACK LABEL!" : $"{Grading.Label(slab.grade)} – {Grading.Describe(slab.grade)}!",
                UiKit.Title.WithFontSize(blackLabel ? 26 : 22).WithAlignment(TextAnchor.MiddleCenter));
            GUI.color = Color.white;
            GUILayout.Label(card == CardDatabase.TwoMoons
                ? $"Die Legende ist jetzt {Money.Format(slab.Value)} wert – aber unverkäuflich. Sie bleibt im Sammelbuch."
                : $"{card.Name} ist jetzt {Money.Format(slab.Value)} wert (×{Grading.Multiplier(slab.grade):0.#}). " +
                  "Leg sie in eine Vitrine – Sammler lieben bewertete Karten.", UiKit.Text.WithAlignment(TextAnchor.MiddleCenter));
            GUILayout.FlexibleSpace();
            if (UiKit.Button("Super! [Esc]", GUILayout.Height(34f))) Game.GradeReveal = null;
            UiKit.EndWindow();
        }

        /// <summary>Weißer Blitz, abgedunkelter Hintergrund und rotierende goldene Strahlen.</summary>
        static void DrawBlackLabelBackdrop(float width, Rect window, float t)
        {
            UiKit.DrawRect(new Rect(0f, 0f, width, UiKit.VirtualHeight), new Color(0f, 0f, 0f, Mathf.Clamp01(t * 2f) * 0.55f));
            var center = new Vector2(window.center.x, window.y + 175f);
            var matrix = GUI.matrix;
            for (int i = 0; i < 12; i++)
            {
                GUIUtility.RotateAroundPivot(i * 30f + t * 20f, center);
                float alpha = Mathf.Clamp01(t * 1.5f) * (0.25f + 0.1f * Mathf.Sin(t * 3f + i));
                UiKit.DrawRect(new Rect(center.x - 6f, center.y - 420f, 12f, 420f), new Color(1f, 0.85f, 0.3f, alpha));
                GUI.matrix = matrix;
            }
            // Blitz am Anfang
            if (t < 0.5f) UiKit.DrawRect(new Rect(0f, 0f, width, UiKit.VirtualHeight), new Color(1f, 1f, 1f, 1f - t * 2f));
        }

        /// <summary>Goldene Funken rund um die Hülle.</summary>
        static void DrawSparkles(Rect rect, float t)
        {
            for (int i = 0; i < 16; i++)
            {
                float phase = Mathf.Repeat(t * 0.8f + i * 0.137f, 1f);
                float angle = i * 2.4f + t * 0.6f;
                float radius = rect.width * (0.55f + 0.35f * phase);
                var p = rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                float size = 3f + 5f * Mathf.Sin(phase * Mathf.PI);
                UiKit.DrawRect(new Rect(p.x - size / 2f, p.y - size / 2f, size, size), new Color(1f, 0.9f, 0.4f, Mathf.Sin(phase * Mathf.PI)));
            }
        }

        /// <summary>Ein heller Glanz-Streifen wandert über die schwarze Hülle.</summary>
        static void DrawShineSweep(Rect rect, float t)
        {
            float x = Mathf.Repeat(t * 0.6f, 1.6f) - 0.3f;
            if (x < 0f || x > 1f) return;
            GUI.BeginGroup(rect);
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(20f, new Vector2(rect.width * x, rect.height / 2f));
            UiKit.DrawRect(new Rect(rect.width * x - 8f, -rect.height, 16f, rect.height * 3f), new Color(1f, 1f, 1f, 0.35f));
            GUI.matrix = matrix;
            GUI.EndGroup();
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

        void DrawRobbery(float width)
        {
            var robbery = Game.CurrentRobbery;
            UiKit.BeginWindow(UiKit.Centered(width, 500f, 330f));
            GUI.color = UiKit.Bad;
            GUILayout.Label("Überfall!", UiKit.Title);
            GUI.color = Color.white;
            GUILayout.Label("Ein Gauner mit Sturmhaube steht vor der Kasse. Die Kunden ducken sich.", UiKit.Text);
            GUILayout.Space(6f);
            GUILayout.Label(robbery.WantsGoods
                ? $"„Her mit der Ware! {robbery.Demand} – sofort!“"
                : $"„Kasse auf! {robbery.Demand}, dann bin ich weg!“", UiKit.Bold);
            if (robbery.Message != null) GUILayout.Label(robbery.Message, UiKit.Text);
            GUILayout.Space(4f);
            GUILayout.Label(Robbery.HasAlarm
                ? "Deine Alarmanlage hat einen Knopf unter der Theke …"
                : "Tipp: Mit einer Alarmanlage (Computer → Einrichtung) könntest du Alarm auslösen.", UiKit.Hint);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (UiKit.Button(robbery.WantsGoods ? "Ware geben" : "Geld geben", GUILayout.Height(36f))) robbery.GiveIn();
            GUI.enabled = !robbery.Negotiated;
            if (UiKit.Button("Verhandeln", GUILayout.Height(36f))) robbery.Negotiate();
            GUI.enabled = Robbery.HasAlarm;
            if (UiKit.Button("Alarmknopf", GUILayout.Height(36f))) robbery.PressAlarm();
            GUI.enabled = true;
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
