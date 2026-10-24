using UnityEngine;

namespace TwoMoons
{
    public class Shelf : Interactable
    {
        public const int Capacity = 24;

        public Vector2Int Cell { get; private set; }
        public int Stock { get; private set; }
        public Product Product { get; private set; }
        /// <summary>Regal für Boosterpacks oder Zubehör-Ständer für Hüllen, Ordner und Matten.</summary>
        public ProductCategory Category { get; private set; }

        public string Label => LabelFor(Category);

        public static string LabelFor(ProductCategory category) =>
            category == ProductCategory.Accessory ? "Zubehör-Ständer"
            : category == ProductCategory.Drink ? "Kühlschrank"
            : category == ProductCategory.Print ? "Druck-Regal"
            : category == ProductCategory.Supply ? "3D-Drucker"
            : "Regal";

        SpriteRenderer[] stockIcons;

        public void Init(Vector2Int cell, SpriteRenderer[] icons, ProductCategory category)
        {
            Cell = cell;
            Category = category;
            stockIcons = icons;
            Refresh();
        }

        public override string GetPrompt(PlayerController player)
        {
            var box = player.Carried;
            if (box == null) return Stock > 0 ? $"1× {Product.Name} herausnehmen ({Stock}/{Capacity})" : $"{Label}: leer";
            if (box.Product.Category == ProductCategory.Promo) return "Turnier-Packs gehören zum Turnier-Counter";
            if (box.Product.Category == ProductCategory.Supply) return "Filament gehört in den 3D-Drucker";
            if (box.Product.Category != Category) return $"{box.Product.Name} gehört in: {LabelFor(box.Product.Category)}";
            if (Stock >= Capacity) return $"{Label} ist voll";
            if (Product != null && Product != box.Product) return $"Hier liegt schon {Product.Name}";
            return $"{box.Product.Name} einräumen";
        }

        public override bool CanInteract(PlayerController player)
        {
            var box = player.Carried;
            if (box == null) return Stock > 0;
            return box.Product.Category == Category && Stock < Capacity && (Product == null || Product == box.Product);
        }

        public override void Interact(PlayerController player)
        {
            var box = player.Carried;
            if (box == null)
            {
                // Ein Pack in die Hand nehmen, z. B. um es am Pack-Tisch zu öffnen.
                var product = Product;
                Take(1);
                player.PickUp(WorldFactory.SpawnBox(Cell, product, 1));
                return;
            }

            int moved = Mathf.Min(Capacity - Stock, box.Amount);
            Product = box.Product;
            Stock += moved;
            box.Amount -= moved;
            if (box.Amount <= 0) player.DestroyCarried();
            Refresh();
            SoundManager.Play(Sfx.Stock);
            GameManager.Instance.NotifyShelfStocked(moved);
        }

        /// <summary>Passt diese Ware hier hinein (richtige Sorte und noch Platz)?</summary>
        public bool CanTake(Product product) =>
            product.Category == Category && Stock < Capacity && (Product == null || Product == product);

        /// <summary>Für den Mitarbeiter: so viel wie möglich aus einem Karton einräumen.</summary>
        public void StockFrom(DeliveryBox box)
        {
            if (!CanTake(box.Product)) return;
            int moved = Mathf.Min(Capacity - Stock, box.Amount);
            Product = box.Product;
            Stock += moved;
            box.Amount -= moved;
            Refresh();
            SoundManager.Play(Sfx.Stock);
        }

        /// <summary>Bestand direkt setzen (Spielstand laden, Hauptmenü).</summary>
        public void SetStock(Product product, int stock)
        {
            Product = product;
            Stock = Mathf.Clamp(stock, 0, Capacity);
            Refresh();
        }

        /// <summary>Kunde nimmt Ware. Gibt zurück, wie viele Packs er wirklich bekommen hat.</summary>
        public int Take(int wanted)
        {
            int taken = Mathf.Min(wanted, Stock);
            Stock -= taken;
            Refresh();
            return taken;
        }

        public void PutBack(Product product, int amount)
        {
            if (amount <= 0) return;
            if (Product == null) Product = product;
            if (Product != product) return;
            Stock = Mathf.Min(Capacity, Stock + amount);
            Refresh();
        }

        void Refresh()
        {
            if (Stock == 0) Product = null;
            if (stockIcons == null) return;
            for (int i = 0; i < stockIcons.Length; i++)
            {
                stockIcons[i].enabled = Stock > i * Capacity / stockIcons.Length;
                if (Product != null) stockIcons[i].sprite = Art.Get(Product.SpriteKey);
            }
        }
    }
}
