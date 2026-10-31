namespace TwoMoons
{
    /// <summary>Schild an der Tür: Damit wird der Laden morgens geöffnet.</summary>
    public class ShopSign : Interactable
    {
        public override string GetPrompt(PlayerController player)
        {
            switch (GameManager.Instance.Phase)
            {
                case GamePhase.Preparation: return "Laden öffnen";
                case GamePhase.Open: return "Geöffnet bis 18:00";
                default: return "Geschlossen";
            }
        }

        public override bool CanInteract(PlayerController player) =>
            GameManager.Instance.Phase == GamePhase.Preparation;

        public override void Interact(PlayerController player) => GameManager.Instance.OpenShop();
    }
}
