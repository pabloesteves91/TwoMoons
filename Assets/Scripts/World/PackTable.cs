namespace TwoMoons
{
    /// <summary>Tisch hinter der Theke: Hier öffnet man Packs aus dem eigenen Lager.</summary>
    public class PackTable : Interactable
    {
        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried == null) return "Pack-Tisch – nimm einen Booster aus dem Regal und bring ihn her";
            if (carried.Product.Category == ProductCategory.Promo) return "Turnier-Packs gehören zum Turnier-Counter";
            if (carried.Product.Category == ProductCategory.Supply) return "Filament gehört in den 3D-Drucker";
            if (carried.Product.Category != ProductCategory.Pack) return "Hier werden nur Boosterpacks geöffnet";
            if (!carried.IsSingle) return "Erst einräumen – geöffnet werden Booster aus dem Regal";
            return $"{carried.Product.Name} öffnen (Einkaufspreis {Money.Format(carried.Product.BuyPrice)})";
        }

        public override bool CanInteract(PlayerController player) =>
            player.Carried != null && player.Carried.IsSingle && player.Carried.Product.Category == ProductCategory.Pack;

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
