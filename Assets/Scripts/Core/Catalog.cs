namespace TwoMoons
{
    /// <summary>Die erfundenen Kartenspiele.</summary>
    public enum TcgGame { Monstera, Spellforge, SoulClash }

    /// <summary>Neue Waren immer hinten anhängen – die Nummern stehen in Spielständen.</summary>
    public enum ProductId { MonsteraPack, SpellforgePack, Sleeves, Binder, Playmat, TournamentPack, SoulClashPack, Soda, EnergyDrink }

    /// <summary>Wo eine Ware im Laden steht: Packs ins Regal, Zubehör in den Zubehör-Ständer.</summary>
    public enum ProductCategory { Pack, Accessory, Promo, Drink }

    public enum OrderKind { ProductBox, Furniture }

    public class Product
    {
        public ProductId Id;
        public string Name;
        public string SpriteKey;
        public ProductCategory Category;
        /// <summary>Zu welchem Spiel ein Boosterpack gehört (nur bei Packs).</summary>
        public TcgGame Game;
        /// <summary>Einkaufspreis pro Stück beim Großhändler.</summary>
        public int BuyPrice;
        /// <summary>Was Kunden für fair halten. Startwert für den Verkaufspreis.</summary>
        public int MarketValue;
        /// <summary>Ab diesem Store-Level gibt es die Ware.</summary>
        public int MinLevel = 1;
    }

    public class OrderOption
    {
        public string Name;
        public string Description;
        public int Cost;
        public OrderKind Kind;
        public ProductId Product;
        public int Amount;
        public Furniture Furniture;
        /// <summary>Ab diesem Store-Level kann man es kaufen.</summary>
        public int MinLevel = 1;
    }

    /// <summary>Alle Waren und Bestellmöglichkeiten.</summary>
    public static class Catalog
    {
        public static readonly Product MonsteraPack = new Product
        {
            Id = ProductId.MonsteraPack, Name = "Monstera-Booster", SpriteKey = "packMonstera",
            Category = ProductCategory.Pack, Game = TcgGame.Monstera, BuyPrice = 300, MarketValue = 400,
        };

        public static readonly Product SpellforgePack = new Product
        {
            Id = ProductId.SpellforgePack, Name = "Spellforge-Booster", SpriteKey = "packSpellforge",
            Category = ProductCategory.Pack, Game = TcgGame.Spellforge, BuyPrice = 350, MarketValue = 500, MinLevel = 2,
        };

        public static readonly Product Sleeves = new Product
        {
            Id = ProductId.Sleeves, Name = "Kartenhüllen", SpriteKey = "sleeves",
            Category = ProductCategory.Accessory, BuyPrice = 150, MarketValue = 300, MinLevel = 2,
        };

        public static readonly Product Binder = new Product
        {
            Id = ProductId.Binder, Name = "Sammelordner", SpriteKey = "binder",
            Category = ProductCategory.Accessory, BuyPrice = 600, MarketValue = 1200, MinLevel = 2,
        };

        public static readonly Product Playmat = new Product
        {
            Id = ProductId.Playmat, Name = "Spielmatte", SpriteKey = "playmat",
            Category = ProductCategory.Accessory, BuyPrice = 800, MarketValue = 1500, MinLevel = 2,
        };

        /// <summary>Gratis-Packs vom Hersteller – nur für den Turnier-Prizepool, nicht zum Verkaufen.</summary>
        public static readonly Product TournamentPack = new Product
        {
            Id = ProductId.TournamentPack, Name = "Turnier-Packs", SpriteKey = "packMonstera",
            Category = ProductCategory.Promo, Game = TcgGame.Monstera, MinLevel = 3,
        };

        public static readonly Product SoulClashPack = new Product
        {
            Id = ProductId.SoulClashPack, Name = "Soul-Clash-Booster", SpriteKey = "packSoulClash",
            Category = ProductCategory.Pack, Game = TcgGame.SoulClash, BuyPrice = 400, MarketValue = 600, MinLevel = 3,
        };

        public static readonly Product Soda = new Product
        {
            Id = ProductId.Soda, Name = "Limo", SpriteKey = "soda",
            Category = ProductCategory.Drink, BuyPrice = 80, MarketValue = 200, MinLevel = 3,
        };

        public static readonly Product EnergyDrink = new Product
        {
            Id = ProductId.EnergyDrink, Name = "Energy-Drink", SpriteKey = "energyDrink",
            Category = ProductCategory.Drink, BuyPrice = 120, MarketValue = 300, MinLevel = 3,
        };

        /// <summary>Alle Waren, in der Reihenfolge von <see cref="ProductId"/>.</summary>
        public static readonly Product[] Products =
            { MonsteraPack, SpellforgePack, Sleeves, Binder, Playmat, TournamentPack, SoulClashPack, Soda, EnergyDrink };

        /// <summary>Tab „Ware“ am Computer.</summary>
        public static readonly OrderOption[] Orders =
        {
            Box(MonsteraPack, "Monstera-Display", "Karton mit 12 Monstera-Boosterpacks", 12),
            Box(SpellforgePack, "Spellforge-Display", "Karton mit 12 Spellforge-Boosterpacks", 12),
            Box(Sleeves, "Kartenhüllen", "Karton mit 10 Packungen Hüllen · für den Zubehör-Ständer", 10),
            Box(Binder, "Sammelordner", "Karton mit 5 Ordnern · für den Zubehör-Ständer", 5),
            Box(Playmat, "Spielmatten", "Karton mit 4 Matten · für den Zubehör-Ständer", 4),
            Box(SoulClashPack, "Soul-Clash-Display", "Karton mit 12 Soul-Clash-Boosterpacks", 12),
            Box(Soda, "Limo", "Kiste mit 12 Flaschen · für den Kühlschrank", 12),
            Box(EnergyDrink, "Energy-Drinks", "Kiste mit 12 Dosen · für den Kühlschrank", 12),
        };

        /// <summary>Tab „Einrichtung“ am Computer: Möbel und Deko, nach Store-Level freigeschaltet.</summary>
        public static readonly OrderOption[] Furnishings =
        {
            Furnishing(Furniture.Shelf, "Platz für 24 Boosterpacks", 6000, 1),
            Furnishing(Furniture.Plant, "Gemütlichkeit +2", 1500, 1),
            Furnishing(Furniture.Poster, "Für die Wand · Gemütlichkeit +1", 1000, 1),
            Furnishing(Furniture.Showcase, "Für 4 Einzelkarten – Sammler kaufen daraus", 8000, 2),
            Furnishing(Furniture.AccessoryStand, "Für Hüllen, Ordner und Spielmatten", 5000, 2),
            Furnishing(Furniture.Rug, "Man kann darüber laufen · Gemütlichkeit +3", 4000, 2),
            Furnishing(Furniture.WallClock, "Für die Wand · Gemütlichkeit +1", 2000, 2),
            Furnishing(Furniture.FairyLights, "Für die Wand, funkelt · Gemütlichkeit +3", 2500, 2),
            Furnishing(Furniture.GameTable, "4 Plätze für Turniere", 12000, 3),
            Furnishing(Furniture.PrizeCounter, "Prizepool vorbereiten und Preise verteilen", 15000, 3),
            Furnishing(Furniture.TournamentBoard, "Für die Wand · zeigt das nächste Turnier", 4000, 3),
            Furnishing(Furniture.Fridge, "Getränkekühlschrank für Limo und Energy-Drinks", 16000, 3),
            Furnishing(Furniture.Couch, "Couch-Ecke: Couch + TV + Konsole · Gemütlichkeit +3", 20000, 3),
            Furnishing(Furniture.Tv, "Für die Wand · Couch-Ecke", 25000, 3),
            Furnishing(Furniture.Console, "Spielkonsole · Couch-Ecke", 18000, 3),
        };

        /// <summary>Ein Karton Ware zum Großhandelspreis.</summary>
        static OrderOption Box(Product product, string name, string description, int amount) => new OrderOption
        {
            Name = name,
            Description = description,
            Cost = product.BuyPrice * amount,
            Kind = OrderKind.ProductBox,
            Product = product.Id,
            Amount = amount,
            MinLevel = product.MinLevel,
        };

        static OrderOption Furnishing(Furniture furniture, string description, int cost, int minLevel) => new OrderOption
        {
            Name = FurnitureCatalog.Get(furniture).Name,
            Description = description,
            Cost = cost,
            Kind = OrderKind.Furniture,
            Furniture = furniture,
            MinLevel = minLevel,
        };

        public static Product Get(ProductId id) => Products[(int)id];

        public static string GameName(TcgGame game)
        {
            switch (game)
            {
                case TcgGame.Spellforge: return "Spellforge";
                case TcgGame.SoulClash: return "Soul Clash";
                default: return "Monstera";
            }
        }
    }
}
