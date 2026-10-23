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
        public int shelvesToPlace;
        public int chapter = 1;
        public int[] questProgress;
        public List<SavedPrice> prices = new List<SavedPrice>();
        public List<SavedShelf> shelves = new List<SavedShelf>();
        public List<SavedBox> boxes = new List<SavedBox>();
        public List<SavedCell> dirt = new List<SavedCell>();
        public long savedAtTicks;

        public DateTime SavedAt => new DateTime(savedAtTicks);
    }

    /// <summary>Drei Speicherplätze im Benutzerordner (Application.persistentDataPath).</summary>
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        static string PathFor(int slot) => Path.Combine(Application.persistentDataPath, $"spielstand_{slot + 1}.json");

        public static bool Exists(int slot) => File.Exists(PathFor(slot));

        public static SaveData Load(int slot)
        {
            if (!Exists(slot)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)));
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
            File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
        }

        public static void Delete(int slot)
        {
            if (Exists(slot)) File.Delete(PathFor(slot));
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
