namespace TwoMoons
{
    public enum ProductId { MonsteraPack }

    public enum OrderKind { ProductBox, Furniture }

    public class Product
    {
        public ProductId Id;
        public string Name;
        public string SpriteKey;
        /// <summary>Einkaufspreis pro Stück beim Großhändler.</summary>
        public int BuyPrice;
        /// <summary>Was Kunden für fair halten. Startwert für den Verkaufspreis.</summary>
        public int MarketValue;
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

    /// <summary>Alle Waren und Bestellmöglichkeiten. Spellforge, Soul Clash und Zubehör kommen in späteren Kapiteln dazu.</summary>
    public static class Catalog
    {
        public static readonly Product MonsteraPack = new Product
        {
            Id = ProductId.MonsteraPack,
            Name = "Monstera-Booster",
            SpriteKey = "packMonstera",
            BuyPrice = 300,
            MarketValue = 400,
        };

        /// <summary>Alles, wofür man am Computer einen Preis festlegen kann.</summary>
        public static readonly Product[] Products = { MonsteraPack };

        /// <summary>Tab „Ware“ am Computer.</summary>
        public static readonly OrderOption[] Orders =
        {
            new OrderOption
            {
                Name = "Monstera-Display",
                Description = "Karton mit 12 Monstera-Boosterpacks",
                Cost = 3600,
                Kind = OrderKind.ProductBox,
                Product = ProductId.MonsteraPack,
                Amount = 12,
            },
        };

        /// <summary>Tab „Einrichtung“ am Computer: Möbel und Deko, nach Store-Level freigeschaltet.</summary>
        public static readonly OrderOption[] Furnishings =
        {
            Furnishing(Furniture.Shelf, "Platz für 24 Boosterpacks", 6000, 1),
            Furnishing(Furniture.Plant, "Gemütlichkeit +2", 1500, 1),
            Furnishing(Furniture.Poster, "Für die Wand · Gemütlichkeit +1", 1000, 1),
            Furnishing(Furniture.Showcase, "Für 4 Einzelkarten – Sammler kaufen daraus", 8000, 2),
            Furnishing(Furniture.Rug, "Man kann darüber laufen · Gemütlichkeit +3", 4000, 2),
            Furnishing(Furniture.WallClock, "Für die Wand · Gemütlichkeit +1", 2000, 2),
            Furnishing(Furniture.FairyLights, "Für die Wand, funkelt · Gemütlichkeit +3", 2500, 2),
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

        public static Product Get(ProductId id)
        {
            switch (id)
            {
                case ProductId.MonsteraPack: return MonsteraPack;
                default: throw new System.ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }
    }
}
