using UnityEngine;

namespace TwoMoons
{
    /// <summary>Zeichnet einfache 16×16-Platzhalter, bis eigene Pixelbilder da sind.</summary>
    public static class PlaceholderSprites
    {
        const int Size = 16;

        public static Sprite CreateSprite(string key)
        {
            var texture = CreateTexture(key);
            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Art.PixelsPerUnit);
            sprite.name = key;
            return sprite;
        }

        public static Texture2D CreateTexture(string key)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = key,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color32[Size * Size];

            void Rect(int x, int y, int w, int h, Color32 c)
            {
                for (int yy = Mathf.Max(0, y); yy < Mathf.Min(Size, y + h); yy++)
                for (int xx = Mathf.Max(0, x); xx < Mathf.Min(Size, x + w); xx++)
                    px[yy * Size + xx] = c;
            }

            void Frame(int x, int y, int w, int h, Color32 c)
            {
                Rect(x, y, w, 1, c);
                Rect(x, y + h - 1, w, 1, c);
                Rect(x, y, 1, h, c);
                Rect(x + w - 1, y, 1, h, c);
            }

            void Circle(int cx, int cy, int r, Color32 c)
            {
                for (int yy = -r; yy <= r; yy++)
                for (int xx = -r; xx <= r; xx++)
                    if (xx * xx + yy * yy <= r * r + r) Rect(cx + xx, cy + yy, 1, 1, c);
            }

            void Speckle(Color32 c, int count, int seed)
            {
                var random = new System.Random(seed);
                for (int i = 0; i < count; i++) Rect(random.Next(Size), random.Next(Size), 1, 1, c);
            }

            void Face(Color32 skin, bool smile, bool angry)
            {
                var outline = new Color32(40, 25, 20, 255);
                Rect(4, 2, 8, 12, outline);
                Rect(2, 4, 12, 8, outline);
                Rect(5, 3, 6, 10, skin);
                Rect(3, 5, 10, 6, skin);
                Rect(5, 8, 2, 2, outline);
                Rect(9, 8, 2, 2, outline);
                if (angry)
                {
                    Rect(4, 10, 3, 1, outline);
                    Rect(9, 10, 3, 1, outline);
                }
                if (smile)
                {
                    Rect(5, 5, 6, 1, outline);
                    Rect(4, 6, 1, 1, outline);
                    Rect(11, 6, 1, 1, outline);
                }
                else if (angry)
                {
                    Rect(5, 5, 6, 1, outline);
                    Rect(4, 4, 1, 1, outline);
                    Rect(11, 4, 1, 1, outline);
                }
                else
                {
                    Rect(5, 5, 6, 1, outline);
                }
            }

            void Person(Color32 shirt, Color32 hair)
            {
                var skin = new Color32(240, 200, 160, 255);
                var pants = new Color32(50, 50, 70, 255);
                var outline = new Color32(30, 25, 35, 255);
                Rect(5, 0, 2, 3, pants);
                Rect(9, 0, 2, 3, pants);
                Rect(4, 3, 8, 6, shirt);
                Rect(3, 4, 1, 4, skin);
                Rect(12, 4, 1, 4, skin);
                Rect(5, 9, 6, 5, skin);
                Rect(5, 13, 6, 2, hair);
                Rect(4, 11, 1, 4, hair);
                Rect(11, 11, 1, 4, hair);
                Rect(6, 11, 1, 1, outline);
                Rect(9, 11, 1, 1, outline);
            }

            switch (key)
            {
                case "floor":
                    Rect(0, 0, 16, 16, new Color32(181, 136, 94, 255));
                    for (int y = 3; y < 16; y += 4) Rect(0, y, 16, 1, new Color32(150, 108, 72, 255));
                    break;
                case "wall":
                    Rect(0, 0, 16, 16, new Color32(88, 70, 110, 255));
                    Rect(0, 12, 16, 4, new Color32(120, 98, 145, 255));
                    Rect(0, 6, 16, 1, new Color32(70, 55, 90, 255));
                    Rect(7, 0, 1, 6, new Color32(70, 55, 90, 255));
                    break;
                case "street":
                    Rect(0, 0, 16, 16, new Color32(125, 125, 135, 255));
                    Rect(3, 4, 1, 1, new Color32(150, 150, 160, 255));
                    Rect(11, 9, 1, 1, new Color32(150, 150, 160, 255));
                    Rect(0, 15, 16, 1, new Color32(100, 100, 110, 255));
                    break;
                case "door":
                    Rect(0, 0, 16, 16, new Color32(181, 136, 94, 255));
                    Rect(2, 2, 12, 12, new Color32(120, 40, 50, 255));
                    Frame(2, 2, 12, 12, new Color32(200, 170, 80, 255));
                    break;
                case "sign":
                    Rect(0, 0, 16, 16, new Color32(88, 70, 110, 255));
                    Rect(2, 4, 12, 8, new Color32(245, 225, 130, 255));
                    Frame(2, 4, 12, 8, new Color32(110, 80, 40, 255));
                    Rect(5, 7, 2, 2, new Color32(90, 90, 200, 255));
                    Rect(9, 7, 2, 2, new Color32(90, 90, 200, 255));
                    break;
                case "counter":
                    Rect(0, 0, 16, 16, new Color32(140, 90, 60, 255));
                    Rect(0, 11, 16, 5, new Color32(190, 140, 95, 255));
                    Rect(0, 10, 16, 1, new Color32(100, 62, 40, 255));
                    break;
                case "register":
                    Rect(0, 0, 16, 16, new Color32(140, 90, 60, 255));
                    Rect(0, 11, 16, 5, new Color32(190, 140, 95, 255));
                    Rect(3, 6, 10, 8, new Color32(60, 60, 72, 255));
                    Rect(5, 10, 6, 3, new Color32(120, 220, 140, 255));
                    break;
                case "computer":
                    Rect(0, 0, 16, 16, new Color32(140, 90, 60, 255));
                    Rect(0, 8, 16, 3, new Color32(190, 140, 95, 255));
                    Rect(3, 9, 10, 7, new Color32(40, 40, 50, 255));
                    Rect(4, 10, 8, 5, new Color32(90, 160, 240, 255));
                    break;
                case "shelf":
                    Rect(1, 0, 14, 16, new Color32(120, 75, 45, 255));
                    Rect(2, 1, 12, 14, new Color32(80, 50, 30, 255));
                    Rect(2, 5, 12, 1, new Color32(160, 110, 70, 255));
                    Rect(2, 10, 12, 1, new Color32(160, 110, 70, 255));
                    break;
                case "box":
                    Rect(2, 1, 12, 11, new Color32(196, 150, 90, 255));
                    Rect(2, 9, 12, 3, new Color32(215, 175, 115, 255));
                    Rect(7, 1, 2, 11, new Color32(230, 215, 170, 255));
                    Frame(2, 1, 12, 11, new Color32(140, 100, 55, 255));
                    break;
                case "signBoard":
                    Rect(0, 0, 16, 16, new Color32(70, 38, 26, 255));
                    Rect(1, 1, 14, 14, new Color32(45, 24, 18, 255));
                    break;
                case "package":
                    Rect(1, 1, 14, 11, new Color32(160, 115, 70, 255));
                    Rect(1, 9, 14, 3, new Color32(185, 135, 85, 255));
                    Rect(7, 1, 2, 11, new Color32(225, 85, 40, 255));
                    Rect(1, 6, 14, 2, new Color32(225, 85, 40, 255));
                    Rect(6, 12, 2, 2, new Color32(245, 200, 80, 255));
                    Rect(8, 12, 2, 2, new Color32(245, 200, 80, 255));
                    Frame(1, 1, 14, 11, new Color32(100, 70, 40, 255));
                    break;
                case "dirt":
                    Rect(3, 4, 6, 4, new Color32(90, 70, 40, 170));
                    Rect(7, 7, 5, 4, new Color32(90, 70, 40, 170));
                    Rect(10, 3, 3, 2, new Color32(90, 70, 40, 170));
                    break;
                case "packTable":
                    Rect(0, 0, 16, 16, new Color32(140, 90, 60, 255));
                    Rect(0, 8, 16, 5, new Color32(190, 140, 95, 255));
                    Rect(3, 9, 4, 6, new Color32(60, 170, 90, 255));
                    Rect(8, 10, 3, 4, new Color32(245, 235, 210, 255));
                    Rect(12, 10, 3, 4, new Color32(245, 235, 210, 255));
                    break;
                case "showcase":
                    Rect(1, 0, 14, 16, new Color32(110, 70, 45, 255));
                    Rect(2, 2, 12, 12, new Color32(170, 215, 230, 255));
                    Rect(2, 8, 12, 1, new Color32(110, 70, 45, 255));
                    Rect(3, 12, 2, 1, new Color32(255, 255, 255, 255));
                    break;
                case "plant":
                    Rect(5, 0, 6, 5, new Color32(190, 100, 60, 255));
                    Rect(4, 4, 8, 1, new Color32(160, 80, 50, 255));
                    Rect(7, 5, 2, 4, new Color32(60, 120, 50, 255));
                    Rect(3, 8, 5, 4, new Color32(80, 170, 80, 255));
                    Rect(8, 9, 5, 5, new Color32(70, 150, 70, 255));
                    Rect(6, 12, 4, 3, new Color32(100, 190, 90, 255));
                    break;
                case "poster":
                    Rect(3, 3, 10, 12, new Color32(30, 22, 18, 255));
                    Rect(4, 4, 8, 10, new Color32(245, 200, 80, 255));
                    Rect(5, 8, 6, 5, new Color32(225, 85, 40, 255));
                    Rect(6, 5, 4, 2, new Color32(250, 240, 220, 255));
                    break;
                case "rug":
                    Rect(1, 2, 14, 12, new Color32(150, 45, 40, 255));
                    Rect(2, 3, 12, 10, new Color32(190, 70, 50, 255));
                    Rect(4, 5, 8, 6, new Color32(235, 180, 70, 255));
                    Rect(6, 7, 4, 2, new Color32(150, 45, 40, 255));
                    break;
                case "wallClock":
                    Rect(5, 5, 6, 8, new Color32(60, 40, 30, 255));
                    Rect(4, 6, 8, 6, new Color32(60, 40, 30, 255));
                    Rect(5, 6, 6, 6, new Color32(250, 245, 230, 255));
                    Rect(8, 9, 1, 3, new Color32(30, 22, 18, 255));
                    Rect(8, 9, 2, 1, new Color32(30, 22, 18, 255));
                    break;
                case "fairyLights":
                    for (int x = 0; x < 16; x++) Rect(x, 11 - (x % 4 == 1 || x % 4 == 2 ? 1 : 0), 1, 1, new Color32(60, 60, 50, 255));
                    Rect(1, 9, 2, 2, new Color32(255, 220, 90, 255));
                    Rect(5, 9, 2, 2, new Color32(240, 110, 80, 255));
                    Rect(9, 9, 2, 2, new Color32(120, 220, 120, 255));
                    Rect(13, 9, 2, 2, new Color32(110, 170, 255, 255));
                    break;
                case "cardIcon":
                    Rect(4, 1, 8, 13, new Color32(30, 22, 18, 255));
                    Rect(5, 2, 6, 11, new Color32(240, 200, 80, 255));
                    Rect(6, 6, 4, 5, new Color32(120, 190, 230, 255));
                    break;
                case "moodHappy": Face(new Color32(120, 220, 110, 255), true, false); break;
                case "moodNeutral": Face(new Color32(250, 215, 80, 255), false, false); break;
                case "moodAngry": Face(new Color32(240, 90, 70, 255), false, true); break;
                case "packSpellforge":
                    Rect(3, 1, 10, 14, new Color32(90, 60, 170, 255));
                    Frame(3, 1, 10, 14, new Color32(45, 25, 90, 255));
                    Rect(7, 5, 2, 7, new Color32(200, 220, 255, 255));
                    Rect(6, 7, 4, 1, new Color32(240, 200, 80, 255));
                    break;
                case "sleeves":
                    Rect(4, 2, 8, 12, new Color32(30, 30, 40, 255));
                    Rect(5, 3, 6, 10, new Color32(220, 60, 60, 255));
                    Rect(5, 10, 6, 2, new Color32(250, 250, 250, 255));
                    break;
                case "binder":
                    Rect(3, 2, 10, 12, new Color32(40, 70, 140, 255));
                    Rect(3, 2, 2, 12, new Color32(25, 45, 90, 255));
                    Rect(7, 7, 4, 3, new Color32(240, 200, 80, 255));
                    break;
                case "playmat":
                    Rect(1, 5, 14, 7, new Color32(40, 120, 90, 255));
                    Frame(1, 5, 14, 7, new Color32(20, 60, 45, 255));
                    Rect(4, 7, 3, 3, new Color32(240, 200, 80, 255));
                    Rect(9, 7, 3, 3, new Color32(240, 200, 80, 255));
                    break;
                case "white":
                    Rect(0, 0, 16, 16, new Color32(255, 255, 255, 255));
                    break;
                case "filament":
                    Rect(4, 3, 8, 10, new Color32(60, 60, 70, 255));
                    Rect(5, 4, 6, 8, new Color32(225, 85, 40, 255));
                    Rect(7, 7, 2, 2, new Color32(60, 60, 70, 255));
                    break;
                case "printSmall":
                    Rect(5, 1, 6, 2, new Color32(80, 80, 90, 255));
                    Rect(5, 3, 6, 6, new Color32(90, 170, 90, 255));
                    Rect(6, 6, 1, 1, new Color32(30, 30, 30, 255));
                    Rect(9, 6, 1, 1, new Color32(30, 30, 30, 255));
                    break;
                case "printMedium":
                    Rect(4, 1, 8, 2, new Color32(80, 80, 90, 255));
                    Rect(6, 3, 4, 8, new Color32(225, 120, 40, 255));
                    Rect(3, 8, 3, 2, new Color32(240, 170, 60, 255));
                    Rect(10, 8, 3, 2, new Color32(240, 170, 60, 255));
                    break;
                case "printLarge":
                    Rect(3, 1, 10, 2, new Color32(80, 80, 90, 255));
                    Rect(5, 3, 6, 8, new Color32(110, 70, 160, 255));
                    Rect(2, 7, 4, 3, new Color32(140, 100, 190, 255));
                    Rect(10, 7, 4, 3, new Color32(140, 100, 190, 255));
                    Rect(6, 11, 4, 3, new Color32(110, 70, 160, 255));
                    break;
                case "printer3d":
                    Rect(1, 0, 14, 15, new Color32(50, 50, 60, 255));
                    Rect(2, 3, 12, 10, new Color32(200, 210, 220, 255));
                    Rect(3, 10, 10, 1, new Color32(80, 80, 90, 255));
                    Rect(6, 8, 3, 2, new Color32(225, 85, 40, 255));
                    Rect(4, 4, 8, 1, new Color32(120, 120, 130, 255));
                    break;
                case "printShelf":
                    Rect(1, 0, 14, 16, new Color32(230, 230, 235, 255));
                    Rect(2, 1, 12, 14, new Color32(180, 190, 200, 255));
                    Rect(2, 5, 12, 1, new Color32(230, 230, 235, 255));
                    Rect(2, 10, 12, 1, new Color32(230, 230, 235, 255));
                    break;
                case "floor2":
                    // Schachbrett-Fliesen für den neuen Raum.
                    Rect(0, 0, 16, 16, new Color32(196, 184, 160, 255));
                    Rect(0, 0, 8, 8, new Color32(170, 156, 132, 255));
                    Rect(8, 8, 8, 8, new Color32(170, 156, 132, 255));
                    break;
                case "gradingStation":
                    Rect(1, 0, 14, 10, new Color32(70, 80, 100, 255));
                    Rect(2, 8, 12, 2, new Color32(230, 230, 240, 255));
                    Rect(3, 10, 2, 5, new Color32(60, 60, 70, 255));
                    Rect(3, 14, 6, 1, new Color32(60, 60, 70, 255));
                    Rect(8, 12, 3, 3, new Color32(255, 240, 160, 255));
                    Rect(9, 3, 4, 4, new Color32(140, 200, 255, 255));
                    Frame(9, 3, 4, 4, new Color32(40, 40, 50, 255));
                    break;
                case "slabIcon":
                    Rect(4, 0, 9, 15, new Color32(190, 215, 240, 255));
                    Rect(5, 11, 7, 3, new Color32(210, 50, 50, 255));
                    Rect(5, 1, 7, 9, new Color32(240, 200, 80, 255));
                    Rect(6, 4, 5, 4, new Color32(120, 190, 230, 255));
                    break;
                case "beanBag":
                    Rect(2, 1, 12, 8, new Color32(200, 70, 90, 255));
                    Rect(3, 9, 10, 4, new Color32(220, 90, 110, 255));
                    Rect(5, 13, 6, 1, new Color32(220, 90, 110, 255));
                    Rect(4, 5, 8, 3, new Color32(170, 50, 70, 255));
                    break;
                case "premiumShowcase":
                    Rect(0, 0, 16, 14, new Color32(40, 30, 50, 255));
                    Rect(1, 2, 14, 10, new Color32(200, 230, 255, 255));
                    Rect(1, 12, 14, 2, new Color32(255, 220, 120, 255));
                    Rect(0, 14, 16, 2, new Color32(240, 200, 80, 255));
                    break;
                case "arcade":
                    Rect(2, 0, 12, 16, new Color32(60, 30, 110, 255));
                    Rect(3, 8, 10, 6, new Color32(20, 20, 30, 255));
                    Rect(4, 9, 8, 4, new Color32(90, 220, 255, 255));
                    Rect(3, 5, 10, 2, new Color32(40, 20, 80, 255));
                    Rect(5, 5, 1, 2, new Color32(240, 80, 80, 255));
                    Rect(9, 5, 2, 1, new Color32(255, 220, 90, 255));
                    Rect(2, 14, 12, 2, new Color32(255, 90, 160, 255));
                    break;
                case "turf":
                    Rect(0, 0, 16, 16, new Color32(80, 160, 70, 255));
                    Speckle(new Color32(110, 190, 90, 255), 30, 3);
                    Speckle(new Color32(60, 130, 55, 255), 20, 4);
                    break;
                case "trophyWall":
                    Rect(0, 3, 16, 11, new Color32(110, 70, 40, 255));
                    Rect(1, 4, 14, 1, new Color32(80, 50, 30, 255));
                    Rect(1, 9, 14, 1, new Color32(80, 50, 30, 255));
                    break;
                case "trophy":
                    Rect(5, 0, 6, 2, new Color32(120, 80, 40, 255));
                    Rect(7, 2, 2, 4, new Color32(240, 190, 60, 255));
                    Rect(4, 6, 8, 6, new Color32(250, 210, 80, 255));
                    Rect(2, 9, 2, 3, new Color32(250, 210, 80, 255));
                    Rect(12, 9, 2, 3, new Color32(250, 210, 80, 255));
                    break;
                case "moonStatue":
                    Rect(3, 0, 10, 3, new Color32(120, 120, 130, 255));
                    Rect(7, 3, 2, 5, new Color32(150, 150, 160, 255));
                    Circle(6, 10, 3, new Color32(225, 85, 40, 255));
                    Circle(10, 11, 3, new Color32(245, 200, 80, 255));
                    break;
                case "papa":
                    Rect(5, 0, 2, 3, new Color32(60, 50, 50, 255));
                    Rect(9, 0, 2, 3, new Color32(60, 50, 50, 255));
                    Rect(4, 3, 8, 6, new Color32(110, 90, 70, 255));
                    Rect(7, 3, 2, 6, new Color32(200, 190, 170, 255));
                    Rect(5, 9, 6, 5, new Color32(240, 200, 160, 255));
                    Rect(5, 13, 6, 2, new Color32(200, 200, 205, 255));
                    Rect(6, 11, 1, 1, new Color32(30, 20, 25, 255));
                    Rect(9, 11, 1, 1, new Color32(30, 20, 25, 255));
                    Rect(6, 9, 4, 1, new Color32(200, 200, 205, 255));
                    break;
                case "neonSign":
                    Rect(1, 5, 14, 7, new Color32(30, 20, 40, 255));
                    Rect(2, 7, 4, 3, new Color32(255, 90, 160, 255));
                    Rect(7, 7, 3, 3, new Color32(90, 220, 255, 255));
                    Rect(11, 7, 3, 3, new Color32(255, 220, 90, 255));
                    break;
                case "packSoulClash":
                    Rect(3, 1, 10, 14, new Color32(200, 50, 40, 255));
                    Frame(3, 1, 10, 14, new Color32(90, 20, 20, 255));
                    Rect(6, 4, 1, 8, new Color32(230, 230, 240, 255));
                    Rect(9, 4, 1, 8, new Color32(230, 230, 240, 255));
                    Rect(5, 7, 6, 1, new Color32(240, 200, 80, 255));
                    break;
                case "soda":
                    Rect(6, 1, 4, 10, new Color32(90, 190, 90, 255));
                    Rect(7, 11, 2, 3, new Color32(90, 190, 90, 255));
                    Rect(6, 5, 4, 3, new Color32(250, 240, 200, 255));
                    break;
                case "energyDrink":
                    Rect(5, 1, 6, 11, new Color32(40, 40, 50, 255));
                    Rect(5, 4, 6, 5, new Color32(120, 230, 60, 255));
                    break;
                case "fridge":
                    Rect(1, 0, 14, 16, new Color32(220, 225, 235, 255));
                    Rect(2, 1, 12, 14, new Color32(150, 200, 230, 255));
                    for (int y = 4; y < 15; y += 4) Rect(2, y, 12, 1, new Color32(220, 225, 235, 255));
                    Rect(12, 6, 1, 4, new Color32(120, 120, 130, 255));
                    break;
                case "couch":
                    Rect(0, 2, 16, 9, new Color32(150, 60, 50, 255));
                    Rect(0, 8, 16, 5, new Color32(120, 45, 40, 255));
                    Rect(0, 2, 2, 9, new Color32(110, 40, 35, 255));
                    Rect(14, 2, 2, 9, new Color32(110, 40, 35, 255));
                    Rect(3, 5, 4, 3, new Color32(240, 200, 80, 255));
                    break;
                case "tv":
                    Rect(1, 4, 14, 10, new Color32(25, 25, 30, 255));
                    Rect(2, 5, 12, 8, new Color32(90, 160, 230, 255));
                    Rect(7, 2, 2, 2, new Color32(25, 25, 30, 255));
                    break;
                case "console":
                    Rect(3, 3, 10, 5, new Color32(40, 40, 50, 255));
                    Rect(4, 6, 2, 1, new Color32(120, 220, 140, 255));
                    Rect(2, 9, 5, 3, new Color32(60, 60, 70, 255));
                    Rect(9, 9, 5, 3, new Color32(60, 60, 70, 255));
                    break;
                case "employee":
                    Person(new Color32(60, 140, 90, 255), new Color32(30, 30, 35, 255));
                    Rect(5, 3, 6, 5, new Color32(240, 200, 80, 255));
                    break;
                case "gameTable":
                    Rect(1, 3, 14, 11, new Color32(120, 80, 50, 255));
                    Rect(2, 4, 12, 9, new Color32(40, 110, 80, 255));
                    Rect(4, 6, 3, 4, new Color32(245, 235, 210, 255));
                    Rect(9, 7, 3, 4, new Color32(245, 235, 210, 255));
                    Rect(2, 0, 2, 3, new Color32(90, 60, 40, 255));
                    Rect(12, 0, 2, 3, new Color32(90, 60, 40, 255));
                    break;
                case "prizeCounter":
                    Rect(0, 0, 16, 12, new Color32(110, 40, 30, 255));
                    Rect(0, 10, 16, 3, new Color32(225, 85, 40, 255));
                    Rect(6, 12, 4, 3, new Color32(245, 200, 80, 255));
                    Rect(5, 15, 6, 1, new Color32(245, 200, 80, 255));
                    Rect(3, 4, 10, 4, new Color32(245, 200, 80, 255));
                    break;
                case "tournamentBoard":
                    Rect(2, 3, 12, 11, new Color32(90, 60, 40, 255));
                    Rect(3, 4, 10, 9, new Color32(40, 60, 50, 255));
                    Rect(4, 11, 8, 1, new Color32(240, 240, 230, 255));
                    Rect(4, 8, 6, 1, new Color32(240, 240, 230, 255));
                    Rect(4, 6, 7, 1, new Color32(240, 240, 230, 255));
                    break;
                case "accessoryStand":
                    Rect(2, 0, 12, 16, new Color32(70, 80, 95, 255));
                    Rect(3, 1, 10, 14, new Color32(45, 50, 60, 255));
                    for (int y = 4; y < 15; y += 4) Rect(3, y, 10, 1, new Color32(150, 160, 175, 255));
                    break;
                case "packMonstera":
                    Rect(3, 1, 10, 14, new Color32(60, 170, 90, 255));
                    Frame(3, 1, 10, 14, new Color32(30, 100, 50, 255));
                    Rect(7, 6, 2, 5, new Color32(250, 220, 80, 255));
                    Rect(5, 8, 6, 1, new Color32(250, 220, 80, 255));
                    break;
                case "player1": Person(new Color32(230, 90, 90, 255), new Color32(70, 45, 30, 255)); break;
                case "player2": Person(new Color32(90, 140, 230, 255), new Color32(230, 200, 90, 255)); break;
                case "player3": Person(new Color32(160, 100, 210, 255), new Color32(35, 35, 40, 255)); break;
                case "customer1": Person(new Color32(70, 180, 170, 255), new Color32(120, 70, 40, 255)); break;
                case "customer2": Person(new Color32(240, 150, 60, 255), new Color32(50, 40, 35, 255)); break;
                case "customer3": Person(new Color32(140, 140, 150, 255), new Color32(200, 90, 60, 255)); break;
                default:
                    Rect(0, 0, 16, 16, new Color32(255, 0, 255, 255));
                    break;
            }

            texture.SetPixels32(px);
            texture.Apply();
            return texture;
        }
    }
}
