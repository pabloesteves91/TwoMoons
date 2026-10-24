using System;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Eine bewertete, in Schutzhülle versiegelte Karte („Slab“).</summary>
    [Serializable]
    public class Slab
    {
        public int card;
        public int grade;

        public CardDef Card => CardDatabase.Get(card);
        public int Value => Grading.Value(Card, grade);
    }

    /// <summary>Karten-Grading (ab Level 5): Note 1–10, gute Noten machen Karten viel wertvoller.</summary>
    public static class Grading
    {
        /// <summary>Kosten pro Karte.</summary>
        public const int Cost = 500;
        /// <summary>Prüfdauer in Spielstunden.</summary>
        public const float Hours = 1f;

        public static float Multiplier(int grade) =>
            grade >= 10 ? 5f : grade == 9 ? 3f : grade == 8 ? 2f : grade == 7 ? 1.5f : 1f;

        public static int Value(CardDef card, int grade) => card == null ? 0 : Mathf.RoundToInt(card.Value * Multiplier(grade));

        public static string Describe(int grade) =>
            grade >= 10 ? "makellos" : grade == 9 ? "fast perfekt" : grade == 8 ? "sehr gut" : grade == 7 ? "gut" : "mit Gebrauchsspuren";

        /// <summary>Gute Noten sind häufiger als schlechte – die Karten kommen ja frisch aus dem Pack.</summary>
        public static int Roll()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.08f) return 10;
            if (roll < 0.26f) return 9;
            if (roll < 0.52f) return 8;
            if (roll < 0.74f) return 7;
            return UnityEngine.Random.Range(3, 7);
        }
    }
}
