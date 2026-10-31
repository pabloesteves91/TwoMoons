using UnityEngine;

namespace TwoMoons
{
    /// <summary>Turnier-Counter: Hersteller-Packs abgeben, Prizepool einteilen, Siegerehrung starten.</summary>
    public class PrizeCounter : Interactable
    {
        public Vector2Int Cell { get; private set; }
        /// <summary>Nach dem Verschieben im Bau-Modus.</summary>
        public void SetCell(Vector2Int cell) => Cell = cell;

        public void Init(Vector2Int cell) => Cell = cell;

        static TournamentManager Tournament => GameManager.Instance.Tournament;

        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried != null && carried.Product.Category == ProductCategory.Promo)
                return $"{carried.Amount} Turnier-Packs in den Prizepool legen";
            if (Tournament.Current == TournamentManager.State.Ceremony) return "Siegerehrung starten!";
            return $"Prizepool einteilen ({Tournament.PrizePackTotal} Packs)";
        }

        public override bool CanInteract(PlayerController player)
        {
            var carried = player.Carried;
            return carried == null || carried.Product.Category == ProductCategory.Promo;
        }

        public override void Interact(PlayerController player)
        {
            var game = GameManager.Instance;
            var carried = player.Carried;
            if (carried != null)
            {
                Tournament.AddPrizePacks(carried.Amount);
                player.DestroyCarried();
                SoundManager.Play(Sfx.Stock);
                game.ShowToast("Prizepool automatisch aufgeteilt – mit [E] am Counter anpassen.", 5f);
                return;
            }
            game.PrizeCounterOpen = true;
        }
    }
}
