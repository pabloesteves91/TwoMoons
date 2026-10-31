using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Fleck auf dem Boden. Schmutz macht Kunden ungeduldiger.</summary>
    public class DirtSpot : Interactable
    {
        public static readonly List<DirtSpot> Existing = new List<DirtSpot>();

        void Awake() => Existing.Add(this);

        void OnDestroy() => Existing.Remove(this);

        public override string GetPrompt(PlayerController player) => "Fleck wegputzen";

        public override void Interact(PlayerController player)
        {
            SoundManager.Play(Sfx.Clean);
            GameManager.Instance.NotifyDirtCleaned();
            Destroy(gameObject);
        }
    }
}
