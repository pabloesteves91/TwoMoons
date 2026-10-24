using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public class Quest
    {
        public string Text;
        public int Target = 1;
        public int Progress;
        /// <summary>Welches Ereignis die Aufgabe voranbringt (siehe <see cref="ChapterOne"/>).</summary>
        public string Key;
        /// <summary>Für Aufgaben, die einen Zustand prüfen (z. B. Gemütlichkeit) statt Ereignisse zu zählen.</summary>
        public Func<int> Poll;
        public bool Done => Progress >= Target;
    }

    public class QuestStage
    {
        public string Title;
        public List<Quest> Quests;
        /// <summary>Kleiner Story-Moment, wenn diese Etappe geschafft ist ({0} = Name).</summary>
        public string[] Outro;
    }

    /// <summary>
    /// Kapitel 1: Der Traum beginnt. Vier Etappen, zusammen etwa 6–7 Spieltage (rund 1 Stunde).
    /// Es ist immer nur eine Etappe mit 3–5 Aufgaben aktiv.
    /// </summary>
    public class ChapterOne : MonoBehaviour
    {
        public const string Title = "Kapitel 1: Der Traum beginnt";

        public readonly List<QuestStage> Stages = new List<QuestStage>
        {
            new QuestStage
            {
                Title = "Der erste Tag",
                Quests = new List<Quest>
                {
                    new Quest { Text = "Laden putzen", Target = 5, Key = "clean" },
                    new Quest { Text = "Erstes Regal aufstellen", Key = "shelfPlaced" },
                    new Quest { Text = "Monstera-Packs einräumen", Target = 12, Key = "stock" },
                    new Quest { Text = "Laden öffnen", Key = "open" },
                    new Quest { Text = "Kunden bedienen", Target = 5, Key = "served" },
                },
                Outro = new[]
                {
                    "{0}: Die ersten Kunden waren da! Aber ein Regal ist viel zu wenig für einen richtigen Laden.",
                    "Mehr Ware, mehr Kunden – und am Ende des Tages soll mehr in der Kasse sein als vorher.",
                },
            },
            new QuestStage
            {
                Title = "Ein richtiger Laden",
                Quests = new List<Quest>
                {
                    new Quest { Text = "Packs verkaufen", Target = 30, Key = "packsSold" },
                    new Quest { Text = "Regale im Laden", Target = 2, Poll = () => GameManager.Instance.Shelves.Count },
                    new Quest { Text = "Tage mit Gewinn abschließen", Target = 2, Key = "profitDay" },
                },
                Outro = new[]
                {
                    "Eine Nachricht von Papa: „Ein Laden ist mehr als Regale. Die Leute sollen sich bei dir wohlfühlen.“",
                    "{0}: Er hat recht. Ein bisschen Deko würde dem alten Laden guttun.",
                },
            },
            new QuestStage
            {
                Title = "Gemütlich soll es sein",
                Quests = new List<Quest>
                {
                    new Quest { Text = "Deko im Laden", Target = 3, Poll = () => GameManager.Instance.Decos.Count },
                    new Quest { Text = "Gemütlichkeit erreichen", Target = 5, Poll = () => GameManager.Instance.Coziness },
                    new Quest { Text = "Kunden bedienen", Target = 25, Key = "served" },
                },
                Outro = new[]
                {
                    "{0}: Es fühlt sich langsam an wie ein richtiger Laden. Wie Papas Laden damals.",
                    "Jetzt muss es sich nur noch rechnen. Mal sehen, was an einem richtig guten Tag möglich ist!",
                },
            },
            new QuestStage
            {
                Title = "Der Laden läuft",
                Quests = new List<Quest>
                {
                    new Quest { Text = "Packs verkaufen", Target = 80, Key = "packsSold" },
                    new Quest { Text = "An einem Tag Packs verkaufen", Target = 20, Key = "bestDay" },
                    new Quest { Text = "Weitere Tage mit Gewinn", Target = 3, Key = "profitDay" },
                    new Quest { Text = "Kunden bedienen", Target = 50, Key = "served" },
                },
            },
        };

        public int StageIndex { get; private set; }
        public QuestStage Stage => Stages[Mathf.Min(StageIndex, Stages.Count - 1)];
        /// <summary>Die Aufgaben der aktuellen Etappe.</summary>
        public List<Quest> Quests => Stage.Quests;
        public bool Completed { get; private set; }

        /// <summary>Kopfzeile der Aufgabenliste, z. B. „Kapitel 1 · 2/4 Ein richtiger Laden“.</summary>
        public string Header => $"Kapitel 1 · {Mathf.Min(StageIndex + 1, Stages.Count)}/{Stages.Count} {Stage.Title}";

        float pollTimer;

        void Start()
        {
            var game = GameManager.Instance;
            game.DirtCleaned += () => Advance("clean", 1);
            game.ShelfPlaced += () => Advance("shelfPlaced", 1);
            game.ShelfStocked += packs => Advance("stock", packs);
            game.ShopOpened += () => Advance("open", 1);
            game.CustomerServed += () => Advance("served", 1);
            game.PacksSold += packs => Advance("packsSold", packs);
            game.DecoPlaced += () => Advance("decoPlaced", 1);
            game.DayEnded += stats =>
            {
                if (stats.Income - stats.Expenses > 0) Advance("profitDay", 1);
                Record("bestDay", stats.PacksSold);
            };
        }

        void Update()
        {
            // Zustands-Aufgaben (z. B. Gemütlichkeit) einmal pro Sekunde prüfen.
            pollTimer -= Time.unscaledDeltaTime;
            if (pollTimer > 0f || Completed || GameManager.Instance.Phase == GamePhase.MainMenu) return;
            pollTimer = 1f;
            foreach (var quest in Quests)
                if (quest.Poll != null && !quest.Done) Set(quest, quest.Poll());
        }

        /// <summary>Fortschritt aller Etappen hintereinander, für den Spielstand.</summary>
        public int[] SaveProgress()
        {
            var progress = new List<int>();
            foreach (var stage in Stages)
                foreach (var quest in stage.Quests) progress.Add(quest.Progress);
            return progress.ToArray();
        }

        public void RestoreProgress(int[] progress)
        {
            if (progress == null) return;
            int i = 0;
            foreach (var stage in Stages)
                foreach (var quest in stage.Quests)
                {
                    if (i < progress.Length) quest.Progress = Mathf.Clamp(progress[i], 0, quest.Target);
                    i++;
                }

            StageIndex = Stages.FindIndex(stage => !stage.Quests.TrueForAll(q => q.Done));
            Completed = StageIndex < 0;
            if (Completed) StageIndex = Stages.Count - 1;
        }

        void Advance(string key, int amount)
        {
            if (Completed) return;
            foreach (var quest in Quests)
                if (quest.Key == key && !quest.Done) Set(quest, quest.Progress + amount);
        }

        /// <summary>Für „Bestwert“-Aufgaben: nur höhere Werte zählen.</summary>
        void Record(string key, int value)
        {
            if (Completed) return;
            foreach (var quest in Quests)
                if (quest.Key == key && !quest.Done && value > quest.Progress) Set(quest, value);
        }

        void Set(Quest quest, int value)
        {
            quest.Progress = Mathf.Clamp(value, 0, quest.Target);
            if (!quest.Done) return;

            var game = GameManager.Instance;
            game.ShowToast($"Aufgabe erledigt: {quest.Text} (+20 XP)");
            game.AddXp(20);

            if (!Quests.TrueForAll(q => q.Done))
            {
                SoundManager.Play(Sfx.Quest);
                return;
            }

            if (StageIndex < Stages.Count - 1)
            {
                // Nächste Etappe, mit einem kleinen Story-Moment dazwischen.
                var outro = Stage.Outro;
                StageIndex++;
                SoundManager.Play(Sfx.Quest);
                game.ShowToast($"Neue Aufgaben: {Stage.Title}", 5f);
                if (outro != null)
                    game.ShowDialog(Array.ConvertAll(outro, line => string.Format(line, game.PlayerName)));
                return;
            }

            Completed = true;
            SoundManager.Play(Sfx.Fanfare);
            game.ChangeReputation(10);
            game.ShowDialog(new[]
            {
                $"{game.PlayerName}: Geschafft! TwoMoons hat seine ersten Stammkunden und schreibt schwarze Zahlen.",
                "Kapitel 1 abgeschlossen!",
            }, () => game.PlayStoryOnce("kapitel1Ende", Cutscenes.Chapter1End));
        }
    }
}
