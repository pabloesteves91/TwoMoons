using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Kasse mit Warteschlange. Der Spieler kassiert jeden Kunden selbst mit [E].</summary>
    public class CashRegister : Interactable
    {
        readonly List<Customer> queue = new List<Customer>();

        Customer Front => queue.Count > 0 && queue[0].IsWaitingAtRegister ? queue[0] : null;

        public bool TryJoin(Customer customer)
        {
            if (queue.Count >= ShopGrid.QueueCells.Length) return false;
            queue.Add(customer);
            customer.SetQueueIndex(queue.Count - 1);
            return true;
        }

        public void Remove(Customer customer)
        {
            int index = queue.IndexOf(customer);
            if (index < 0) return;
            queue.RemoveAt(index);
            for (int i = index; i < queue.Count; i++) queue[i].SetQueueIndex(i);
        }

        public override string GetPrompt(PlayerController player)
        {
            var front = Front;
            return front != null ? $"Abkassieren ({Money.Format(front.Total)})" : "Kasse – kein Kunde wartet";
        }

        public override bool CanInteract(PlayerController player) => Front != null;

        public override void Interact(PlayerController player)
        {
            var front = Front;
            if (front != null) front.Pay();
        }
    }
}
