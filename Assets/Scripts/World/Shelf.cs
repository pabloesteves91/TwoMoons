using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public class Shelf : Interactable
    {
        public const int Capacity = 24;

        public Vector2Int Cell { get; private set; }
        /// <summary>Nach dem Verschieben im Bau-Modus.</summary>
        public void SetCell(Vector2Int cell) => Cell = cell;
        /// <summary>Inhalt in der Reihenfolge des Einräumens. Normale Regale haben genau eine Sorte, das Druck-Regal darf mischen.</summary>
        readonly List<Product> order = new List<Product>();
        readonly Dictionary<Product, int> counts = new Dictionary<Product, int>();

        public int Stock { get; private set; }
        /// <summary>Die (erste) Ware im Regal – bei gemischten Regalen die, die zuerst eingeräumt wurde.</summary>
        public Product Product => order.Count > 0 ? order[0] : null;
        /// <summary>Im Druck-Regal dürfen verschiedene Figuren nebeneinander stehen.</summary>
        public bool Mixed => Category == ProductCategory.Print;

        public int CountOf(Product product) => counts.TryGetValue(product, out int count) ? count : 0;

        /// <summary>Alle Sorten mit Anzahl (für Anzeige und Spielstand).</summary>
        public IEnumerable<KeyValuePair<Product, int>> Contents
        {
            get
            {
                foreach (var product in order) yield return new KeyValuePair<Product, int>(product, counts[product]);
            }
        }

        /// <summary>Was sich ein Kunde aussucht: bei gemischten Regalen zufällig eine der Sorten.</summary>
        public Product PickProduct() => order.Count == 0 ? null : Mixed ? order[Random.Range(0, order.Count)] : order[0];

        void Add(Product product, int amount)
        {
            if (amount <= 0) return;
            if (!counts.ContainsKey(product))
            {
                counts[product] = 0;
                order.Add(product);
            }
            counts[product] += amount;
            Stock += amount;
        }

        int Remove(Product product, int wanted)
        {
            int taken = Mathf.Min(wanted, CountOf(product));
            if (taken <= 0) return 0;
            counts[product] -= taken;
            Stock -= taken;
            if (counts[product] <= 0)
            {
                counts.Remove(product);
                order.Remove(product);
            }
            return taken;
        }
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
            if (!Fits(box.Product.Category)) return $"{box.Product.Name} gehört in: {LabelFor(box.Product.Category)}";
            if (Stock >= Capacity) return $"{Label} ist voll";
            if (!Mixed && Product != null && Product != box.Product) return $"Hier liegt schon {Product.Name}";
            return $"{box.Product.Name} einräumen";
        }

        public override bool CanInteract(PlayerController player)
        {
            var box = player.Carried;
            if (box == null) return Stock > 0;
            return CanTake(box.Product);
        }

        public override void Interact(PlayerController player)
        {
            var box = player.Carried;
            if (box == null)
            {
                // Ein Pack in die Hand nehmen, z. B. um es am Pack-Tisch zu öffnen.
                var product = Product;
                Remove(product, 1);
                Refresh();
                player.PickUp(WorldFactory.SpawnSingle(Cell, product));
                return;
            }

            int moved = Mathf.Min(Capacity - Stock, box.Amount);
            Add(box.Product, moved);
            box.Amount -= moved;
            if (box.Amount <= 0) player.DestroyCarried();
            Refresh();
            SoundManager.Play(Sfx.Stock);
            GameManager.Instance.NotifyShelfStocked(moved);
        }

        /// <summary>Passt diese Ware hier hinein (richtige Sorte und noch Platz)?</summary>
        public bool CanTake(Product product) =>
            Fits(product.Category) && Stock < Capacity && (Mixed || Product == null || Product == product);

        /// <summary>Gehört diese Warenart in dieses Regal? Brettspiele stehen im normalen Regal bei den Boostern.</summary>
        public bool Fits(ProductCategory category) =>
            category == Category || (Category == ProductCategory.Pack && category == ProductCategory.BoardGame);

        /// <summary>Für den Mitarbeiter: so viel wie möglich aus einem Karton einräumen.</summary>
        public void StockFrom(DeliveryBox box)
        {
            if (!CanTake(box.Product)) return;
            int moved = Mathf.Min(Capacity - Stock, box.Amount);
            Add(box.Product, moved);
            box.Amount -= moved;
            Refresh();
            SoundManager.Play(Sfx.Stock);
        }

        /// <summary>Bestand direkt setzen (Spielstand laden, Hauptmenü).</summary>
        public void SetStock(Product product, int stock)
        {
            order.Clear();
            counts.Clear();
            Stock = 0;
            if (product != null) Add(product, Mathf.Clamp(stock, 0, Capacity));
            Refresh();
        }

        /// <summary>Weitere Sorte dazulegen (Spielstand laden, gemischtes Druck-Regal).</summary>
        public void AddStock(Product product, int amount)
        {
            Add(product, Mathf.Clamp(amount, 0, Capacity - Stock));
            Refresh();
        }

        /// <summary>Kunde nimmt Ware. Gibt zurück, wie viele er wirklich bekommen hat.</summary>
        public int Take(Product product, int wanted)
        {
            int taken = Remove(product, wanted);
            Refresh();
            return taken;
        }

        public void PutBack(Product product, int amount)
        {
            if (amount <= 0 || !Fits(product.Category)) return;
            if (!Mixed && Product != null && Product != product) return;
            Add(product, Mathf.Min(amount, Capacity - Stock));
            Refresh();
        }

        void Refresh()
        {
            if (stockIcons == null) return;
            for (int i = 0; i < stockIcons.Length; i++)
            {
                // Gemischtes Regal: die Symbole zeigen die verschiedenen Sorten nebeneinander.
                stockIcons[i].enabled = Mixed ? i < Stock : Stock > i * Capacity / stockIcons.Length;
                if (order.Count > 0) stockIcons[i].sprite = Art.Get(order[Mixed ? i % order.Count : 0].SpriteKey);
            }
        }
    }
}
