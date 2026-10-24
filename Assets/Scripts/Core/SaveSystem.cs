using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TwoMoons
{
    [Serializable]
    public class SavedShelf
    {
        public int x, y;
        public int stock;
        public int product = -1;
        /// <summary>0 = Regal für Packs, 1 = Zubehör-Ständer.</summary>
        public int category;
    }

    [Serializable]
    public class SavedBox
    {
        public int x, y;
        public int product;
        public int amount;
    }

    [Serializable]
    public class SavedCell
    {
        public int x, y;
    }

    [Serializable]
    public class SavedCard
    {
        public int card;
        public int count;
    }

    [Serializable]
    public class SavedShowcase
    {
        public int x, y;
        public int[] cards;
        public int[] prices;
    }

    [Serializable]
    public class SavedCount
    {
        public int type;
        public int count;
    }

    [Serializable]
    public class SavedPackage
    {
        public int x, y;
        public List<SavedCount> contents = new List<SavedCount>();
    }

    [Serializable]
    public class SavedTournament
    {
        public int nextDay;
        public int[] prizes = new int[4];
        public int bonusCard = -1;
        public int held;
    }

    [Serializable]
    public class SavedDeco
    {
        public int type;
        public int x, y;
    }

    [Serializable]
    public class SavedPrice
    {
        public int product;
        public int cents;
    }

    /// <summary>Alles, was in einem Speicherplatz steht. Wird als JSON gespeichert.</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string playerName;
        public int figure;
        public int day;
        public int money;
        public int reputation;
        public int storeLevel = 1;
        public int xp;
        /// <summary>Nur noch zum Laden älterer Spielstände, neu: <see cref="toPlace"/>.</summary>
        public int shelvesToPlace;
        public int showcasesToPlace;
        /// <summary>Tagesphase (GamePhase) und Sekunden seit Ladenöffnung.</summary>
        public int phase;
        public float dayTime;
        public DayStats today;
        public int chapter = 1;
        public int[] questProgress;
        public List<SavedPrice> prices = new List<SavedPrice>();
        public List<SavedShelf> shelves = new List<SavedShelf>();
        public List<SavedBox> boxes = new List<SavedBox>();
        public List<SavedCell> dirt = new List<SavedCell>();
        public List<SavedCard> cards = new List<SavedCard>();
        public List<SavedShowcase> showcases = new List<SavedShowcase>();
        public List<SavedCount> toPlace = new List<SavedCount>();
        public List<SavedDeco> decos = new List<SavedDeco>();
        public List<string> seenStories = new List<string>();
        public SavedPackage package;
        public SavedTournament tournament;
        public long savedAtTicks;

        public DateTime SavedAt => new DateTime(savedAtTicks);
    }

    /// <summary>
    /// Drei Speicherplätze. Auf dem Computer als Datei im Benutzerordner (Application.persistentDataPath),
    /// im Browser über PlayerPrefs (dort sicher dauerhaft gespeichert).
    /// </summary>
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        static bool InBrowser => Application.platform == RuntimePlatform.WebGLPlayer;

        static string PathFor(int slot) => Path.Combine(Application.persistentDataPath, $"spielstand_{slot + 1}.json");

        static string KeyFor(int slot) => $"TwoMoons.Spielstand{slot + 1}";

        public static bool Exists(int slot) => InBrowser ? PlayerPrefs.HasKey(KeyFor(slot)) : File.Exists(PathFor(slot));

        public static SaveData Load(int slot)
        {
            if (!Exists(slot)) return null;
            try
            {
                string json = InBrowser ? PlayerPrefs.GetString(KeyFor(slot)) : File.ReadAllText(PathFor(slot));
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Spielstand {slot + 1} konnte nicht gelesen werden: {e.Message}");
                return null;
            }
        }

        public static void Save(int slot, SaveData data)
        {
            data.savedAtTicks = DateTime.Now.Ticks;
            string json = JsonUtility.ToJson(data, true);
            if (InBrowser)
            {
                PlayerPrefs.SetString(KeyFor(slot), json);
                PlayerPrefs.Save();
            }
            else
            {
                File.WriteAllText(PathFor(slot), json);
            }
        }

        public static void Delete(int slot)
        {
            if (InBrowser)
            {
                PlayerPrefs.DeleteKey(KeyFor(slot));
                PlayerPrefs.Save();
            }
            else if (Exists(slot))
            {
                File.Delete(PathFor(slot));
            }
        }

        /// <summary>Der zuletzt gespeicherte Platz, oder -1 wenn es keinen gibt.</summary>
        public static int LatestSlot()
        {
            int latest = -1;
            long latestTicks = 0;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                var data = Load(slot);
                if (data == null || data.savedAtTicks <= latestTicks) continue;
                latest = slot;
                latestTicks = data.savedAtTicks;
            }
            return latest;
        }
    }
}
