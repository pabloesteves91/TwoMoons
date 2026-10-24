using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Pixelbilder für die Intro-Geschichte. Die Szenen sind 160×90 Pixel groß.
    /// Eigene Bilder mit demselben Namen in Assets/Art/Intro/Resources/Intro/ ersetzen die Platzhalter
    /// (z. B. bedroom.png, kitchen.png, shopFront.png, shopEmpty.png, father.png).
    /// </summary>
    public static class IntroArt
    {
        public const int Width = 160;
        public const int Height = 90;

        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string key)
        {
            if (cache.TryGetValue(key, out var texture) && texture != null) return texture;
            texture = Resources.Load<Texture2D>($"Intro/{key}");
            if (texture == null) texture = Create(key);
            texture.filterMode = FilterMode.Point;
            cache[key] = texture;
            return texture;
        }

        /// <summary>Kleine Leinwand zum Malen einzelner Pixel. Y zeigt nach oben, wie in Unity-Texturen.</summary>
        class Canvas
        {
            public readonly int W, H;
            readonly Color32[] px;

            public Canvas(int w, int h)
            {
                W = w;
                H = h;
                px = new Color32[w * h];
            }

            public void Rect(int x, int y, int w, int h, Color32 c)
            {
                for (int yy = Mathf.Max(0, y); yy < Mathf.Min(H, y + h); yy++)
                for (int xx = Mathf.Max(0, x); xx < Mathf.Min(W, x + w); xx++)
                    px[yy * W + xx] = c;
            }

            public void Frame(int x, int y, int w, int h, Color32 c)
            {
                Rect(x, y, w, 1, c);
                Rect(x, y + h - 1, w, 1, c);
                Rect(x, y, 1, h, c);
                Rect(x + w - 1, y, 1, h, c);
            }

            public void Circle(int cx, int cy, int r, Color32 c)
            {
                for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r + r) Rect(cx + x, cy + y, 1, 1, c);
            }

            /// <summary>Zufällige Punkte, z. B. Sterne, Staub oder Holzmaserung.</summary>
            public void Speckle(int x, int y, int w, int h, int count, Color32 c, int seed)
            {
                var random = new System.Random(seed);
                for (int i = 0; i < count; i++) Rect(x + random.Next(w), y + random.Next(h), 1, 1, c);
            }

            public Texture2D ToTexture(string name)
            {
                var texture = new Texture2D(W, H, TextureFormat.RGBA32, false, false)
                {
                    name = name,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                texture.SetPixels32(px);
                texture.Apply();
                return texture;
            }
        }

        static readonly Color32 Outline = new Color32(30, 20, 25, 255);
        static readonly Color32 Skin = new Color32(240, 200, 160, 255);

        static Texture2D Create(string key)
        {
            switch (key)
            {
                case "bedroom": return Bedroom();
                case "nightSky": return NightSky();
                case "kitchen": return Kitchen();
                case "shopFront": return ShopFront();
                case "shopEmpty": return ShopEmpty();
                case "moonRed": return Moon(new Color32(225, 85, 40, 255), new Color32(170, 55, 30, 255));
                case "moonGold": return Moon(new Color32(245, 200, 80, 255), new Color32(200, 150, 50, 255));
                case "father": return Father();
                case "door": return Door();
                case "thought": return Thought();
                case "cake": return Cake();
                case "key": return Key();
                default: return new Canvas(1, 1).ToTexture(key);
            }
        }

        static Texture2D Bedroom()
        {
            var c = new Canvas(Width, Height);
            // Wand mit Tapetenstreifen, Holzboden
            c.Rect(0, 0, Width, Height, new Color32(52, 46, 84, 255));
            for (int x = 4; x < Width; x += 12) c.Rect(x, 20, 1, Height - 20, new Color32(60, 54, 94, 255));
            c.Rect(0, 0, Width, 20, new Color32(92, 64, 48, 255));
            for (int y = 4; y < 20; y += 5) c.Rect(0, y, Width, 1, new Color32(76, 52, 40, 255));
            c.Rect(0, 19, Width, 2, new Color32(40, 30, 30, 255));

            // Fensteröffnung (der Himmel dahinter wird separat gezeichnet → hier transparent lassen)
            c.Rect(92, 40, 44, 34, new Color32(0, 0, 0, 0));
            c.Frame(91, 39, 46, 36, new Color32(120, 90, 60, 255));
            c.Frame(90, 38, 48, 38, new Color32(90, 66, 44, 255));
            c.Rect(113, 40, 2, 34, new Color32(120, 90, 60, 255));
            c.Rect(92, 56, 44, 2, new Color32(120, 90, 60, 255));
            c.Rect(88, 36, 52, 3, new Color32(150, 110, 70, 255));

            // Poster mit einer Monster-Karte
            c.Rect(46, 50, 16, 22, Outline);
            c.Rect(47, 51, 14, 20, new Color32(240, 200, 80, 255));
            c.Rect(49, 58, 10, 11, new Color32(120, 190, 230, 255));
            c.Circle(54, 62, 3, new Color32(90, 170, 90, 255));

            // Regal mit Kartenstapeln und Spielzeug
            c.Rect(66, 42, 18, 2, new Color32(120, 80, 50, 255));
            c.Rect(68, 44, 4, 5, new Color32(200, 70, 60, 255));
            c.Rect(73, 44, 3, 4, new Color32(80, 140, 220, 255));
            c.Rect(78, 44, 5, 3, new Color32(240, 210, 90, 255));

            // Bett links: Kopfteil, Kissen, Decke (der Junge wird darübergezeichnet)
            c.Rect(4, 18, 6, 26, new Color32(110, 70, 45, 255));
            c.Rect(4, 18, 54, 12, new Color32(110, 70, 45, 255));
            c.Rect(10, 28, 46, 8, new Color32(230, 230, 240, 255));
            c.Rect(10, 28, 14, 10, new Color32(250, 250, 255, 255));
            c.Rect(22, 26, 36, 12, new Color32(70, 120, 200, 255));
            for (int x = 24; x < 58; x += 6) c.Rect(x, 30, 3, 3, new Color32(240, 200, 80, 255));
            c.Rect(56, 18, 4, 20, new Color32(110, 70, 45, 255));

            // Nachttisch mit Lampe
            c.Rect(62, 18, 12, 12, new Color32(120, 80, 50, 255));
            c.Rect(66, 30, 4, 6, new Color32(240, 220, 150, 255));
            c.Rect(64, 36, 8, 4, new Color32(200, 120, 70, 255));

            // Türrahmen rechts
            c.Rect(142, 18, 18, 46, new Color32(30, 24, 30, 255));
            c.Frame(141, 17, 19, 48, new Color32(110, 80, 55, 255));
            return c.ToTexture("bedroom");
        }

        static Texture2D NightSky()
        {
            var c = new Canvas(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                float t = y / (float)Height;
                c.Rect(0, y, Width, 1, Color.Lerp(new Color(0.12f, 0.1f, 0.3f), new Color(0.03f, 0.03f, 0.1f), t));
            }
            c.Speckle(0, 10, Width, Height - 10, 60, new Color32(255, 255, 230, 255), 7);
            // Silhouette der Stadt
            var random = new System.Random(3);
            for (int x = 0; x < Width; x += 6)
            {
                int h = 6 + random.Next(16);
                c.Rect(x, 0, 6, h, new Color32(15, 12, 25, 255));
                if (random.Next(3) == 0) c.Rect(x + 2, h - 5, 1, 1, new Color32(250, 220, 120, 255));
            }
            return c.ToTexture("nightSky");
        }

        static Texture2D Moon(Color32 light, Color32 dark)
        {
            var c = new Canvas(14, 14);
            c.Circle(7, 7, 6, Outline);
            c.Circle(7, 7, 5, light);
            c.Circle(5, 8, 1, dark);
            c.Circle(9, 5, 1, dark);
            c.Rect(8, 9, 2, 1, dark);
            return c.ToTexture("moon");
        }

        static Texture2D Father()
        {
            var c = new Canvas(16, 26);
            var shirt = new Color32(70, 110, 90, 255);
            var pants = new Color32(60, 55, 70, 255);
            var hair = new Color32(90, 60, 40, 255);
            c.Rect(4, 0, 3, 9, pants);
            c.Rect(9, 0, 3, 9, pants);
            c.Rect(3, 9, 10, 9, shirt);
            c.Rect(2, 10, 1, 7, Skin);
            c.Rect(13, 10, 1, 7, Skin);
            c.Rect(5, 18, 6, 7, Skin);
            c.Rect(5, 23, 6, 2, hair);
            c.Rect(4, 20, 1, 5, hair);
            c.Rect(11, 20, 1, 5, hair);
            c.Rect(6, 21, 1, 1, Outline);
            c.Rect(9, 21, 1, 1, Outline);
            c.Rect(6, 19, 4, 1, hair); // Schnurrbart
            return c.ToTexture("father");
        }

        static Texture2D Door()
        {
            var c = new Canvas(16, 44);
            c.Rect(0, 0, 16, 44, new Color32(140, 95, 60, 255));
            c.Frame(2, 24, 12, 16, new Color32(110, 75, 48, 255));
            c.Frame(2, 4, 12, 16, new Color32(110, 75, 48, 255));
            c.Rect(12, 20, 2, 2, new Color32(230, 200, 90, 255));
            return c.ToTexture("door");
        }

        static Texture2D Thought()
        {
            var c = new Canvas(44, 34);
            var white = new Color32(250, 248, 240, 255);
            c.Circle(4, 3, 2, white);
            c.Circle(9, 8, 3, white);
            c.Rect(14, 10, 30, 24, white);
            c.Circle(14, 22, 10, white);
            c.Circle(43, 22, 1, white);
            // Mini-Laden: Regale mit bunten Karten, Kasse, ein Kunde
            c.Rect(16, 12, 26, 20, new Color32(200, 160, 110, 255));
            c.Rect(17, 26, 8, 5, new Color32(120, 80, 50, 255));
            c.Rect(27, 26, 8, 5, new Color32(120, 80, 50, 255));
            c.Rect(18, 28, 2, 2, new Color32(90, 170, 90, 255));
            c.Rect(21, 28, 2, 2, new Color32(220, 80, 60, 255));
            c.Rect(28, 28, 2, 2, new Color32(80, 140, 220, 255));
            c.Rect(31, 28, 2, 2, new Color32(240, 210, 80, 255));
            c.Rect(36, 14, 5, 6, new Color32(60, 60, 72, 255));
            c.Rect(20, 13, 3, 6, new Color32(240, 150, 60, 255));
            c.Rect(20, 19, 3, 3, Skin);
            return c.ToTexture("thought");
        }

        static Texture2D Kitchen()
        {
            var c = new Canvas(Width, Height);
            c.Rect(0, 0, Width, Height, new Color32(236, 206, 156, 255));
            for (int y = 24; y < Height; y += 8) c.Rect(0, y, Width, 1, new Color32(226, 194, 144, 255));
            c.Rect(0, 0, Width, 22, new Color32(170, 110, 70, 255));
            for (int x = 0; x < Width; x += 10) c.Rect(x, 0, 1, 22, new Color32(150, 95, 60, 255));
            // Fenster mit Morgenhimmel
            c.Rect(20, 46, 36, 28, new Color32(150, 200, 240, 255));
            c.Rect(20, 46, 36, 6, new Color32(250, 210, 160, 255));
            c.Frame(19, 45, 38, 30, new Color32(255, 255, 255, 255));
            c.Rect(37, 46, 2, 28, new Color32(255, 255, 255, 255));
            // Girlande
            for (int x = 70; x < 150; x += 8)
            {
                c.Rect(x, 72 - (x / 8 % 2), 6, 1, new Color32(120, 90, 60, 255));
                c.Rect(x + 2, 66 - (x / 8 % 2), 3, 5, x / 8 % 2 == 0 ? new Color32(225, 85, 40, 255) : new Color32(245, 200, 80, 255));
            }
            // Tisch
            c.Rect(50, 30, 70, 4, new Color32(150, 95, 55, 255));
            c.Rect(50, 26, 70, 4, new Color32(240, 240, 245, 255));
            c.Rect(54, 10, 4, 16, new Color32(120, 75, 45, 255));
            c.Rect(112, 10, 4, 16, new Color32(120, 75, 45, 255));
            return c.ToTexture("kitchen");
        }

        static Texture2D Cake()
        {
            var c = new Canvas(20, 18);
            c.Rect(1, 0, 18, 10, new Color32(250, 235, 220, 255));
            c.Rect(1, 6, 18, 2, new Color32(225, 85, 40, 255));
            c.Rect(1, 10, 18, 2, new Color32(250, 250, 255, 255));
            c.Rect(0, 0, 20, 1, Outline);
            // Kerzen (Flammen werden animiert dazugezeichnet)
            for (int x = 4; x < 18; x += 5) c.Rect(x, 12, 1, 4, new Color32(120, 190, 230, 255));
            return c.ToTexture("cake");
        }

        static Texture2D Key()
        {
            var c = new Canvas(14, 8);
            var gold = new Color32(240, 200, 80, 255);
            c.Circle(3, 4, 3, gold);
            c.Circle(3, 4, 1, new Color32(0, 0, 0, 0));
            c.Rect(6, 3, 8, 2, gold);
            c.Rect(11, 1, 1, 2, gold);
            c.Rect(13, 1, 1, 2, gold);
            return c.ToTexture("key");
        }

        static Texture2D ShopFront()
        {
            var c = new Canvas(Width, Height);
            // Himmel
            for (int y = 0; y < Height; y++)
                c.Rect(0, y, Width, 1, Color.Lerp(new Color(0.98f, 0.82f, 0.62f), new Color(0.55f, 0.75f, 0.95f), y / (float)Height));
            // Gehweg und Straße
            c.Rect(0, 0, Width, 10, new Color32(110, 110, 120, 255));
            c.Rect(0, 10, Width, 6, new Color32(160, 160, 165, 255));
            for (int x = 0; x < Width; x += 16) c.Rect(x, 10, 1, 6, new Color32(140, 140, 145, 255));
            // Nachbarhäuser
            c.Rect(0, 16, 30, 60, new Color32(150, 120, 110, 255));
            c.Rect(130, 16, 30, 66, new Color32(120, 130, 150, 255));
            // Ladenhaus
            c.Rect(30, 16, 100, 62, new Color32(88, 70, 110, 255));
            c.Rect(28, 76, 104, 4, new Color32(60, 48, 80, 255));
            // Leeres Ladenschild
            c.Rect(50, 62, 60, 10, new Color32(120, 100, 80, 255));
            c.Frame(50, 62, 60, 10, new Color32(70, 55, 45, 255));
            // Verstaubtes Schaufenster mit Brettern
            c.Rect(36, 22, 44, 34, new Color32(150, 160, 160, 255));
            c.Speckle(36, 22, 44, 34, 160, new Color32(170, 175, 170, 255), 11);
            c.Frame(35, 21, 46, 36, new Color32(60, 45, 40, 255));
            for (int i = 0; i < 40; i++)
            {
                c.Rect(38 + i, 24 + (int)(i * 0.75f), 3, 2, new Color32(150, 100, 60, 255));
                c.Rect(78 - i, 24 + (int)(i * 0.75f), 3, 2, new Color32(140, 92, 55, 255));
            }
            // Tür
            c.Rect(92, 16, 22, 40, new Color32(120, 40, 50, 255));
            c.Frame(92, 16, 22, 40, new Color32(70, 30, 30, 255));
            c.Rect(108, 34, 2, 2, new Color32(230, 200, 90, 255));
            // Zettel an der Tür
            c.Rect(97, 40, 10, 8, new Color32(240, 235, 210, 255));
            c.Rect(98, 45, 8, 1, new Color32(150, 140, 130, 255));
            c.Rect(98, 43, 6, 1, new Color32(150, 140, 130, 255));
            return c.ToTexture("shopFront");
        }

        static Texture2D ShopEmpty()
        {
            var c = new Canvas(Width, Height);
            c.Rect(0, 0, Width, Height, new Color32(80, 64, 98, 255));
            c.Rect(0, 0, Width, 30, new Color32(150, 112, 80, 255));
            for (int y = 3; y < 30; y += 5) c.Rect(0, y, Width, 1, new Color32(128, 95, 68, 255));
            c.Rect(0, 29, Width, 2, new Color32(50, 40, 55, 255));
            // Staub auf dem Boden
            c.Speckle(0, 0, Width, 29, 220, new Color32(170, 150, 130, 255), 21);
            // Spinnweben in den Ecken
            for (int i = 0; i < 14; i++)
            {
                c.Rect(i, Height - 1 - i, 1, 1, new Color32(220, 220, 230, 255));
                c.Rect(Width - 1 - i, Height - 1 - i, 1, 1, new Color32(220, 220, 230, 255));
            }
            c.Rect(0, Height - 8, 8, 1, new Color32(220, 220, 230, 255));
            c.Rect(7, Height - 8, 1, 8, new Color32(220, 220, 230, 255));
            c.Rect(Width - 8, Height - 8, 8, 1, new Color32(220, 220, 230, 255));
            c.Rect(Width - 8, Height - 8, 1, 8, new Color32(220, 220, 230, 255));
            // Altes, schiefes Regal
            for (int i = 0; i < 30; i++) c.Rect(110 + i / 6, 30 + i, 3, 1, new Color32(110, 70, 45, 255));
            for (int i = 0; i < 30; i++) c.Rect(132 + i / 6, 30 + i, 3, 1, new Color32(110, 70, 45, 255));
            c.Rect(111, 40, 24, 2, new Color32(140, 95, 60, 255));
            c.Rect(113, 50, 24, 2, new Color32(140, 95, 60, 255));
            // Leere Theke links, Fenster mit Licht
            c.Rect(10, 30, 40, 14, new Color32(120, 80, 55, 255));
            c.Rect(10, 42, 40, 3, new Color32(170, 120, 80, 255));
            c.Rect(62, 46, 30, 26, new Color32(200, 190, 160, 255));
            c.Frame(61, 45, 32, 28, new Color32(60, 45, 40, 255));
            // Ein Karton Monstera-Packs
            c.Rect(70, 14, 14, 10, new Color32(196, 150, 90, 255));
            c.Rect(76, 14, 2, 10, new Color32(230, 215, 170, 255));
            c.Rect(72, 24, 4, 3, new Color32(60, 170, 90, 255));
            return c.ToTexture("shopEmpty");
        }
    }
}
