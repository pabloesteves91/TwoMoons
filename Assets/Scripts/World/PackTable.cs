namespace TwoMoons
{
    /// <summary>Tisch hinter der Theke: Hier öffnet man Packs aus dem eigenen Lager.</summary>
    public class PackTable : Interactable
    {
        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried == null) return "Pack-Tisch – bring ein Pack aus dem Regal oder einen Karton";
            if (carried.Product.Category == ProductCategory.Promo) return "Turnier-Packs gehören zum Turnier-Counter";
            if (carried.Product.Category != ProductCategory.Pack) return "Hier werden nur Boosterpacks geöffnet";
            return $"{carried.Product.Name} öffnen ({carried.Amount} dabei)";
        }

        public override bool CanInteract(PlayerController player) =>
            player.Carried != null && player.Carried.Amount > 0 && player.Carried.Product.Category == ProductCategory.Pack;

        public override void Interact(PlayerController player)
        {
            var carried = player.Carried;
            var product = carried.Product;
            carried.Amount--;
            if (carried.Amount <= 0) player.DestroyCarried();
            GameManager.Instance.OpenPack(product);
        }
    }
}
