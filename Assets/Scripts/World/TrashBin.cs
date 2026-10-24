using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Mülleimer (ab Level 2): Kunden werfen hier die Folie ihrer geöffneten Booster hinein.
    /// Ist der Sack voll, nimmt man ihn mit [E] heraus (ein neuer leerer ist sofort drin) und bringt ihn zum Container vor der Tür.
    /// </summary>
    public class TrashBin : Interactable
    {
        public const int Capacity = 8;

        public Vector2Int Cell { get; private set; }
        /// <summary>Nach dem Verschieben im Bau-Modus.</summary>
        public void SetCell(Vector2Int cell) => Cell = cell;

        public int Fill { get; private set; }
        public bool IsFull => Fill >= Capacity;

        SpriteRenderer look;

        public void Init(Vector2Int cell)
        {
            Cell = cell;
            look = GetComponent<SpriteRenderer>();
            Refresh();
        }

        public void Restore(int fill)
        {
            Fill = Mathf.Clamp(fill, 0, Capacity);
            Refresh();
        }

        /// <summary>Ein Kunde wirft Müll hinein. False, wenn der Eimer schon voll ist.</summary>
        public bool Add()
        {
            if (IsFull) return false;
            Fill++;
            Refresh();
            if (IsFull) GameManager.Instance.ShowToast("Ein Mülleimer ist voll – Sack mit [E] herausnehmen und zum Container vor der Tür bringen.", 5f);
            return true;
        }

        void Refresh()
        {
            if (look != null) look.sprite = Art.Get(IsFull ? "trashBinFull" : "trashBin");
        }

        public override string GetPrompt(PlayerController player) =>
            IsFull ? "Vollen Müllsack herausnehmen" : $"Mülleimer ({Fill}/{Capacity})";

        public override bool CanInteract(PlayerController player) => IsFull && player.Carried == null;

        public override void Interact(PlayerController player)
        {
            // Sack raus, neuer leerer Sack rein.
            Fill = 0;
            Refresh();
            player.PickUp(WorldFactory.SpawnSingle(Cell, Catalog.TrashBag));
        }
    }
}
