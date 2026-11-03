using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Müll, den Kunden auf einem Tisch liegen lassen. Mit [E] einsammeln und in den Mülleimer werfen.
    /// Liegt zu viel herum, setzt sich niemand mehr an den Tisch.
    /// </summary>
    public class TableTrash : Interactable
    {
        public const int Max = 3;
        /// <summary>Ab so viel Müll setzt sich keiner mehr hin.</summary>
        public const int TooMessy = 2;

        public int Count { get; private set; }

        SpriteRenderer pile;

        public void Init()
        {
            pile = WorldFactory.CreateSprite("Müll", "wrappers", transform.position + new Vector3(0.15f, 0.2f, 0f), 2, transform);
            pile.transform.localScale = Vector3.one * 0.55f;
            GetComponent<YSort>()?.Collect();
            Refresh();
        }

        public void Add()
        {
            Count = Mathf.Min(Max, Count + 1);
            Refresh();
        }

        public void Restore(int count)
        {
            Count = Mathf.Clamp(count, 0, Max);
            Refresh();
        }

        void Refresh()
        {
            if (pile == null) return;
            pile.enabled = Count > 0;
            pile.transform.localScale = Vector3.one * (0.4f + 0.15f * Count);
        }

        public override string GetPrompt(PlayerController player) => Count > 0 ? $"Müll vom Tisch einsammeln ({Count})" : null;

        public override bool CanInteract(PlayerController player) => Count > 0 && player.Carried == null;

        public override void Interact(PlayerController player)
        {
            var wrappers = WorldFactory.SpawnSingle(ShopGrid.WorldToCell(transform.position), Catalog.Wrappers);
            wrappers.Amount = Count;
            Count = 0;
            Refresh();
            player.PickUp(wrappers);
            SoundManager.Play(Sfx.Clean);
            GameManager.Instance.NotifyTableCleaned();
        }
    }
}
