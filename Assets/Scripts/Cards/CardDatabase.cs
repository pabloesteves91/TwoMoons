using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public enum Rarity { Common, Rare, Ultra }

    public class CardDef
    {
        /// <summary>Eindeutig über alle Spiele (steht im Spielstand).</summary>
        public int Id;
        public TcgGame Game;
        /// <summary>Nummer innerhalb des eigenen Spiels (#01–#30).</summary>
        public int Number;
        public string Name;
        public Rarity Rarity;
        /// <summary>Marktwert einer einzelnen Karte (Cent), z. B. für die Vitrine.</summary>
        public int Value;
        /// <summary>Dateiname für ein eigenes Kartenbild in Assets/Art/Cards/Resources/Cards/.</summary>
        public string ArtKey;
    }

    /// <summary>
    /// Alle Karten der erfundenen Kartenspiele: je 30 Karten (20 Gewöhnlich, 7 Selten, 3 Ultra-Selten).
    /// Ids: Monstera 0–29, Spellforge 30–59, Soul Clash 60–89.
    /// </summary>
    public static class CardDatabase
    {
        public const int CardsPerPack = 10;
        public const int CardsPerGame = 30;

        static readonly string[][] MonsteraNames =
        {
            new[]
            {
                "Mooskröte", "Funkelfink", "Kieselkauz", "Pilzling", "Blubberfisch",
                "Glimmwurm", "Zapfenzwerg", "Nebelmotte", "Rindenraupe", "Tümpeltroll",
                "Sandschnecke", "Brisenbiber", "Dornigel", "Pfützenpups", "Laubläufer",
                "Wurzelwicht", "Schlammsalamander", "Distelfuchs", "Krümelkrabbe", "Sternenmaus",
            },
            new[] { "Donnerdachs", "Kristallkäfer", "Flammfalke", "Frostfuchs", "Schattenluchs", "Sturmschildkröte", "Glutlöwe" },
            new[] { "Mondwal", "Sonnenphönix", "Zwielichtdrache" },
        };

        static readonly string[][] SpellforgeNames =
        {
            new[]
            {
                "Lehrling der Flamme", "Schildknappe", "Kräuterhexe", "Wandernder Barde", "Steinwächter",
                "Runenschreiber", "Nebelpfad", "Heiltrank", "Funkenregen", "Eiserne Rüstung",
                "Waldläuferin", "Zwergenschmied", "Irrlicht", "Wächterturm", "Feuerball",
                "Elfenbogen", "Burgtor", "Mondbrunnen", "Schattendolch", "Glückstaler",
            },
            new[] { "Erzmagierin Lyra", "Drachenritter", "Kristallgolem", "Sturmzauber", "Königin der Dornen", "Arkanes Siegel", "Phönixfeder" },
            new[] { "Uralter Wyrm", "Krone der Zwei Monde", "Meister der Schmiede" },
        };

        static readonly string[][] SoulClashNames =
        {
            new[]
            {
                "Klingenläufer", "Funkenkämpferin", "Schattenschüler", "Eisenfaust", "Windtänzerin",
                "Seelenfunke", "Kampfschrei", "Bannkreis", "Gegenschlag", "Doppelschwert",
                "Ninja-Rekrut", "Feuerfaust", "Donnerkick", "Schutzaura", "Kettenblitz",
                "Geisterfuchs", "Mondklinge", "Duellarena", "Kriegsbanner", "Seelenstein",
            },
            new[] { "Drachenseele Kaito", "Sturmherrin Yuna", "Titanenwächter", "Phantomkrieger", "Blutmond-Ritter", "Himmelsschwert", "Ewige Flamme" },
            new[] { "Seelenkaiser", "Zwillingsmond-Drache", "Göttin des Duells" },
        };

        static readonly List<CardDef> all = new List<CardDef>();

        public static readonly List<CardDef> Monstera = Build(TcgGame.Monstera, MonsteraNames, "monstera");
        public static readonly List<CardDef> Spellforge = Build(TcgGame.Spellforge, SpellforgeNames, "spellforge");
        public static readonly List<CardDef> SoulClash = Build(TcgGame.SoulClash, SoulClashNames, "soulclash");

        /// <summary>Alle Karten aller Spiele, nach Id.</summary>
        public static IReadOnlyList<CardDef> All => all;

        public static readonly TcgGame[] Games = { TcgGame.Monstera, TcgGame.Spellforge, TcgGame.SoulClash };

        /// <summary>
        /// Die legendäre Two-Moons-Karte aus Papas Geschichte. Gibt es nur einmal, nie in Packs –
        /// man bekommt sie im Finale (Stadtturnier). Eigenes Bild: Cards/two_moons.png.
        /// </summary>
        public static readonly CardDef TwoMoons = new CardDef
        {
            Id = 999, Game = TcgGame.Monstera, Number = 0, Name = "Two Moons", Rarity = Rarity.Ultra, Value = 1000000, ArtKey = "two_moons",
        };

        static List<CardDef> Build(TcgGame game, string[][] names, string artPrefix)
        {
            var cards = new List<CardDef>();
            int[] values = { 50, 300, 2500 };
            for (int rarity = 0; rarity < names.Length; rarity++)
            {
                foreach (var name in names[rarity])
                {
                    int number = cards.Count + 1;
                    var card = new CardDef
                    {
                        Id = (int)game * CardsPerGame + cards.Count,
                        Game = game,
                        Number = number,
                        Name = name,
                        Rarity = (Rarity)rarity,
                        Value = values[rarity],
                        ArtKey = $"{artPrefix}_{number:00}",
                    };
                    cards.Add(card);
                    all.Add(card);
                }
            }
            return cards;
        }

        public static List<CardDef> ForGame(TcgGame game)
        {
            switch (game)
            {
                case TcgGame.Spellforge: return Spellforge;
                case TcgGame.SoulClash: return SoulClash;
                default: return Monstera;
            }
        }

        public static CardDef Get(int id)
        {
            foreach (var card in all) if (card.Id == id) return card;
            return id == TwoMoons.Id ? TwoMoons : null;
        }

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
        public static List<CardDef> RollPack(TcgGame game)
        {
            var pack = new List<CardDef>();
            for (int i = 0; i < 7; i++) pack.Add(Random(game, Rarity.Common));
            for (int i = 0; i < 2; i++) pack.Add(Random(game, UnityEngine.Random.value < 0.3f ? Rarity.Rare : Rarity.Common));
            pack.Add(Random(game, UnityEngine.Random.value < 0.33f ? Rarity.Ultra : Rarity.Rare));
            pack.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));
            return pack;
        }

        static CardDef Random(TcgGame game, Rarity rarity)
        {
            var options = ForGame(game).FindAll(c => c.Rarity == rarity);
            return options[UnityEngine.Random.Range(0, options.Count)];
        }
    }
}
