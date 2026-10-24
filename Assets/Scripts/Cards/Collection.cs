using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Eigene Karten: wie viele man gerade hat und welche man schon einmal gezogen hat (Sammelbuch).</summary>
    public class Collection
    {
        readonly Dictionary<int, int> owned = new Dictionary<int, int>();
        readonly HashSet<int> discovered = new HashSet<int>();

        public int DiscoveredCount => discovered.Count;

        public bool IsDiscovered(int cardId) => discovered.Contains(cardId);

        public int Count(int cardId) => owned.TryGetValue(cardId, out int count) ? count : 0;

        /// <summary>Gibt true zurück, wenn die Karte neu im Sammelbuch ist.</summary>
        public bool Add(int cardId)
        {
            owned[cardId] = Count(cardId) + 1;
            return discovered.Add(cardId);
        }

        public bool Remove(int cardId)
        {
            int count = Count(cardId);
            if (count <= 0) return false;
            owned[cardId] = count - 1;
            return true;
        }

        public void SaveTo(SaveData data)
        {
            data.cards.Clear();
            foreach (var card in CardDatabase.Monstera)
            {
                if (!IsDiscovered(card.Id)) continue;
                data.cards.Add(new SavedCard { card = card.Id, count = Count(card.Id) });
            }
        }

        public void LoadFrom(SaveData data)
        {
            owned.Clear();
            discovered.Clear();
            foreach (var saved in data.cards)
            {
                discovered.Add(saved.card);
                owned[saved.card] = saved.count;
            }
        }
    }
}
