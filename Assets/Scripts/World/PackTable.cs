namespace TwoMoons
{
    /// <summary>Tisch hinter der Theke: Hier öffnet man Packs aus dem eigenen Lager.</summary>
    public class PackTable : Interactable
    {
        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried == null) return "Pack-Tisch – bring ein Pack aus dem Regal oder einen Karton";
            return $"Pack öffnen ({carried.Amount}× {carried.Product.Name} dabei)";
        }

        public override bool CanInteract(PlayerController player) => player.Carried != null && player.Carried.Amount > 0;

        public override void Interact(PlayerController player)
        {
            var carried = player.Carried;
            carried.Amount--;
            if (carried.Amount <= 0) player.DestroyCarried();
            GameManager.Instance.OpenPack();
        }
    }
}
