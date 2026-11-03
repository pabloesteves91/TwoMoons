using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Tür zum Lagerraum in der linken Wand: Kartons, die keinen Platz im Laden haben, kommen hier hinein.
    /// Mit leeren Händen öffnet [E] das Lager zum Herausnehmen; der Mitarbeiter holt sich hier auch Nachschub.
    /// </summary>
    public class StorageDoor : Interactable
    {
        static GameManager Game => GameManager.Instance;

        /// <summary>Was in einen Lagerraum darf: alles außer Müll und Einzelstücken.</summary>
        public static bool CanStore(DeliveryBox box) =>
            box != null && !box.IsSingle && box.Product.Category != ProductCategory.Trash;

        public override string GetPrompt(PlayerController player)
        {
            var carried = player.Carried;
            if (carried == null) return $"Lagerraum öffnen ({Game.StorageTotal} Stück)";
            return CanStore(carried) ? $"Karton ins Lager stellen ({carried.Amount}× {carried.Product.Name})" : "Lagerraum – nur ganze Kartons";
        }

        public override bool CanInteract(PlayerController player) => player.Carried == null || CanStore(player.Carried);

        public override void Interact(PlayerController player)
        {
            var carried = player.Carried;
            if (carried == null)
            {
                Game.StorageOpen = true;
                return;
            }
            Game.AddToStorage(carried.Product, carried.Amount);
            player.DestroyCarried();
            SoundManager.Play(Sfx.Drop);
        }

        /// <summary>Bestand für das Mouseover-Schild.</summary>
        public static string Summary()
        {
            var lines = new List<string> { $"Lagerraum ({Game.StorageTotal} Stück)" };
            foreach (var pair in Game.Storage)
                if (pair.Value > 0) lines.Add($"{pair.Value}× {Catalog.Get(pair.Key).Name}");
            if (lines.Count == 1) lines.Add("leer – Kartons mit [E] hineinstellen");
            return string.Join("\n", lines);
        }
    }
}
