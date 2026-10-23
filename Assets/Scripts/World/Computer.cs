namespace TwoMoons
{
    /// <summary>Computer hinter der Theke: Hier wird Ware bestellt.</summary>
    public class Computer : Interactable
    {
        public override string GetPrompt(PlayerController player) => "Computer: Ware bestellen";

        public override void Interact(PlayerController player) => GameManager.Instance.ComputerOpen = true;
    }
}
