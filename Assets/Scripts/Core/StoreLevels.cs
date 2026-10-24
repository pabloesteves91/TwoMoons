namespace TwoMoons
{
    /// <summary>
    /// Store-Level 1–7. Aufstieg, wenn die XP reichen und die Kapitel-Aufgaben des aktuellen Levels erledigt sind.
    /// Siehe Docs/Roadmap.md.
    /// </summary>
    public static class StoreLevels
    {
        public const int Max = 7;

        static readonly int[] XpNeeded = { 0, 0, 150, 500, 1200, 2500, 4500, 7500 };

        static readonly string[] Titles =
        {
            "", "Leerer Laden", "Kleiner Kartenladen", "Treffpunkt", "Sammlerparadies",
            "Spielerheimat", "Stadtbekannt", "Legendär",
        };

        /// <summary>Was ein Level freischaltet – für den Level-Up-Bildschirm.</summary>
        static readonly string[][] Unlocks =
        {
            new string[0],
            new[] { "Regal, Kasse, Computer, Pack-Tisch", "Monstera-Booster", "Deko: Topfpflanze, Poster" },
            new[] { "Vitrine für Einzelkarten", "Deko: Teppich, Wanduhr, Lichterkette", "Bald: Spellforge und Zubehör" },
            new[] { "Bald: Turniere mit Spieltischen und Prizepool", "Bald: Soul Clash" },
            new[] { "Bald: 3D-Drucker und Ankauf von Karten" },
            new[] { "Bald: Couch-&-TV-Ecke, Getränke, Ladenerweiterung" },
            new[] { "Bald: Großturniere, Premium-Vitrine, Arcade-Automat" },
            new[] { "Bald: Stadtturnier und Trophäenwand" },
        };

        /// <summary>XP, die insgesamt für dieses Level nötig sind.</summary>
        public static int XpFor(int level) => XpNeeded[UnityEngine.Mathf.Clamp(level, 1, Max)];

        public static string Title(int level) => Titles[UnityEngine.Mathf.Clamp(level, 1, Max)];

        public static string[] UnlocksFor(int level) => Unlocks[UnityEngine.Mathf.Clamp(level, 1, Max)];
    }
}
