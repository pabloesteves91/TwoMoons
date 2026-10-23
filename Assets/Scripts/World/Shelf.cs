using UnityEngine;

namespace TwoMoons
{
    public class Shelf : Interactable
    {
        public const int Capacity = 24;

        public Vector2Int Cell { get; private set; }
        public int Stock { get; private set; }
        public Product Product { get; private set; }

        SpriteRenderer[] stockIcons;

        public void Init(Vector2Int cell, SpriteRenderer[] icons)
        {
            Cell = cell;
            stockIcons = icons;
            Refresh();
        }

        public override string GetPrompt(PlayerController player)
        {
            var box = player.Carried;
            if (box == null) return $"Regal: {Stock}/{Capacity} Packs";
            if (Stock >= Capacity) return "Regal ist voll";
            if (Product != null && Product != box.Product) return $"Hier liegt schon {Product.Name}";
            return $"{box.Product.Name} einräumen";
        }

        public override bool CanInteract(PlayerController player)
        {
            var box = player.Carried;
            return box != null && Stock < Capacity && (Product == null || Product == box.Product);
        }

        public override void Interact(PlayerController player)
        {
            var box = player.Carried;
            int moved = Mathf.Min(Capacity - Stock, box.Amount);
            Product = box.Product;
            Stock += moved;
            box.Amount -= moved;
            if (box.Amount <= 0) player.DestroyCarried();
            Refresh();
            GameManager.Instance.NotifyShelfStocked(moved);
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
