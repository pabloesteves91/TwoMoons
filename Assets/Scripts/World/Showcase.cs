using UnityEngine;

namespace TwoMoons
{
    /// <summary>Vitrine für bis zu 4 Einzelkarten mit eigenem Preis. Sammler-Kunden kaufen daraus.</summary>
    public class Showcase : Interactable
    {
        public const int SlotCount = 4;

        public Vector2Int Cell { get; private set; }

        /// <summary>Karten-Id pro Fach, -1 = leer.</summary>
        public readonly int[] Cards = { -1, -1, -1, -1 };
        public readonly int[] Prices = new int[SlotCount];

        SpriteRenderer[] cardIcons;

        public void Init(Vector2Int cell, SpriteRenderer[] icons)
        {
            Cell = cell;
            cardIcons = icons;
            Refresh();
        }

        public bool HasCards
        {
            get
            {
                foreach (int card in Cards) if (card >= 0) return true;
                return false;
            }
        }

        public void Put(int slot, CardDef card, int price)
        {
            Cards[slot] = card.Id;
            Prices[slot] = price;
            Refresh();
        }

        /// <summary>Nimmt die Karte aus dem Fach und gibt ihre Id zurück (-1, wenn leer).</summary>
        public int Take(int slot)
        {
            int card = Cards[slot];
            Cards[slot] = -1;
            Refresh();
            return card;
        }

        public override string GetPrompt(PlayerController player)
        {
            int filled = 0;
            foreach (int card in Cards) if (card >= 0) filled++;
            return $"Vitrine einräumen ({filled}/{SlotCount} Karten)";
        }

        public override void Interact(PlayerController player) => GameManager.Instance.OpenShowcase = this;

        void Refresh()
        {
            if (cardIcons == null) return;
            for (int i = 0; i < cardIcons.Length; i++) cardIcons[i].enabled = Cards[i] >= 0;
        }
    }
}
