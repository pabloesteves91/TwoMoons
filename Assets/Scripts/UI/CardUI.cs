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

            DrawTextField(rect, card);
        }

        static GUIStyle nameStyle, numberStyle, rarityStyle;
        static readonly GUIContent content = new GUIContent();

        /// <summary>
        /// Helles Textfeld unten auf der Karte (Pixelzeilen 3–10 von 32): oben links die Seltenheit,
        /// oben rechts die Nummer, darunter mittig der Name – zu lange Namen werden kleiner statt abgeschnitten.
        /// </summary>
        static void DrawTextField(Rect rect, CardDef card)
        {
            EnsureTextures();
            float pixel = rect.height / CardArt.Height;
            var field = new Rect(rect.x + 3f * pixel, rect.yMax - 10f * pixel, rect.width - 6f * pixel, 7f * pixel);
            var ink = new Color(0.22f, 0.13f, 0.08f);
            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(UiKit.Bold) { alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Overflow };
                numberStyle = new GUIStyle(UiKit.Text) { alignment = TextAnchor.MiddleRight, wordWrap = false, clipping = TextClipping.Overflow };
            }
            nameStyle.normal.textColor = ink;
            numberStyle.normal.textColor = new Color(ink.r, ink.g, ink.b, 0.75f);

            // Obere Zeile: Seltenheit links, Nummer rechts
            var top = new Rect(field.x + pixel * 0.8f, field.y + pixel * 0.5f, field.width - pixel * 1.6f, pixel * 2f);
            // Seltenheit als kleines Wort, in dunkler Seltenheitsfarbe
            numberStyle.fontSize = Mathf.Max(7, Mathf.RoundToInt(pixel * 1.5f));
            rarityStyle ??= new GUIStyle(numberStyle) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            rarityStyle.fontSize = numberStyle.fontSize;
            rarityStyle.normal.textColor = card == CardDatabase.TwoMoons || card.Rarity == Rarity.Ultra ? new Color(0.7f, 0.45f, 0.05f)
                : card.Rarity == Rarity.Rare ? new Color(0.15f, 0.38f, 0.75f)
                : new Color(ink.r, ink.g, ink.b, 0.6f);
            GUI.Label(top, card == CardDatabase.TwoMoons ? "Legende" : CardDatabase.RarityName(card.Rarity), rarityStyle);
            string number = card == CardDatabase.TwoMoons ? "1/1" : $"{card.Number:00}/{CardDatabase.CardsPerGame}";
            GUI.Label(top, number, numberStyle);

            // Trennlinie
            GUI.color = new Color(ink.r, ink.g, ink.b, 0.2f);
            GUI.DrawTexture(new Rect(field.x + pixel, top.yMax + pixel * 0.3f, field.width - pixel * 2f, Mathf.Max(1f, pixel * 0.15f)), white);
            GUI.color = Color.white;

            // Name: so groß wie möglich, aber immer ganz lesbar
            var nameRect = new Rect(field.x + pixel * 0.5f, top.yMax + pixel * 0.5f, field.width - pixel, field.yMax - top.yMax - pixel * 0.7f);
            content.text = card.Name;
            int size = Mathf.Max(7, Mathf.RoundToInt(pixel * 2.4f));
            nameStyle.fontSize = size;
            while (size > 7 && nameStyle.CalcSize(content).x > nameRect.width)
            {
                size--;
                nameStyle.fontSize = size;
            }
            GUI.Label(nameRect, content, nameStyle);
        }

        /// <summary>Versiegelte Karte in der Schutzhülle („Slab“) mit Notenschild oben.</summary>
        public static void DrawSlab(Rect rect, CardDef card, int grade)
        {
            EnsureTextures();
            bool blackLabel = grade >= Grading.BlackLabel;
            // Black Label: schwarze Hülle mit goldenem Rand statt der hellen.
            if (blackLabel)
            {
                GUI.color = UiKit.Gold;
                GUI.DrawTexture(rect, white);
                GUI.color = new Color(0.07f, 0.07f, 0.08f, 1f);
                GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), white);
            }
            else
            {
                GUI.color = new Color(0.78f, 0.86f, 0.95f, 0.95f);
                GUI.DrawTexture(rect, white);
            }
            float label = rect.height * 0.18f;
            GUI.color = blackLabel ? new Color(0.14f, 0.14f, 0.15f) : grade >= 10 ? UiKit.Gold : grade >= 8 ? new Color(0.85f, 0.2f, 0.2f) : new Color(0.35f, 0.4f, 0.5f);
            var labelRect = new Rect(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.03f, rect.width * 0.88f, label);
            GUI.DrawTexture(labelRect, white);
            GUI.color = Color.white;
            var style = new GUIStyle(UiKit.Bold) { alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Overflow };
            style.normal.textColor = blackLabel ? UiKit.Gold : Color.white;
            // So groß wie möglich, aber immer ganz im Schild.
            var text = new GUIContent(Grading.Label(grade).ToUpperInvariant());
            int size = Mathf.Max(8, Mathf.RoundToInt(label * 0.7f));
            style.fontSize = size;
            while (size > 6 && style.CalcSize(text).x > labelRect.width - 4f) style.fontSize = --size;
            GUI.Label(labelRect, text, style);

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
