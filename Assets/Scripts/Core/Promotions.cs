using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Tagesaktionen, morgens vor Ladenöffnung am Computer (Tab „Preise“) einstellbar:
    /// Rabatt pro Warengruppe, Flyer-Werbung (mehr Kunden) und „3 für 2“ auf Booster.
    /// Gilt für einen Tag und wird am nächsten Morgen zurückgesetzt.
    /// </summary>
    public class Promotions
    {
        public static readonly int[] DiscountSteps = { 0, 10, 20, 30 };
        public const int FlyerCost = 2000;

        /// <summary>Warengruppen, auf die es Rabatt geben kann.</summary>
        public static readonly ProductCategory[] Categories =
            { ProductCategory.Pack, ProductCategory.Accessory, ProductCategory.Drink, ProductCategory.Print, ProductCategory.BoardGame };

        readonly Dictionary<ProductCategory, int> discounts = new Dictionary<ProductCategory, int>();
        public bool Flyer { get; private set; }
        /// <summary>„3 für 2“: Jeder dritte Booster ist gratis.</summary>
        public bool BundleDeal;

        public int Discount(ProductCategory category) => discounts.TryGetValue(category, out int percent) ? percent : 0;

        public void SetDiscount(ProductCategory category, int percent) => discounts[category] = percent;

        public bool Any
        {
            get
            {
                if (Flyer || BundleDeal) return true;
                foreach (var pair in discounts) if (pair.Value > 0) return true;
                return false;
            }
        }

        /// <summary>Was der Kunde für ein Stück zahlt: dein Preis minus Rabatt.</summary>
        public int PriceAfterDiscount(Product product, int price) =>
            Mathf.RoundToInt(price * (100 - Discount(product.Category)) / 100f);

        /// <summary>Wie viele Stück bezahlt werden (bei „3 für 2“ ist jeder dritte Booster gratis).</summary>
        public int PaidCount(Product product, int count) =>
            BundleDeal && product != null && product.Category == ProductCategory.Pack ? count - count / 3 : count;

        public void SetFlyer(bool on) => Flyer = on;

        public void Clear()
        {
            discounts.Clear();
            Flyer = false;
            BundleDeal = false;
        }

        public void Save(SaveData data)
        {
            data.promoDiscounts.Clear();
            foreach (var pair in discounts)
                if (pair.Value > 0) data.promoDiscounts.Add(new SavedCard { card = (int)pair.Key, count = pair.Value });
            data.promoFlyer = Flyer;
            data.promoBundle = BundleDeal;
        }

        public void Load(SaveData data)
        {
            Clear();
            if (data.promoDiscounts != null)
                foreach (var saved in data.promoDiscounts) discounts[(ProductCategory)saved.card] = saved.count;
            Flyer = data.promoFlyer;
            BundleDeal = data.promoBundle;
        }
    }
}
