using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Aufgaben des aktuellen Kapitels (Kapitel = Store-Level). Es ist immer nur eine Etappe aktiv;
    /// erledigte Aufgaben verschwinden aus der Liste.
    /// </summary>
    public class Chapters : MonoBehaviour
    {
        public int Chapter { get; private set; } = 1;
        public List<QuestStage> Stages { get; private set; } = ChapterData.Create(1);
        public int StageIndex { get; private set; }
        public bool Completed { get; private set; }

        public QuestStage Stage => Stages[Mathf.Clamp(StageIndex, 0, Stages.Count - 1)];
        /// <summary>Alle Aufgaben der aktuellen Etappe.</summary>
        public List<Quest> Quests => Stage.Quests;
        /// <summary>Nur die noch offenen Aufgaben der aktuellen Etappe.</summary>
        public List<Quest> OpenQuests => Completed ? new List<Quest>() : Quests.FindAll(q => !q.Done);

        public string Header =>
            $"Kapitel {Chapter} · {Mathf.Min(StageIndex + 1, Stages.Count)}/{Stages.Count} {Stage.Title}";

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
            game.ProductSold += (product, count) =>
            {
                Advance($"sold:{product.Id}", count);
                if (product.Category == ProductCategory.Accessory) Advance("accessorySold", count);
                Advance($"category:{product.Category}", count);
            };
            game.DecoPlaced += () => Advance("decoPlaced", 1);
            game.PackOpened += ultras =>
            {
                Advance("packOpened", 1);
                if (ultras > 0) Advance("ultraPulled", ultras);
            };
            game.CardSold += grade =>
            {
                Advance("cardSold", 1);
                if (grade > 0) Advance("slabSold", 1);
            };
            game.Graded += grade =>
            {
                Advance("graded", 1);
                if (grade >= 9) Advance("topGrade", 1);
            };
            game.ShopExpanded += () => Advance("expanded", 1);
            game.CityTournamentHeld += () => Advance(TournamentManager.CityTournamentKey, 1);
            game.TournamentHeld += () => Advance("tournament", 1);
            game.EmployeeHired += () => Advance("hired", 1);
            game.Printed += () => Advance("printed", 1);
            game.CardsBought += () => Advance("cardsBought", 1);
            game.DayEnded += stats =>
            {
                if (stats.Income - stats.Expenses > 0) Advance("profitDay", 1);
                Record("bestDay", stats.PacksSold);
            };
        }

        /// <summary>Neues Kapitel beginnen (nach dem Level-Aufstieg).</summary>
        public void StartChapter(int chapter)
        {
            Chapter = chapter;
            Stages = ChapterData.Create(chapter);
            StageIndex = 0;
            Completed = Stages.Count == 0;
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

        /// <summary>Fortschritt aller Etappen des Kapitels hintereinander, für den Spielstand.</summary>
        public int[] SaveProgress()
        {
            var progress = new List<int>();
            foreach (var stage in Stages)
                foreach (var quest in stage.Quests) progress.Add(quest.Progress);
            return progress.ToArray();
        }

        public void Restore(int chapter, int[] progress)
        {
            StartChapter(chapter);
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
                var finished = Stage;
                StageIndex++;
                SoundManager.Play(Sfx.Quest);
                game.ShowToast($"Neue Aufgaben: {Stage.Title}", 5f);
                if (finished.Outro != null)
                    game.ShowDialog(Array.ConvertAll(finished.Outro, line => string.Format(line, game.PlayerName)), finished.Then);
                else
                    finished.Then?.Invoke();
                return;
            }

            Completed = true;
            SoundManager.Play(Sfx.Fanfare);
            game.ChangeReputation(10);
            game.ChapterFinished(Chapter);
        }
    }
}
