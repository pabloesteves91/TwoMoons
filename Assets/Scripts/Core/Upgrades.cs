using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Dauerhafte Laden-Upgrades (Computer → Einrichtung). Nur anhängen, die Nummern stehen im Spielstand.</summary>
    public enum Upgrade { CardReader, BigShelves, AirCon, Camera, ShopWindow }

    public class UpgradeDef
    {
        public Upgrade Id;
        public string Name;
        public string Description;
        public int Cost;
        public int MinLevel;
    }

    /// <summary>Teure Verbesserungen für später im Spiel – jede einmal kaufbar.</summary>
    public class Upgrades
    {
        public static readonly UpgradeDef[] All =
        {
            new UpgradeDef { Id = Upgrade.CardReader, Name = "Kartenterminal", Cost = 150000, MinLevel = 4,
                Description = "Kartenzahlung: Kunden warten 15 % geduldiger, dein Mitarbeiter kassiert doppelt so schnell" },
            new UpgradeDef { Id = Upgrade.Camera, Name = "Überwachungskamera", Cost = 200000, MinLevel = 4,
                Description = "Halb so viele Ladendiebe, Überfälle nur noch halb so oft" },
            new UpgradeDef { Id = Upgrade.BigShelves, Name = "Größere Regale", Cost = 250000, MinLevel = 5,
                Description = "Alle Regale fassen 32 statt 24 Stück" },
            new UpgradeDef { Id = Upgrade.AirCon, Name = "Klimaanlage", Cost = 300000, MinLevel = 5,
                Description = "Angenehmes Klima: Kunden 25 % geduldiger, Gemütlichkeit +5" },
            new UpgradeDef { Id = Upgrade.ShopWindow, Name = "Großes Schaufenster", Cost = 500000, MinLevel = 6,
                Description = "Lockt Laufkundschaft an: den ganzen Tag 20 % mehr Kunden" },
        };

        readonly HashSet<Upgrade> owned = new HashSet<Upgrade>();

        public bool Has(Upgrade upgrade) => owned.Contains(upgrade);

        public void Add(Upgrade upgrade) => owned.Add(upgrade);

        public void Save(SaveData data)
        {
            data.upgrades.Clear();
            foreach (var upgrade in owned) data.upgrades.Add((int)upgrade);
        }

        public void Load(SaveData data)
        {
            owned.Clear();
            if (data.upgrades != null) foreach (int id in data.upgrades) owned.Add((Upgrade)id);
        }
    }
}
