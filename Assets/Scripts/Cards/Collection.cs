using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Eigene Karten: wie viele man gerade hat und welche man schon einmal gezogen hat (Sammelbuch).</summary>
    public class Collection
    {
        readonly Dictionary<int, int> owned = new Dictionary<int, int>();
        readonly HashSet<int> discovered = new HashSet<int>();
        readonly Dictionary<int, int> bestGrades = new Dictionary<int, int>();

        /// <summary>Beste Note, die diese Karte je bekommen hat (0 = nie bewertet).</summary>
        public int BestGrade(int cardId) => bestGrades.TryGetValue(cardId, out int grade) ? grade : 0;

        public void RecordGrade(int cardId, int grade)
        {
            if (grade > BestGrade(cardId)) bestGrades[cardId] = grade;
        }

        public int DiscoveredCount => discovered.Count;

        public int DiscoveredIn(TcgGame game)
        {
            int count = 0;
            foreach (int id in discovered)
                if (CardDatabase.Get(id)?.Game == game) count++;
            return count;
        }

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
            foreach (var card in CardDatabase.All)
            {
                if (!IsDiscovered(card.Id)) continue;
                data.cards.Add(new SavedCard { card = card.Id, count = Count(card.Id) });
            }
            data.bestGrades.Clear();
            foreach (var pair in bestGrades) data.bestGrades.Add(new SavedCard { card = pair.Key, count = pair.Value });
        }

        public void LoadFrom(SaveData data)
        {
            owned.Clear();
            discovered.Clear();
            bestGrades.Clear();
            foreach (var saved in data.cards)
            {
                discovered.Add(saved.card);
                owned[saved.card] = saved.count;
            }
            if (data.bestGrades != null) foreach (var saved in data.bestGrades) bestGrades[saved.card] = saved.count;
        }
    }
}
