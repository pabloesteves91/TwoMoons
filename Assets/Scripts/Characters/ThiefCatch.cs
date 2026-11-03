namespace TwoMoons
{
    /// <summary>Macht einen schleichenden Ladendieb für den Spieler ansprechbar: [E] = erwischt.</summary>
    public class ThiefCatch : Interactable
    {
        Customer thief;

        public void Init(Customer customer) => thief = customer;

        public override string GetPrompt(PlayerController player) => thief != null && thief.IsSneaking ? "Ladendieb erwischen!" : null;

        public override void Interact(PlayerController player) => thief.Caught();
    }
}
