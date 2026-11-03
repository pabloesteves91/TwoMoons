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

        static bool CarriesWrappers(PlayerController player) =>
            player.Carried != null && player.Carried.Product == Catalog.Wrappers;

        public override string GetPrompt(PlayerController player)
        {
            if (CarriesWrappers(player)) return IsFull ? "Mülleimer voll – erst den Sack herausnehmen" : "Folien in den Mülleimer werfen";
            return IsFull ? "Vollen Müllsack herausnehmen" : $"Mülleimer ({Fill}/{Capacity})";
        }

        public override bool CanInteract(PlayerController player) =>
            CarriesWrappers(player) ? !IsFull : IsFull && player.Carried == null;

        public override void Interact(PlayerController player)
        {
            if (CarriesWrappers(player))
            {
                // Vom Tisch eingesammelte Folien: so viel wie reinpasst, der Rest bleibt in der Hand.
                var wrappers = player.Carried;
                int put = Mathf.Min(wrappers.Amount, Capacity - Fill);
                for (int i = 0; i < put; i++) Add();
                wrappers.Amount -= put;
                if (wrappers.Amount <= 0) player.DestroyCarried();
                SoundManager.Play(Sfx.Drop);
                return;
            }
            // Sack raus, neuer leerer Sack rein.
            Fill = 0;
            Refresh();
            player.PickUp(WorldFactory.SpawnSingle(Cell, Catalog.TrashBag));
        }
    }
}
