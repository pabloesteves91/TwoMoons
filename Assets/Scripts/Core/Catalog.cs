namespace TwoMoons
{
    public enum ProductId { MonsteraPack }

    public enum OrderKind { ProductBox, Shelf }

    public class Product
    {
        public ProductId Id;
        public string Name;
        public string SpriteKey;
        public int SellPrice;
    }

    public class OrderOption
    {
        public string Name;
        public string Description;
        public int Cost;
        public OrderKind Kind;
        public ProductId Product;
        public int Amount;
    }

    /// <summary>Alle Waren und Bestellmöglichkeiten. Spellforge, Soul Clash und Zubehör kommen in späteren Kapiteln dazu.</summary>
    public static class Catalog
    {
        public static readonly Product MonsteraPack = new Product
        {
            Id = ProductId.MonsteraPack,
            Name = "Monstera-Booster",
            SpriteKey = "packMonstera",
            SellPrice = 400,
        };

        public static readonly OrderOption[] Orders =
        {
            new OrderOption
            {
                Name = "Monstera-Display",
                Description = "Karton mit 12 Monstera-Boosterpacks",
                Cost = 3000,
                Kind = OrderKind.ProductBox,
                Product = ProductId.MonsteraPack,
                Amount = 12,
            },
            new OrderOption
            {
                Name = "Regal",
                Description = "Platz für 24 Boosterpacks",
                Cost = 6000,
                Kind = OrderKind.Shelf,
            },
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
