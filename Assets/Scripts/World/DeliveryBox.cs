using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Lieferkarton mit Ware. Kann aufgehoben, getragen und ins Regal geräumt werden.</summary>
    public class DeliveryBox : Interactable
    {
        public static readonly List<DeliveryBox> Existing = new List<DeliveryBox>();

        public Product Product;
        public int Amount;

        void Awake() => Existing.Add(this);

        void OnDestroy() => Existing.Remove(this);

        public override string GetPrompt(PlayerController player) =>
            player.Carried == null ? $"Karton aufheben ({Amount}× {Product.Name})" : null;

        public override void Interact(PlayerController player) => player.PickUp(this);
    }
}
