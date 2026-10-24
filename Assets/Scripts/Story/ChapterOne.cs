using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public class Quest
    {
        public string Text;
        public int Target = 1;
        public int Progress;
        public bool Done => Progress >= Target;
    }

    /// <summary>Kapitel 1: Der Traum beginnt.</summary>
    public class ChapterOne : MonoBehaviour
    {
        public const string Title = "Kapitel 1: Der Traum beginnt";

        public readonly List<Quest> Quests = new List<Quest>
        {
            new Quest { Text = "Laden putzen", Target = 5 },
            new Quest { Text = "Erstes Regal aufstellen" },
            new Quest { Text = "Monstera-Packs einräumen", Target = 12 },
            new Quest { Text = "Laden öffnen" },
            new Quest { Text = "Kunden bedienen", Target = 5 },
        };

        public bool Completed { get; private set; }

        void Start()
        {
            var game = GameManager.Instance;
            game.DirtCleaned += () => Advance(0, 1);
            game.ShelfPlaced += () => Advance(1, 1);
            game.ShelfStocked += packs => Advance(2, packs);
            game.ShopOpened += () => Advance(3, 1);
            game.CustomerServed += () => Advance(4, 1);
        }

        public int[] SaveProgress() => Quests.ConvertAll(q => q.Progress).ToArray();

        public void RestoreProgress(int[] progress)
        {
            if (progress == null) return;
            for (int i = 0; i < Quests.Count && i < progress.Length; i++)
                Quests[i].Progress = Mathf.Clamp(progress[i], 0, Quests[i].Target);
            Completed = Quests.TrueForAll(q => q.Done);
        }

        void Advance(int index, int amount)
        {
            var quest = Quests[index];
            if (Completed || quest.Done) return;

            var game = GameManager.Instance;
            quest.Progress = Mathf.Min(quest.Target, quest.Progress + amount);
            if (!quest.Done) return;
            game.ShowToast($"Aufgabe erledigt: {quest.Text} (+20 XP)");
            game.AddXp(20);

            if (!Quests.TrueForAll(q => q.Done))
            {
                SoundManager.Play(Sfx.Quest);
                return;
            }
            Completed = true;
            SoundManager.Play(Sfx.Fanfare);
            game.ChangeReputation(10);
            game.ShowDialog(new[]
            {
                $"{game.PlayerName}: Geschafft! Die ersten Kunden sind zufrieden nach Hause gegangen.",
                "TwoMoons ist wirklich eröffnet. Jetzt muss ich dafür sorgen, dass sie wiederkommen …",
                "Kapitel 1 abgeschlossen! Kapitel 2 „Die ersten Stammkunden“ folgt bald. Bis dahin kannst du weiterspielen.",
            });
        }
    }
}
