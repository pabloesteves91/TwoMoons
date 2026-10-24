namespace TwoMoons
{
    /// <summary>Großer Müllcontainer draußen links vom Eingang: volle Müllsäcke hier hineinwerfen.</summary>
    public class TrashContainer : Interactable
    {
        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried != null && carried.Product.Category == ProductCategory.Trash) return "Müllsack in den Container werfen";
            return "Müllcontainer";
        }

        public override bool CanInteract(PlayerController player) =>
            player.Carried != null && player.Carried.Product.Category == ProductCategory.Trash;

        public override void Interact(PlayerController player)
        {
            player.DestroyCarried();
            SoundManager.Play(Sfx.Drop);
            GameManager.Instance.TrashDisposed();
        }
    }
}
