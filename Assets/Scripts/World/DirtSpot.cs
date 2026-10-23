namespace TwoMoons
{
    public class DirtSpot : Interactable
    {
        public override string GetPrompt(PlayerController player) => "Fleck wegputzen";

        public override void Interact(PlayerController player)
        {
            SoundManager.Play(Sfx.Clean);
            GameManager.Instance.NotifyDirtCleaned();
            Destroy(gameObject);
        }
    }
}
