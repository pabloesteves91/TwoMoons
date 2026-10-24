using System;
using System.Collections.Generic;

namespace TwoMoons
{
    public class Quest
    {
        public string Text;
        public int Target = 1;
        public int Progress;
        /// <summary>Welches Ereignis die Aufgabe voranbringt (siehe <see cref="Chapters"/>).</summary>
        public string Key;
        /// <summary>Für Aufgaben, die einen Zustand prüfen (z. B. Gemütlichkeit) statt Ereignisse zu zählen.</summary>
        public Func<int> Poll;
        public bool Done => Progress >= Target;
    }

    public class QuestStage
    {
        public string Title;
        public List<Quest> Quests;
        /// <summary>Kleiner Story-Moment, wenn diese Etappe geschafft ist ({0} = Name).</summary>
        public string[] Outro;
    }

    /// <summary>
    /// Die Aufgaben aller 7 Kapitel, in Etappen. Kapitel = Store-Level.
    /// Kapitel 1–3 sind auf ca. 1 Stunde ausgelegt, 4–7 auf ca. 90 Minuten.
    /// Aufgaben für noch nicht gebaute Funktionen (Turniere, Spellforge, 3D-Druck …) kommen dazu, sobald es sie gibt.
    /// </summary>
    public static class ChapterData
    {
        static GameManager Game => GameManager.Instance;

        // Ereignis-Aufgaben (zählen ab Beginn der Etappe)
        static Quest Sell(int packs) => new Quest { Text = "Packs verkaufen", Target = packs, Key = "packsSold" };
        static Quest Serve(int customers) => new Quest { Text = "Kunden bedienen", Target = customers, Key = "served" };
        static Quest ProfitDays(int days) => new Quest { Text = "Tage mit Gewinn abschließen", Target = days, Key = "profitDay" };
        static Quest OpenPacks(int packs) => new Quest { Text = "Packs am Pack-Tisch öffnen", Target = packs, Key = "packOpened" };
        static Quest SellCards(int cards) => new Quest { Text = "Karten aus der Vitrine verkaufen", Target = cards, Key = "cardSold" };
        static Quest PullUltra(int cards) => new Quest { Text = "Ultra-Seltene ziehen", Target = cards, Key = "ultraPulled" };
        static Quest SellProduct(Product product, int count) =>
            new Quest { Text = $"{product.Name} verkaufen", Target = count, Key = $"sold:{product.Id}" };
        static Quest SellAccessories(int count) => new Quest { Text = "Zubehör verkaufen", Target = count, Key = "accessorySold" };
        static Quest AccessoryStands(int count) =>
            new Quest { Text = "Zubehör-Ständer im Laden", Target = count, Poll = () => Game.Shelves.FindAll(s => s.Category == ProductCategory.Accessory).Count };
        static Quest Tournaments(int count) => new Quest { Text = "Turniere ausrichten", Target = count, Key = "tournament" };
        static Quest Furnished(Furniture furniture, int count) =>
            new Quest { Text = $"{FurnitureCatalog.Get(furniture).Name} im Laden", Target = count, Poll = () => Game.Decos.FindAll(d => d.Type == furniture).Count };
        static Quest Hire() => new Quest { Text = "Einen Mitarbeiter einstellen", Key = "hired", Poll = () => Game.Staff != null ? 1 : 0 };
        static Quest SellCategory(ProductCategory category, string text, int count) =>
            new Quest { Text = text, Target = count, Key = $"category:{category}" };
        static Quest Prints(int count) => new Quest { Text = "3D-Drucke fertigstellen", Target = count, Key = "printed" };
        static Quest BuyCollections(int count) => new Quest { Text = "Karten ankaufen", Target = count, Key = "cardsBought" };
        static Quest GradeCards(int count) => new Quest { Text = "Karten bewerten lassen", Target = count, Key = "graded" };
        static Quest TopGrades(int count) => new Quest { Text = "Note 9 oder 10 bekommen", Target = count, Key = "topGrade" };
        static Quest SellSlabs(int count) => new Quest { Text = "Bewertete Karten verkaufen", Target = count, Key = "slabSold" };
        static Quest Extension() => new Quest { Text = "Laden erweitern (Computer → Einrichtung)", Key = "expanded", Poll = () => ShopGrid.Expanded ? 1 : 0 };
        static Quest BestDay(int packs) => new Quest { Text = "An einem Tag Packs verkaufen", Target = packs, Key = "bestDay" };

        // Zustands-Aufgaben
        static Quest Shelves(int count) => new Quest { Text = "Regale im Laden", Target = count, Poll = () => Game.Shelves.Count };
        static Quest Showcases(int count) => new Quest { Text = "Vitrinen im Laden", Target = count, Poll = () => Game.Showcases.Count };
        static Quest Decos(int count) => new Quest { Text = "Deko im Laden", Target = count, Poll = () => Game.Decos.Count };
        static Quest Cozy(int value) => new Quest { Text = "Gemütlichkeit erreichen", Target = value, Poll = () => Game.Coziness };
        static Quest Cards(int count) => new Quest { Text = "Karten im Sammelbuch", Target = count, Poll = () => Game.Collection.DiscoveredCount };
        static Quest CardsIn(TcgGame game, int count) =>
            new Quest { Text = $"{Catalog.GameName(game)}-Karten im Sammelbuch", Target = count, Poll = () => Game.Collection.DiscoveredIn(game) };
        static Quest Money(int euros) => new Quest { Text = "Euro auf dem Konto", Target = euros, Poll = () => Game.MoneyCents / 100 };

        static Quest Reputation(int stage, string name) =>
            new Quest { Text = $"Ruf „{name}“ erreichen", Target = 1, Poll = () => Game.ReputationStage >= stage ? 1 : 0 };

        /// <summary>Erstellt die Etappen eines Kapitels (jedes Mal neu, damit der Fortschritt bei 0 beginnt).</summary>
        public static List<QuestStage> Create(int chapter)
        {
            switch (chapter)
            {
                case 1:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Der erste Tag",
                            Quests = new List<Quest>
                            {
                                new Quest { Text = "Laden putzen", Target = 5, Key = "clean" },
                                new Quest { Text = "Erstes Regal aufstellen", Key = "shelfPlaced" },
                                new Quest { Text = "Monstera-Packs einräumen", Target = 12, Key = "stock" },
                                new Quest { Text = "Laden öffnen", Key = "open" },
                                Serve(5),
                            },
                            Outro = new[]
                            {
                                "{0}: Die ersten Kunden waren da! Aber ein Regal ist viel zu wenig für einen richtigen Laden.",
                                "Mehr Ware, mehr Kunden – und am Ende des Tages soll mehr in der Kasse sein als vorher.",
                            },
                        },
                        new QuestStage
                        {
                            Title = "Ein richtiger Laden",
                            Quests = new List<Quest> { Sell(30), Shelves(2), ProfitDays(2) },
                            Outro = new[]
                            {
                                "Eine Nachricht von Papa: „Ein Laden ist mehr als Regale. Die Leute sollen sich bei dir wohlfühlen.“",
                                "{0}: Er hat recht. Ein bisschen Deko würde dem alten Laden guttun.",
                            },
                        },
                        new QuestStage
                        {
                            Title = "Gemütlich soll es sein",
                            Quests = new List<Quest> { Decos(3), Cozy(5), Serve(25) },
                            Outro = new[]
                            {
                                "{0}: Es fühlt sich langsam an wie ein richtiger Laden. Wie Papas Laden damals.",
                                "Jetzt muss es sich nur noch rechnen. Mal sehen, was an einem richtig guten Tag möglich ist!",
                            },
                        },
                        new QuestStage
                        {
                            Title = "Der Laden läuft",
                            Quests = new List<Quest> { Sell(80), BestDay(20), ProfitDays(3), Serve(50) },
                        },
                    };

                case 2:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Neue Ideen",
                            Quests = new List<Quest> { Showcases(1), AccessoryStands(1), OpenPacks(5), Cards(10) },
                            Outro = new[]
                            {
                                "Papa ruft an: „Sammler sind die treuesten Kunden. Zeig ihnen, was du hast.“",
                            },
                        },
                        new QuestStage
                        {
                            Title = "Sammler entdecken TwoMoons",
                            Quests = new List<Quest> { SellCards(3), Reputation(1, "Geheimtipp"), SellProduct(Catalog.SpellforgePack, 20), Sell(100) },
                            Outro = new[] { "{0}: Immer mehr Gesichter kenne ich schon beim Namen. Das sind meine ersten Stammkunden!" },
                        },
                        new QuestStage
                        {
                            Title = "Stammkunden",
                            Quests = new List<Quest> { Serve(80), SellAccessories(10), Cozy(10), ProfitDays(3) },
                        },
                    };

                case 3:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Turnier-Vorbereitung",
                            Quests = new List<Quest> { Furnished(Furniture.GameTable, 1), Furnished(Furniture.PrizeCounter, 1), Furnished(Furniture.TournamentBoard, 1) },
                            Outro = new[] { "{0}: Alles steht bereit. Jetzt fehlen nur noch die Spieler!" },
                        },
                        new QuestStage
                        {
                            Title = "Das erste Turnier",
                            Quests = new List<Quest> { Tournaments(1), Sell(150), Money(300) },
                            Outro = new[] { "{0}: Die Leute fragen schon, wann das nächste Turnier ist!" },
                        },
                        new QuestStage
                        {
                            Title = "Ein Name in der Stadt",
                            Quests = new List<Quest> { Tournaments(2), Reputation(2, "Beliebt"), Serve(120), PullUltra(1) },
                            Outro = new[] { "{0}: Die Turnierspieler bleiben gern noch länger. Eine gemütliche Ecke wäre perfekt!" },
                        },
                        new QuestStage
                        {
                            Title = "Die Couch-Ecke",
                            Quests = new List<Quest>
                            {
                                Furnished(Furniture.Couch, 1), Furnished(Furniture.Tv, 1), Furnished(Furniture.Console, 1),
                                SellCategory(ProductCategory.Drink, "Getränke verkaufen", 15), Hire(),
                                SellProduct(Catalog.SoulClashPack, 20),
                            },
                        },
                    };

                case 4:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Die Werkstatt",
                            Quests = new List<Quest> { Furnished(Furniture.Printer3D, 1), Prints(3), SellCategory(ProductCategory.Print, "3D-Drucke verkaufen", 3) },
                            Outro = new[] { "{0}: Die Figuren sehen fantastisch aus! Papa hätte das geliebt." },
                        },
                        new QuestStage
                        {
                            Title = "Für Sammler",
                            Quests = new List<Quest> { Cards(20), SellCards(10), BuyCollections(3), Money(500) },
                            Outro = new[] { "In einer Kiste findet {0} Papas altes Notizbuch. Auf der letzten Seite: zwei gezeichnete Monde." },
                        },
                        new QuestStage
                        {
                            Title = "Volle Regale",
                            Quests = new List<Quest> { Sell(250), Cozy(15), ProfitDays(5) },
                        },
                        new QuestStage
                        {
                            Title = "Ein Laden zum Bleiben",
                            Quests = new List<Quest> { Serve(200), Decos(8) },
                        },
                    };

                case 5:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Auf den Punkt geprüft",
                            Quests = new List<Quest> { Furnished(Furniture.GradingStation, 1), GradeCards(3) },
                            Outro = new[] { "{0}: Note 9 auf der ersten Karte – das muss ich Papa erzählen!" },
                        },
                        new QuestStage
                        {
                            Title = "Sammlerstücke",
                            Quests = new List<Quest> { GradeCards(8), TopGrades(2), SellSlabs(4), SellCards(12) },
                            Outro = new[]
                            {
                                "{0} ruft Papa nach Ladenschluss an. Seine Stimme klingt müde, er hustet.",
                                "Papa: „Mir geht's nicht so gut zurzeit. Aber erzähl – wie läuft mein alter Laden?“",
                                "{0}: „Er wird zu klein, Papa. Wir bauen an.“ – Papa lacht leise: „Das wollte ich immer.“",
                            },
                        },
                        new QuestStage
                        {
                            Title = "TwoMoons wächst",
                            Quests = new List<Quest> { Extension(), Shelves(8), Showcases(3), Decos(12) },
                            Outro = new[] { "Der neue Raum ist voll mit Leuten. {0} macht ein Foto und schickt es Papa." },
                        },
                        new QuestStage
                        {
                            Title = "Stadtgespräch",
                            Quests = new List<Quest> { Reputation(3, "Stadtbekannt"), Sell(400), Serve(300), ProfitDays(6) },
                        },
                    };

                case 6:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Großer Andrang",
                            Quests = new List<Quest> { Sell(500), SellCards(25), Cozy(25) },
                            Outro = new[] { "Papa steht plötzlich in der Tür: „Ich helfe dir heute an der Kasse. So lange ich kann.“" },
                        },
                        new QuestStage
                        {
                            Title = "Seltene Schätze",
                            Quests = new List<Quest> { Serve(450), Money(2000), PullUltra(3) },
                        },
                        new QuestStage
                        {
                            Title = "Das komplette Sammelbuch",
                            Quests = new List<Quest> { CardsIn(TcgGame.Monstera, CardDatabase.CardsPerGame), ProfitDays(8) },
                        },
                    };

                case 7:
                    return new List<QuestStage>
                    {
                        new QuestStage
                        {
                            Title = "Legende werden",
                            Quests = new List<Quest> { Reputation(4, "Legendär"), Sell(700), Money(3000) },
                            Outro = new[] { "Der Hersteller schreibt: TwoMoons soll das große Stadtturnier ausrichten!" },
                        },
                        new QuestStage
                        {
                            Title = "Bereit für das große Turnier",
                            Quests = new List<Quest> { Serve(600), SellCards(40), ProfitDays(10) },
                        },
                    };

                default:
                    return new List<QuestStage>();
            }
        }
    }
}
