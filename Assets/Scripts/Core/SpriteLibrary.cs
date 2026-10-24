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
        public Sprite wall;
        public Sprite street;
        public Sprite door;
        public Sprite sign;
        public Sprite counter;
        public Sprite register;
        public Sprite computer;
        public Sprite shelf;
        public Sprite box;
        public Sprite dirt;
        public Sprite packTable;
        public Sprite showcase;

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
        public Sprite cardIcon;

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

        public static readonly string[] Keys =
        {
            "floor", "wall", "street", "door", "sign", "counter", "register", "computer", "shelf", "box", "dirt",
            "packTable", "showcase",
            "plant", "poster", "rug", "wallClock", "fairyLights",
            "packMonstera", "cardIcon",
            "moodHappy", "moodNeutral", "moodAngry",
            "player1", "player2", "player3", "customer1", "customer2", "customer3",
        };
    }
}
