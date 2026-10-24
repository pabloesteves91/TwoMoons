using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public enum Rarity { Common, Rare, Ultra }

    public class CardDef
    {
        public int Id;
        public int Number;
        public string Name;
        public Rarity Rarity;
        /// <summary>Marktwert einer einzelnen Karte (Cent), z. B. für die Vitrine.</summary>
        public int Value;
        /// <summary>Dateiname für ein eigenes Kartenbild in Assets/Art/Cards/Resources/Cards/.</summary>
        public string ArtKey;
    }

    /// <summary>Alle Karten der erfundenen Kartenspiele. Zum Start: 30 Monstera-Karten.</summary>
    public static class CardDatabase
    {
        public const int CardsPerPack = 10;

        static readonly string[] CommonNames =
        {
            "Mooskröte", "Funkelfink", "Kieselkauz", "Pilzling", "Blubberfisch",
            "Glimmwurm", "Zapfenzwerg", "Nebelmotte", "Rindenraupe", "Tümpeltroll",
            "Sandschnecke", "Brisenbiber", "Dornigel", "Pfützenpups", "Laubläufer",
            "Wurzelwicht", "Schlammsalamander", "Distelfuchs", "Krümelkrabbe", "Sternenmaus",
        };

        static readonly string[] RareNames =
        {
            "Donnerdachs", "Kristallkäfer", "Flammfalke", "Frostfuchs",
            "Schattenluchs", "Sturmschildkröte", "Glutlöwe",
        };

        static readonly string[] UltraNames = { "Mondwal", "Sonnenphönix", "Zwielichtdrache" };

        public static readonly List<CardDef> Monstera = Build();

        static List<CardDef> Build()
        {
            var cards = new List<CardDef>();
            void Add(string name, Rarity rarity, int value)
            {
                int number = cards.Count + 1;
                cards.Add(new CardDef
                {
                    Id = cards.Count,
                    Number = number,
                    Name = name,
                    Rarity = rarity,
                    Value = value,
                    ArtKey = $"monstera_{number:00}",
                });
            }
            foreach (var name in CommonNames) Add(name, Rarity.Common, 50);
            foreach (var name in RareNames) Add(name, Rarity.Rare, 300);
            foreach (var name in UltraNames) Add(name, Rarity.Ultra, 2500);
            return cards;
        }

        public static CardDef Get(int id) => id >= 0 && id < Monstera.Count ? Monstera[id] : null;

        public static string RarityName(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare: return "Selten";
                case Rarity.Ultra: return "Ultra-Selten";
                default: return "Gewöhnlich";
            }
        }

        public static Color RarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare: return new Color(0.35f, 0.65f, 1f);
                case Rarity.Ultra: return new Color(1f, 0.8f, 0.25f);
                default: return new Color(0.85f, 0.85f, 0.85f);
            }
        }

        /// <summary>
        /// Ein Pack mit 10 Karten, die beste zuletzt (großzügig):
        /// 7× Gewöhnlich, 2× Gewöhnlich oder Selten (30 %), 1× Selten oder Ultra-Selten (33 %).
        /// </summary>
        public static List<CardDef> RollPack()
        {
            var pack = new List<CardDef>();
            for (int i = 0; i < 7; i++) pack.Add(Random(Rarity.Common));
            for (int i = 0; i < 2; i++) pack.Add(Random(UnityEngine.Random.value < 0.3f ? Rarity.Rare : Rarity.Common));
            pack.Add(Random(UnityEngine.Random.value < 0.33f ? Rarity.Ultra : Rarity.Rare));
            pack.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));
            return pack;
        }

        static CardDef Random(Rarity rarity)
        {
            var options = Monstera.FindAll(c => c.Rarity == rarity);
            return options[UnityEngine.Random.Range(0, options.Count)];
        }
    }
}
