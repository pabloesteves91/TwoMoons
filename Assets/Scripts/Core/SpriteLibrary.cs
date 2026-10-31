using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Alle Bilder des Spiels. Jedes Feld gehört zu einer PNG-Datei mit gleichem Namen in Assets/Art/Sprites.
    /// Eigene Pixelbilder (16×16) einfach mit demselben Dateinamen ersetzen. Leere Felder bekommen einen Platzhalter.
    /// </summary>
    [CreateAssetMenu(menuName = "TwoMoons/Sprite Library", fileName = "SpriteLibrary")]
    public class SpriteLibrary : ScriptableObject
    {
        [Header("Laden")]
        public Sprite floor;
        [Tooltip("Zusätzliche Boden-Variante (derzeit nicht verwendet)")]
        public Sprite floor2;
        public Sprite wall;
        public Sprite street;
        public Sprite door;
        public Sprite sign;
        public Sprite signBoard;
        public Sprite counter;
        public Sprite register;
        public Sprite computer;
        public Sprite shelf;
        public Sprite box;
        public Sprite package;
        public Sprite dirt;
        public Sprite packTable;
        public Sprite showcase;
        public Sprite accessoryStand;
        public Sprite gameTable;
        public Sprite prizeCounter;
        public Sprite tournamentBoard;
        public Sprite fridge;
        public Sprite couch;
        public Sprite tv;
        public Sprite console;
        public Sprite printer3d;
        public Sprite printShelf;
        public Sprite neonSign;
        public Sprite gradingStation;
        public Sprite beanBag;
        public Sprite premiumShowcase;
        public Sprite arcade;
        public Sprite turf;
        public Sprite trophyWall;
        public Sprite moonStatue;
        public Sprite trophy;
        public Sprite table;
        public Sprite chair;
        public Sprite trashBin;
        public Sprite trashBinFull;
        public Sprite container;
        public Sprite storageDoor;

        [Header("Deko")]
        public Sprite plant;
        public Sprite poster;
        public Sprite rug;
        public Sprite wallClock;
        public Sprite fairyLights;

        [Header("Oberfläche")]
        [Tooltip("Logo im Hauptmenü, beliebige Größe. Leer = Assets/Art/UI/Resources/logo.png")]
        public Sprite logo;

        [Header("Ware")]
        public Sprite packMonstera;
        public Sprite packSpellforge;
        public Sprite packSoulClash;
        public Sprite soda;
        public Sprite energyDrink;
        public Sprite filament;
        public Sprite printSmall;
        public Sprite printMedium;
        public Sprite printLarge;
        public Sprite sleeves;
        public Sprite binder;
        public Sprite playmat;
        public Sprite cardIcon;
        public Sprite slabIcon;
        public Sprite trashBag;
        public Sprite boardGame1;
        public Sprite boardGame2;
        public Sprite boardGame3;
        public Sprite wrappers;

        [Header("Kunden-Laune")]
        public Sprite moodHappy;
        public Sprite moodNeutral;
        public Sprite moodAngry;

        [Header("Figuren")]
        public Sprite player1;
        public Sprite player2;
        public Sprite player3;
        public Sprite customer1;
        public Sprite customer2;
        public Sprite customer3;
        public Sprite employee;
        [Tooltip("Papa im Laden (Kapitel 6 und Finale)")]
        public Sprite papa;

        public static readonly string[] Keys =
        {
            "floor", "wall", "street", "door", "sign", "signBoard", "counter", "register", "computer", "shelf", "box", "package", "dirt",
            "packTable", "showcase",
            "plant", "poster", "rug", "wallClock", "fairyLights",
            "packMonstera", "packSpellforge", "sleeves", "binder", "playmat", "accessoryStand", "gameTable", "prizeCounter", "tournamentBoard", "fridge", "couch", "tv", "console", "packSoulClash", "soda", "energyDrink", "employee", "printer3d", "printShelf", "neonSign", "filament", "printSmall", "printMedium", "printLarge", "cardIcon", "gradingStation", "beanBag", "slabIcon", "floor2", "premiumShowcase", "arcade", "turf", "trophyWall", "moonStatue", "trophy", "papa", "table", "chair", "trashBin", "trashBinFull", "trashBag", "container", "wrappers", "storageDoor", "boardGame1", "boardGame2", "boardGame3",
            "moodHappy", "moodNeutral", "moodAngry",
            "player1", "player2", "player3", "customer1", "customer2", "customer3",
        };
    }
}
