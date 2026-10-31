using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Lieferkarton mit Ware. Kann aufgehoben, getragen und ins Regal geräumt werden.</summary>
    public class DeliveryBox : Interactable
    {
        public static readonly List<DeliveryBox> Existing = new List<DeliveryBox>();

        public Product Product;
        public int Amount;

        /// <summary>Einzelstück aus dem Regal (z. B. ein Booster) statt eines Kartons – sieht aus wie die Ware selbst.</summary>
        public bool IsSingle;

        SpriteRenderer look;
        bool? shownSingle;

        void Awake() => Existing.Add(this);

        void OnDestroy() => Existing.Remove(this);

        void Update() => RefreshLook();

        /// <summary>Ein einzelnes Stück sieht aus wie die Ware selbst, mehrere wie ein Karton.</summary>
        public void RefreshLook()
        {
            if (Product == null || shownSingle == IsSingle) return;
            if (look == null) look = GetComponent<SpriteRenderer>();
            shownSingle = IsSingle;
            look.sprite = Art.Get(IsSingle ? Product.SpriteKey : "box");
            transform.localScale = Vector3.one * (IsSingle ? 0.7f : 1f);
        }

        public override string GetPrompt(PlayerController player) =>
            player.Carried == null ? $"{(IsSingle ? Product.Name : "Karton")} aufheben ({Amount}× {Product.Name})" : null;

        public override void Interact(PlayerController player) => player.PickUp(this);
    }
}
