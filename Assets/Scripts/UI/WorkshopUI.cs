using UnityEngine;

namespace TwoMoons
{
    /// <summary>Fenster für den 3D-Drucker (Modell wählen) und den Ankauf von Karten an der Kasse.</summary>
    public class WorkshopUI : MonoBehaviour
    {
        string counterMessage;

        static GameManager Game => GameManager.Instance;

        void OnGUI()
        {
            if (Game.Phase == GamePhase.MainMenu) return;
            if (Game.OpenPrinter == null && Game.SellerOffer == null)
            {
                counterMessage = null;
                return;
            }
            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.5f);
            if (Game.SellerOffer != null) DrawOffer(width);
            else DrawPrinter(width);
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
