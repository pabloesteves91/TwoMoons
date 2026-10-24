using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Kartenbilder. Eigene Bilder (z. B. 24×32 Pixel) als Assets/Art/Cards/Resources/Cards/monstera_01.png usw. ablegen,
    /// sonst wird ein Platzhalter-Monster gezeichnet.
    /// </summary>
    public static class CardArt
    {
        public const int Width = 24;
        public const int Height = 32;

        static readonly Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();
        static Texture2D back;

        public static Texture2D Get(CardDef card)
        {
            if (cache.TryGetValue(card.Id, out var texture) && texture != null) return texture;
            texture = Resources.Load<Texture2D>($"Cards/{card.ArtKey}");
            if (texture == null) texture = card == CardDatabase.TwoMoons ? CreateTwoMoons() : CreatePlaceholder(card);
            cache[card.Id] = texture;
            return texture;
        }

        /// <summary>Kartenrückseite in den Logo-Farben.</summary>
        public static Texture2D Back
        {
            get
            {
                if (back != null) return back;
                back = NewTexture();
                var px = new Color32[Width * Height];
                Fill(px, 0, 0, Width, Height, new Color32(40, 20, 15, 255));
                Fill(px, 1, 1, Width - 2, Height - 2, new Color32(150, 50, 25, 255));
                Fill(px, 3, 3, Width - 6, Height - 6, new Color32(90, 30, 18, 255));
                Circle(px, 9, 18, 5, new Color32(230, 90, 40, 255));
                Circle(px, 15, 13, 5, new Color32(240, 190, 70, 255));
                back.SetPixels32(px);
                back.Apply();
                return back;
            }
        }

        static Texture2D NewTexture() => new Texture2D(Width, Height, TextureFormat.RGBA32, false, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };

        static Texture2D CreatePlaceholder(CardDef card)
        {
            var texture = NewTexture();
            var px = new Color32[Width * Height];
            var random = new System.Random(card.Id * 7919 + 13);

            Color32 frame = CardDatabase.RarityColor(card.Rarity);
            var dark = new Color32(30, 22, 18, 255);
            // Jedes Spiel hat seine eigene Farbwelt: Monstera bunt, Spellforge violett-blau, Soul Clash rot-orange.
            float h = card.Game == TcgGame.Spellforge ? 0.62f + (float)random.NextDouble() * 0.18f
                : card.Game == TcgGame.SoulClash ? (float)random.NextDouble() * 0.1f
                : (float)random.NextDouble();
            Color32 sky = Color.HSVToRGB(h, 0.3f, 0.95f);
            Color32 body = Color.HSVToRGB((h + 0.5f) % 1f, 0.65f, 0.8f);
            Color32 belly = Color.HSVToRGB((h + 0.5f) % 1f, 0.3f, 1f);

            Fill(px, 0, 0, Width, Height, dark);
            Fill(px, 1, 1, Width - 2, Height - 2, frame);
            // Bildfeld oben, Textfeld unten
            Fill(px, 3, 12, Width - 6, Height - 15, sky);
            Fill(px, 3, 3, Width - 6, 7, new Color32(245, 235, 210, 255));

            // Monster: Körper, Bauch, Augen, Ohren/Hörner je nach Zufall
            int cx = 12, cy = 19, r = 5 + random.Next(0, 2);
            Circle(px, cx, cy, r, body);
            Circle(px, cx, cy - 1, r - 2, belly);
            Fill(px, cx - 3, cy + 1, 2, 2, dark);
            Fill(px, cx + 1, cy + 1, 2, 2, dark);
            if (random.Next(2) == 0)
            {
                Fill(px, cx - r + 1, cy + r - 1, 2, 3, body);
                Fill(px, cx + r - 2, cy + r - 1, 2, 3, body);
            }
            else
            {
                Fill(px, cx - 1, cy + r, 2, 3, frame);
            }
            Fill(px, cx - 2, cy - 2, 4, 1, dark);

            // Seltenheits-Steine im Textfeld
            int gems = (int)card.Rarity + 1;
            for (int i = 0; i < gems; i++) Fill(px, 5 + i * 3, 5, 2, 2, frame);

            texture.SetPixels32(px);
            texture.Apply();
            return texture;
        }

        /// <summary>Platzhalter für die Legende: Nachthimmel mit rotem und goldenem Mond, goldener Rahmen.</summary>
        static Texture2D CreateTwoMoons()
        {
            var texture = NewTexture();
            var px = new Color32[Width * Height];
            var gold = new Color32(245, 200, 80, 255);
            Fill(px, 0, 0, Width, Height, new Color32(30, 22, 18, 255));
            Fill(px, 1, 1, Width - 2, Height - 2, gold);
            Fill(px, 3, 12, Width - 6, Height - 15, new Color32(25, 25, 70, 255));
            var random = new System.Random(2);
            for (int i = 0; i < 14; i++) Fill(px, 3 + random.Next(Width - 6), 12 + random.Next(Height - 15), 1, 1, new Color32(255, 250, 220, 255));
            Circle(px, 9, 22, 4, new Color32(225, 85, 40, 255));
            Circle(px, 15, 18, 4, gold);
            Fill(px, 3, 3, Width - 6, 7, new Color32(245, 235, 210, 255));
            for (int i = 0; i < 5; i++) Fill(px, 5 + i * 3, 5, 2, 2, gold);
            texture.SetPixels32(px);
            texture.Apply();
            return texture;
        }

        static void Fill(Color32[] px, int x, int y, int w, int h, Color32 c)
        {
            for (int yy = Mathf.Max(0, y); yy < Mathf.Min(Height, y + h); yy++)
            for (int xx = Mathf.Max(0, x); xx < Mathf.Min(Width, x + w); xx++)
                px[yy * Width + xx] = c;
        }

        static void Circle(Color32[] px, int cx, int cy, int radius, Color32 c)
        {
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius) Fill(px, cx + x, cy + y, 1, 1, c);
        }
    }
}
