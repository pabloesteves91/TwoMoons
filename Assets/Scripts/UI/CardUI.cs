using UnityEngine;

namespace TwoMoons
{
    /// <summary>Zeichnet Karten in Menüs – Ultra-Seltene mit schimmerndem Holo-Effekt.</summary>
    public static class CardUI
    {
        static Texture2D rainbow;
        static Texture2D white;

        /// <summary>Karte mit Bild, Name und Nummer. Bei <paramref name="small"/> ohne Text.</summary>
        public static void Draw(Rect rect, CardDef card, bool small = false)
        {
            GUI.DrawTexture(rect, CardArt.Get(card), ScaleMode.StretchToFill);
            if (card.Rarity == Rarity.Ultra) DrawShine(rect);
            if (small) return;

            // Name und Nummer im hellen Textfeld unten auf der Karte (unten 10 von 32 Pixeln).
            float pixel = rect.height / CardArt.Height;
            var textArea = new Rect(rect.x + 3f * pixel, rect.yMax - 10f * pixel, rect.width - 6f * pixel, 7f * pixel);
            var nameStyle = new GUIStyle(UiKit.Bold) { alignment = TextAnchor.UpperCenter, fontSize = Mathf.RoundToInt(pixel * 2.4f), wordWrap = false };
            nameStyle.normal.textColor = new Color(0.18f, 0.1f, 0.06f);
            GUI.Label(new Rect(textArea.x, textArea.y + pixel * 2.4f, textArea.width, textArea.height), card.Name, nameStyle);

            var numberStyle = new GUIStyle(nameStyle) { alignment = TextAnchor.UpperRight, fontSize = Mathf.RoundToInt(pixel * 1.6f), fontStyle = FontStyle.Normal };
            GUI.Label(new Rect(textArea.x, textArea.y + pixel * 0.3f, textArea.width - pixel, textArea.height), $"#{card.Number:00}/{CardDatabase.CardsPerGame}", numberStyle);
        }

        /// <summary>Versiegelte Karte in der Schutzhülle („Slab“) mit Notenschild oben.</summary>
        public static void DrawSlab(Rect rect, CardDef card, int grade)
        {
            EnsureTextures();
            GUI.color = new Color(0.78f, 0.86f, 0.95f, 0.95f);
            GUI.DrawTexture(rect, white);
            float label = rect.height * 0.18f;
            GUI.color = grade >= 10 ? UiKit.Gold : grade >= 8 ? new Color(0.85f, 0.2f, 0.2f) : new Color(0.35f, 0.4f, 0.5f);
            var labelRect = new Rect(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.03f, rect.width * 0.88f, label);
            GUI.DrawTexture(labelRect, white);
            GUI.color = Color.white;
            var style = new GUIStyle(UiKit.Bold) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.Max(8, Mathf.RoundToInt(label * 0.7f)) };
            style.normal.textColor = Color.white;
            GUI.Label(labelRect, $"TM {grade}", style);

            float inset = rect.width * 0.1f;
            var cardRect = new Rect(rect.x + inset, labelRect.yMax + rect.height * 0.03f, rect.width - 2f * inset, rect.yMax - labelRect.yMax - rect.height * 0.06f);
            Draw(cardRect, card, true);
        }

        public static void DrawBack(Rect rect) => GUI.DrawTexture(rect, CardArt.Back, ScaleMode.StretchToFill);

        /// <summary>Wandernder Regenbogen-Streifen und funkelnde Punkte über der Karte.</summary>
        static void DrawShine(Rect rect)
        {
            EnsureTextures();
            float t = Time.unscaledTime;

            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            GUI.DrawTextureWithTexCoords(rect, rainbow, new Rect(t * 0.35f, 0f, 0.6f, 1f));

            for (int i = 0; i < 5; i++)
            {
                float phase = Mathf.Repeat(t * 0.7f + i * 0.37f, 1f);
                float alpha = Mathf.Sin(phase * Mathf.PI);
                float x = Mathf.Repeat(i * 0.61f + 0.13f, 1f);
                float y = Mathf.Repeat(i * 0.37f + 0.21f, 0.6f);
                float size = Mathf.Max(3f, rect.width * 0.05f) * alpha;
                GUI.color = new Color(1f, 1f, 0.9f, alpha);
                GUI.DrawTexture(new Rect(rect.x + rect.width * x - size / 2f, rect.y + rect.height * y - size / 2f, size, size), white);
            }
            GUI.color = Color.white;
        }

        static void EnsureTextures()
        {
            if (rainbow != null && white != null) return;
            rainbow = new Texture2D(64, 1, TextureFormat.RGBA32, false, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave,
            };
            for (int x = 0; x < 64; x++)
            {
                var color = Color.HSVToRGB(x / 64f, 0.6f, 1f);
                rainbow.SetPixel(x, 0, color);
            }
            rainbow.Apply();

            white = new Texture2D(1, 1, TextureFormat.RGBA32, false, false) { hideFlags = HideFlags.HideAndDontSave };
            white.SetPixel(0, 0, Color.white);
            white.Apply();
        }
    }
}
