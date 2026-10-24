using UnityEngine;

namespace TwoMoons
{
    /// <summary>Hilfe im Esc-Menü: Steuerung und Erklärungen zu allem im Laden, nach Themen.</summary>
    public static class HelpUI
    {
        static readonly string[] Topics = { "Steuerung", "Laden", "Computer", "Karten", "Extras", "Möbel", "Level" };

        static int topic;
        static Vector2 scroll;

        static GameManager Game => GameManager.Instance;

        /// <summary>Zeichnet das Hilfe-Fenster. Gibt true zurück, wenn „Zurück“ gedrückt wurde.</summary>
        public static bool Draw(float width)
        {
            UiKit.BeginWindow(UiKit.Centered(width, 720f, Mathf.Min(UiKit.VirtualHeight - 40f, 480f)));
            GUILayout.Label("Hilfe", UiKit.Title);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < Topics.Length; i++)
            {
                GUI.color = topic == i ? Color.white : UiKit.Dimmed;
                if (UiKit.Button(Topics[i], GUILayout.Height(28f)))
                {
                    topic = i;
                    scroll = Vector2.zero;
                }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);

            scroll = GUILayout.BeginScrollView(scroll);
            switch (topic)
            {
                case 0: Controls(); break;
                case 1: Shop(); break;
                case 2: Computer(); break;
                case 3: Cards(); break;
                case 4: Extras(); break;
                case 5: Furniture(); break;
                default: Levels(); break;
            }
            GUILayout.EndScrollView();

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            bool back = UiKit.Button("Zurück", GUILayout.Height(32f));
            if (Game.Guide != null && UiKit.Button("Tutorial nochmal starten", GUILayout.Height(32f)))
            {
                Game.PauseOpen = false;
                Game.Guide.Restart();
                back = true;
            }
            GUILayout.EndHorizontal();
            UiKit.EndWindow();
            return back;
        }

        static void Section(string title, params string[] lines)
        {
            GUILayout.Label(title, UiKit.Bold);
            foreach (var line in lines) GUILayout.Label("•  " + line, UiKit.Text);
            GUILayout.Space(8f);
        }

        static void Controls() =>
            Section("Tastatur & Maus",
                "[W] [A] [S] [D] oder Pfeiltasten – laufen",
                "[E] – benutzen: putzen, Karton aufheben/einräumen, kassieren, Computer, Vitrine, Pack-Tisch …",
                "[Q] – getragenen Karton abstellen",
                "[B] – Bau-Modus: Möbel aufstellen (Linksklick), nochmal [B] wechselt das Möbelstück. Morgens: Möbel anklicken zum Verschieben",
                "Rechtsklick – im Bau-Modus abbrechen",
                "[C] – Sammelbuch",
                "[Tab] – Aufgabenliste auf- und zuklappen",
                "[Esc] – Pause-Menü (Speichern, Einstellungen, Hilfe) bzw. Fenster schließen",
                "[F11] – Vollbild ein/aus",
                "Klick oder [Leertaste] – Dialoge und Geschichten weiterklicken");

        static void Shop()
        {
            Section("Ein Tag im Laden",
                "Morgens ist der Laden zu: putzen, einräumen, bestellen, Möbel stellen.",
                "Am Schild neben der Tür öffnest du den Laden. Ein Tag dauert von 9 bis 18 Uhr (Level 1–2 etwa 5 Minuten, ab Level 3 etwa 7 Minuten).",
                "Abends gibt es die Abrechnung, danach schläfst du – am Morgen wird automatisch gespeichert.");
            Section("Kunden",
                "Kunden nehmen Ware aus dem Regal und stellen sich an der Kasse an. Drück dort [E].",
                "Das Gesicht über dem Kopf zeigt die Laune: froh → neutral → verärgert. Danach gehen sie (Ruf −1).",
                "Ist ein Regal leer, schauen sie im nächsten. Ist alles leer, gehen sie.",
                "Manchmal kleckert ein Kunde – Flecken mit [E] wegwischen, sonst werden alle ungeduldiger.",
                "Ladendiebe: Wer „*pfeif*“ macht und ohne zu zahlen zur Tür schleicht, hat etwas eingesteckt. Hinlaufen und [E] drücken!");
            Section("Tagesereignisse (ab Tag 2)",
                "Regentag – weniger Kunden, aber geduldiger.",
                "Schulschluss-Ansturm – 13 bis 15 Uhr kommen viel mehr Kunden.",
                "Influencer-Besuch – gut bedienen bringt viel Ruf, sonst gibt es Ärger.",
                "Großhändler-Aktion – alle Ware 20 % günstiger.",
                "Sammlertag – Kunden kaufen ein Pack mehr, mehr Sammler an der Vitrine.");
            Section("Preise",
                "Bis zum Marktwert kaufen alle. Darüber geht ein Teil der Kunden: +30 % etwa jeder Zweite, ab +60 % keiner.",
                "Unter 85 % ist es ein Schnäppchen: der Kunde nimmt ein Pack mehr und der Ruf steigt schneller.");
            Section("Ruf & Gemütlichkeit",
                "Zufriedene Kunden bringen Ruf. Mit besserem Ruf kommen mehr Kunden und sie warten länger.",
                "Deko macht den Laden gemütlich: Kunden sind geduldiger und abends gibt es etwas Ruf dazu.");
        }

        static void Computer() =>
            Section("Der Computer (oben links, [E])",
                "Ware – Kartons mit Boostern, Zubehör, Getränken, Filament. Sofort am Eingang geliefert.",
                "Einrichtung – Möbel und Deko. Alles kommt in einem Paket am Eingang: mit [E] auspacken, dann aufstellen. Ab Level 5 auch die Ladenerweiterung.",
                "Preise – Verkaufspreis jeder Ware einstellen.",
                "Team – ab Level 3 einen Mitarbeiter einstellen (Kasse oder Regale), Lohn wird abends abgezogen.",
                "Level – was jedes Store-Level freischaltet.",
                "Talente – Fähigkeitspunkte einlösen (einer pro Level-Aufstieg).");

        static void Cards()
        {
            Section("Packs öffnen & Sammelbuch",
                "Am Pack-Tisch hinter der Theke öffnest du Packs aus dem Regal oder aus einem getragenen Karton.",
                "Jedes Pack hat 10 Karten, die beste zuletzt. Ultra-Seltene glänzen.",
                "Im Sammelbuch [C] siehst du alle Karten pro Spiel (Monstera, Spellforge, Soul Clash).");
            Section("Vitrine",
                "Lege Einzelkarten in die Vitrine und stelle den Preis ein. Sammler kaufen daraus.",
                "Die Premium-Vitrine (Level 6) hat 8 Fächer, dort zahlen Sammler bis zu 10 % mehr.");
            Section("Ankauf (ab Level 4)",
                "Manche Kunden wollen dir Karten verkaufen. Kaufen, Gegenangebot (−15 %) oder ablehnen.");
            Section("Grading (ab Level 5)",
                "An der Grading-Station eine Karte bewerten lassen (5 €, 1 Spielstunde).",
                "Note 10 ×5, 9 ×3, 8 ×2, 7 ×1,5 Wert. Bewertete Karten mögen Sammler besonders.");
        }

        static void Extras()
        {
            Section("Turniere (ab Level 3)",
                "Brauchen Spieltisch und Turnier-Counter. Jeden 2. Tag, 15–18 Uhr.",
                "Morgens schickt der Hersteller Turnier-Packs – zum Turnier-Counter tragen, dort den Prizepool verteilen.",
                "Ab 13 Uhr melden sich Spieler an der Kasse an (5 € Startgeld). Um 18 Uhr Siegerehrung am Counter mit [E].",
                "Ab Level 6 werden es Großturniere, in Kapitel 7 kommt das Stadtturnier.");
            Section("Spielecke",
                "Couch + Fernseher + Konsole: Kunden zocken nach dem Kauf und holen sich danach gern ein Getränk.",
                "Arcade-Automat (Level 6): Kunden spielen für 1 € pro Runde.");
            Section("3D-Drucker (ab Level 4)",
                "Filament-Spulen hintragen, Modell wählen, fertige Figur abholen und ins Druck-Regal stellen.",
                "Verheddert sich das Filament, mit [E] am Drucker beheben.");
        }

        static void Furniture()
        {
            GUILayout.Label("Alle Möbel und Deko (kaufen am Computer → Einrichtung):", UiKit.Hint);
            foreach (var option in Catalog.Furnishings)
            {
                bool locked = option.MinLevel > Game.StoreLevel;
                GUI.color = locked ? UiKit.Dimmed : Color.white;
                var def = FurnitureCatalog.Get(option.Furniture);
                string where = def.Placement == Placement.Wall ? " · Wand" : def.Placement == Placement.Rug ? " · Boden" : "";
                GUILayout.Label($"{option.Name} – {Money.Format(option.Cost)} · ab Level {option.MinLevel}{where}", UiKit.Bold);
                GUILayout.Label(option.Description, UiKit.Text);
                GUILayout.Space(4f);
            }
            GUI.color = Color.white;
        }

        static void Levels()
        {
            Section("Store-Level & Kapitel",
                "Jedes Level ist ein Kapitel mit eigenen Aufgaben (oben rechts).",
                "Aufsteigen kannst du, wenn die Aufgaben erledigt sind und genug XP da sind.",
                "XP gibt es für Verkäufe, zufriedene Kunden, Aufgaben und Turniere.");
            Section("Fähigkeiten",
                "Mit jedem Level-Aufstieg gibt es einen Punkt. Einlösen am Computer im Tab „Talente“.");
            foreach (var skill in Skills.All)
            {
                GUI.color = Game.Skills.Has(skill.Id) ? UiKit.Good : Color.white;
                GUILayout.Label($"{skill.Name}: {skill.Description}", UiKit.Text);
            }
            GUI.color = Color.white;
        }
    }
}
