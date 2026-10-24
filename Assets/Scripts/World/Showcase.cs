using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Vitrine für Einzelkarten mit eigenem Preis (4 Fächer, Premium-Vitrine 8). Sammler-Kunden kaufen daraus.
    /// Bewertete Karten (Slabs) liegen mit ihrer Note im Fach.
    /// </summary>
    public class Showcase : Interactable
    {
        public const int NormalSlots = 4;
        public const int PremiumSlots = 8;
        /// <summary>In der beleuchteten Premium-Vitrine zahlen Sammler bis zu 10 % mehr.</summary>
        public const float PremiumBonus = 1.1f;

        public Vector2Int Cell { get; private set; }
        public bool Premium { get; private set; }
        public int SlotCount => Cards.Length;

        /// <summary>Karten-Id pro Fach, -1 = leer.</summary>
        public int[] Cards { get; private set; }
        public int[] Prices { get; private set; }
        /// <summary>Note der Karte im Fach, 0 = nicht bewertet.</summary>
        public int[] Grades { get; private set; }

        SpriteRenderer[] cardIcons;

        public void Init(Vector2Int cell, SpriteRenderer[] icons, bool premium)
        {
            Cell = cell;
            Premium = premium;
            cardIcons = icons;
            Cards = new int[icons.Length];
            for (int i = 0; i < Cards.Length; i++) Cards[i] = -1;
            Prices = new int[icons.Length];
            Grades = new int[icons.Length];
            Refresh();
        }

        public bool HasGradedCards
        {
            get
            {
                for (int i = 0; i < Cards.Length; i++) if (Cards[i] >= 0 && Grades[i] > 0) return true;
                return false;
            }
        }

        /// <summary>Was die Karte im Fach wert ist (mit Note).</summary>
        public int ValueAt(int slot) => Grading.Value(CardDatabase.Get(Cards[slot]), Grades[slot]);

        public bool HasCards
        {
            get
            {
                foreach (int card in Cards) if (card >= 0) return true;
                return false;
            }
        }

        public void Put(int slot, CardDef card, int price, int grade = 0)
        {
            Cards[slot] = card.Id;
            Prices[slot] = price;
            Grades[slot] = grade;
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

        /// <summary>Karte zurück zum Spieler: bewertete in die Slab-Sammlung, sonst ins Sammelbuch.</summary>
        public void ReturnToOwner(int slot)
        {
            int grade = Grades[slot];
            int card = Take(slot);
            if (card < 0) return;
            if (grade > 0) GameManager.Instance.AddSlab(new Slab { card = card, grade = grade }, false);
            else GameManager.Instance.Collection.Add(card);
        }

        public override string GetPrompt(PlayerController player)
        {
            int filled = 0;
            foreach (int card in Cards) if (card >= 0) filled++;
            return $"{(Premium ? "Premium-Vitrine" : "Vitrine")} einräumen ({filled}/{SlotCount} Karten)";
        }

        public override void Interact(PlayerController player) => GameManager.Instance.OpenShowcase = this;

        void Refresh()
        {
            if (cardIcons == null) return;
            for (int i = 0; i < cardIcons.Length; i++)
            {
                cardIcons[i].enabled = Cards[i] >= 0;
                if (Cards[i] >= 0) cardIcons[i].sprite = Art.Get(Grades[i] > 0 ? "slabIcon" : "cardIcon");
            }
        }
    }
}
