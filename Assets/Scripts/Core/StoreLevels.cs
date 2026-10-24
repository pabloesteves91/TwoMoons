namespace TwoMoons
{
    /// <summary>
    /// Store-Level 1–7. Aufstieg, wenn die XP reichen und die Kapitel-Aufgaben des aktuellen Levels erledigt sind.
    /// Siehe Docs/Roadmap.md.
    /// </summary>
    public static class StoreLevels
    {
        public const int Max = 7;

        /// <summary>Grob auf die Kapitel-Länge abgestimmt: Kapitel 1–3 je ca. 1 Std., 4–7 je ca. 90 Min.</summary>
        static readonly int[] XpNeeded = { 0, 0, 1200, 3500, 7000, 12000, 18000, 26000 };

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
            new[] { "Spellforge-Booster und Spellforge-Karten", "Zubehör-Ständer: Hüllen, Ordner, Spielmatten", "Vitrine für Einzelkarten", "Deko: Teppich, Wanduhr, Lichterkette" },
            new[] { "Turniere jeden 2. Tag: Spieltisch, Turnier-Counter, Turnier-Tafel", "Gratis Turnier-Packs vom Hersteller", "Soul-Clash-Booster und -Karten", "Couch-&-TV-Ecke mit Konsole, Getränkekühlschrank", "Dein erster Mitarbeiter (Computer → Team)" },
            new[] { "Bald: 3D-Drucker und Ankauf von Karten" },
            new[] { "Bald: Karten-Grading", "Bald: Ladenerweiterung (zweiter Raum)" },
            new[] { "Bald: Großturniere, Premium-Vitrine, Arcade-Automat" },
            new[] { "Bald: Stadtturnier und Trophäenwand" },
        };

        /// <summary>XP, die insgesamt für dieses Level nötig sind.</summary>
        public static int XpFor(int level) => XpNeeded[UnityEngine.Mathf.Clamp(level, 1, Max)];

        public static string Title(int level) => Titles[UnityEngine.Mathf.Clamp(level, 1, Max)];

        public static string[] UnlocksFor(int level) => Unlocks[UnityEngine.Mathf.Clamp(level, 1, Max)];
    }
}
