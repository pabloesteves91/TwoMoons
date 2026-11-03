using System.Collections.Generic;
using System.Text;

namespace TwoMoons
{
    /// <summary>
    /// Lieferpaket mit gekauften Möbeln und Deko. Alles, was man kauft, landet im selben Paket am Eingang.
    /// [E] packt es aus und startet direkt den Bau-Modus – man muss nichts tragen.
    /// </summary>
    public class FurniturePackage : Interactable
    {
        public readonly Dictionary<Furniture, int> Contents = new Dictionary<Furniture, int>();

        public void Add(Furniture furniture, int amount = 1) =>
            Contents[furniture] = (Contents.TryGetValue(furniture, out int count) ? count : 0) + amount;

        public string Describe()
        {
            var text = new StringBuilder();
            foreach (var pair in Contents)
            {
                if (pair.Value <= 0) continue;
                if (text.Length > 0) text.Append(", ");
                text.Append(FurnitureCatalog.Get(pair.Key).Name);
                if (pair.Value > 1) text.Append($" ×{pair.Value}");
            }
            return text.ToString();
        }

        public override string GetPrompt(PlayerController player) => $"Paket auspacken: {Describe()}";

        public override bool CanInteract(PlayerController player) => player.Carried == null;

        public override void Interact(PlayerController player)
        {
            var game = GameManager.Instance;
            foreach (var pair in Contents) game.AddToPlace(pair.Key, pair.Value);
            game.Package = null;
            Destroy(gameObject);
            SoundManager.Play(Sfx.Drop);
            BuildController.StartRequested = true;
        }
    }
}
