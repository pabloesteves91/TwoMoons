using System;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Die Geschichte vor dem ersten Tag: ein Kind träumt von einem Laden, Papa erzählt die Legende der zwei Monde,
    /// und zum 18. Geburtstag bekommt es den alten, leeren Laden geschenkt.
    /// Nur zum Durchklicken (Klick / Leertaste), [Esc] überspringt.
    /// </summary>
    public class IntroCutscene : MonoBehaviour
    {
        const float FadeSeconds = 0.6f;
        const float CharsPerSecond = 45f;

        class Scene
        {
            public Action<IntroCutscene, float> Draw;
            /// <summary>Sprecher (null = Erzähler) und Text. {0} wird durch den Namen ersetzt.</summary>
            public (string speaker, string text)[] Lines;
        }

        const string Papa = "Papa";
        const string Kid = "{0}";

        static readonly Scene[] Scenes =
        {
            new Scene
            {
                Draw = (c, t) => c.DrawBedroom(t, thought: true, father: false, doorOpen: 0f, dark: 0f),
                Lines = new[]
                {
                    ((string)null, "Es ist spät in der Nacht. Vor dem Fenster gehen zwei Monde auf – ein roter und ein goldener."),
                    (null, "{0} kann nicht schlafen. Wie fast jede Nacht träumt {0} von einem eigenen Laden …"),
                    (null, "… mit Regalen voller Sammelkarten und Kunden, die strahlend mit ihren Packs nach Hause gehen."),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawBedroom(t + 20f, thought: false, father: true, doorOpen: Mathf.Clamp01(t / 1.2f), dark: 0f, walkTime: t - 1f),
                Lines = new[]
                {
                    ((string)null, "Da geht leise die Tür auf."),
                    (Papa, "Na, mein Schatz? Wieder zu aufgeregt zum Schlafen?"),
                    (Kid, "Papa … erzählst du mir noch einmal die Geschichte von den zwei Monden?"),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawBedroom(t + 40f, thought: false, father: true, doorOpen: 1f, dark: c.IsLastLine ? Mathf.Clamp01(c.LineTime / 3f) * 0.6f : 0f, walkTime: 99f),
                Lines = new[]
                {
                    (Papa, "Es heißt, in den Nächten, in denen beide Monde zugleich am Himmel stehen, wurde eine ganz besondere Karte gedruckt."),
                    (Papa, "Man nennt sie „Two Moons“. Niemand weiß, wo sie heute ist."),
                    (Papa, "Aber man sagt, sie findet ihren Weg zu dem, der seinen Traum nie aufgibt."),
                    (Kid, "Ich geb meinen Traum nie auf, Papa. Versprochen."),
                    (Papa, "Das weiß ich. Und jetzt schlaf. Träume brauchen Kraft."),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawYearsPass(t),
                Lines = new[]
                {
                    ((string)null, "Die Jahre vergehen. Viele Monde ziehen über den Himmel …"),
                    (null, "… aber der Traum bleibt."),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawKitchen(t),
                Lines = new[]
                {
                    ((string)null, "Am Morgen des 18. Geburtstags."),
                    (Papa, "Alles Gute, {0}! Ich hab da etwas für dich."),
                    (null, "Ein alter Schlüssel. Am Anhänger baumeln zwei kleine Monde – ein roter und ein goldener."),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawShopFront(t),
                Lines = new[]
                {
                    (Papa, "Das hier war einmal mein Laden. Als du klein warst, musste ich ihn schließen."),
                    (Papa, "Seitdem stand er leer. Verkauft habe ich ihn nie – ich habe auf dich gewartet."),
                    (Papa, "Ab heute gehört er dir."),
                },
            },
            new Scene
            {
                Draw = (c, t) => c.DrawShopEmpty(t),
                Lines = new[]
                {
                    ((string)null, "Drinnen: Staub, Spinnweben und ein schiefes, altes Regal. Sonst nichts."),
                    (Papa, "Er ist leer, ich weiß. Einen Karton Monstera-Packs habe ich dir noch besorgt. Den Rest schaffst du."),
                    (Kid, "Er ist perfekt, Papa."),
                    (Kid, "Und ich weiß auch schon, wie er heißen wird … TwoMoons."),
                },
            },
        };

        int sceneIndex;
        int lineIndex;
        float sceneStart;
        float lineStart;
        float fadeOutStart = -1f;
        bool finished;
        bool active;

        Rect canvas;
        float pixel;
        Texture2D white;

        static GameManager Game => GameManager.Instance;

        bool IsLastLine => lineIndex == Scenes[sceneIndex].Lines.Length - 1;
        float LineTime => Time.unscaledTime - lineStart;

        void Restart()
        {
            sceneIndex = 0;
            lineIndex = 0;
            sceneStart = lineStart = Time.unscaledTime;
            fadeOutStart = -1f;
            finished = false;
        }

        void Update()
        {
            if (Game.Phase != GamePhase.Intro)
            {
                active = false;
                return;
            }
            if (!active)
            {
                active = true;
                Restart();
            }
            if (finished) return;

            if (GameInput.ConsumeCancel())
            {
                Finish();
                return;
            }

            if (fadeOutStart >= 0f)
            {
                if (Time.unscaledTime - fadeOutStart < FadeSeconds) return;
                fadeOutStart = -1f;
                sceneIndex++;
                if (sceneIndex >= Scenes.Length)
                {
                    Finish();
                    return;
                }
                lineIndex = 0;
                sceneStart = lineStart = Time.unscaledTime;
                return;
            }

            if (!GameInput.ConfirmPressed) return;

            // Erster Klick: Text sofort ganz zeigen. Zweiter Klick: weiter.
            if (VisibleChars < CurrentText.Length)
            {
                lineStart -= 1000f;
                return;
            }

            SoundManager.Play(Sfx.Dialog);
            if (lineIndex < Scenes[sceneIndex].Lines.Length - 1)
            {
                lineIndex++;
                lineStart = Time.unscaledTime;
            }
            else
            {
                fadeOutStart = Time.unscaledTime;
            }
        }

        void Finish()
        {
            finished = true;
            Game.FinishIntro();
        }

        string CurrentText => string.Format(Scenes[sceneIndex].Lines[lineIndex].text, Game.PlayerName);

        int VisibleChars => Mathf.FloorToInt(LineTime * CharsPerSecond);

        void OnGUI()
        {
            if (Game.Phase != GamePhase.Intro || finished) return;

            float width = UiKit.Begin();
            EnsureWhite();

            // Schwarzer Hintergrund, Szene im 16:9-Format darüber.
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, 0f, width, UiKit.VirtualHeight), white);
            GUI.color = Color.white;

            pixel = Mathf.Min(width / IntroArt.Width, UiKit.VirtualHeight / IntroArt.Height);
            float w = IntroArt.Width * pixel, h = IntroArt.Height * pixel;
            canvas = new Rect((width - w) / 2f, (UiKit.VirtualHeight - h) / 2f, w, h);

            GUI.BeginGroup(canvas);
            canvas = new Rect(0f, 0f, w, h);
            Scenes[sceneIndex].Draw(this, Time.unscaledTime - sceneStart);
            GUI.EndGroup();
            canvas = new Rect((width - w) / 2f, (UiKit.VirtualHeight - h) / 2f, w, h);

            // Ein- und Ausblenden
            float fade = Mathf.Clamp01(1f - (Time.unscaledTime - sceneStart) / FadeSeconds);
            if (fadeOutStart >= 0f) fade = Mathf.Max(fade, Mathf.Clamp01((Time.unscaledTime - fadeOutStart) / FadeSeconds));
            if (fade > 0f)
            {
                GUI.color = new Color(0f, 0f, 0f, fade);
                GUI.DrawTexture(new Rect(0f, 0f, width, UiKit.VirtualHeight), white);
                GUI.color = Color.white;
            }

            DrawTextBox(width);

            if (GUI.Button(new Rect(width - 150f, 12f, 138f, 30f), "Überspringen [Esc]", UiKit.ButtonStyle))
            {
                SoundManager.Play(Sfx.UiClick);
                Finish();
            }
        }

        void DrawTextBox(float width)
        {
            if (fadeOutStart >= 0f) return;
            var (speaker, _) = Scenes[sceneIndex].Lines[lineIndex];
            string text = CurrentText;
            string visible = text.Substring(0, Mathf.Clamp(VisibleChars, 0, text.Length));

            var area = new Rect((width - 760f) / 2f, UiKit.VirtualHeight - 128f, 760f, 112f);
            GUI.Box(area, GUIContent.none, UiKit.Panel);
            float y = area.y + 10f;
            if (speaker != null)
            {
                GUI.Label(new Rect(area.x + 18f, y, area.width - 36f, 22f), string.Format(speaker, Game.PlayerName), UiKit.Title.WithFontSize(17));
                y += 24f;
            }
            var style = speaker == null ? UiKit.Text.WithItalic() : UiKit.Text;
            GUI.Label(new Rect(area.x + 18f, y, area.width - 36f, area.yMax - y - 22f), visible, style.WithFontSize(16));
            if (visible.Length == text.Length)
                GUI.Label(new Rect(area.x, area.yMax - 22f, area.width - 14f, 18f), "Weiter: Klick / [Leertaste]", UiKit.Hint.WithAlignment(TextAnchor.MiddleRight));
        }

        // ---------- Zeichnen auf der 160×90-Leinwand (x/y in Pixeln, y = 0 unten) ----------

        Rect ToScreen(float x, float y, float w, float h) =>
            new Rect(canvas.x + x * pixel, canvas.y + (IntroArt.Height - y - h) * pixel, w * pixel, h * pixel);

        void Draw(Texture2D texture, float x, float y, float alpha = 1f)
        {
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(ToScreen(x, y, texture.width, texture.height), texture);
            GUI.color = Color.white;
        }

        void Fill(float x, float y, float w, float h, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(ToScreen(x, y, w, h), white);
            GUI.color = Color.white;
        }

        Sprite PlayerSprite => Art.Get($"player{Game.Figure + 1}");

        /// <summary>Die Spielfigur, <paramref name="size"/> Pixel groß, mit den Füßen bei (x, y).</summary>
        void DrawPlayer(float x, float y, float size, bool flip = false)
        {
            var rect = ToScreen(x, y, size, size);
            if (flip) rect = new Rect(rect.xMax, rect.y, -rect.width, rect.height);
            UiKit.DrawSprite(PlayerSprite, rect);
        }

        /// <summary>Nur den Kopf der Figur zeichnen (obere 7 von 16 Pixeln) – für das Kind im Bett.</summary>
        void DrawPlayerHead(float x, float y)
        {
            var sprite = PlayerSprite;
            var texture = sprite.texture;
            var r = sprite.textureRect;
            float headRows = r.height * 7f / 16f;
            var uv = new Rect(r.x / texture.width, (r.yMax - headRows) / texture.height, r.width / texture.width, headRows / texture.height);
            GUI.DrawTextureWithTexCoords(ToScreen(x, y, 16f, 7f), texture, uv);
        }

        void DrawBedroom(float t, bool thought, bool father, float doorOpen, float dark, float walkTime = 0f)
        {
            // Himmel hinter dem Fenster mit aufgehenden Monden (Fensteröffnung: x 92–136, y 40–74).
            var sky = IntroArt.Get("nightSky");
            GUI.BeginGroup(ToScreen(92f, 40f, 44f, 34f));
            var saved = canvas;
            canvas = new Rect(-92f * pixel, -(IntroArt.Height - 74f) * pixel, saved.width, saved.height);
            Draw(sky, 92f - 30f, 40f - 20f);
            DrawTwinkle(t, 92f, 44f, 44f, 30f);
            Draw(IntroArt.Get("moonRed"), 98f, 44f + Mathf.Min(t, 30f) * 0.5f);
            Draw(IntroArt.Get("moonGold"), 116f, 40f + Mathf.Min(t, 30f) * 0.55f);
            canvas = saved;
            GUI.EndGroup();

            Draw(IntroArt.Get("bedroom"), 0f, 0f);

            // Mondlicht auf dem Boden
            Fill(80f, 0f, 60f, 20f, new Color(0.7f, 0.6f, 1f, 0.08f));

            // Kind im Bett
            DrawPlayerHead(12f, 31f + Mathf.Sin(t * 1.5f) * 0.3f);

            if (thought)
            {
                float bob = Mathf.Sin(t * 1.4f) * 1.5f;
                float alpha = Mathf.Clamp01((t - 1.5f) / 1.5f);
                Draw(IntroArt.Get("thought"), 22f, 40f + bob, alpha);
            }

            // Tür: geht auf, warmes Licht fällt herein
            if (doorOpen > 0f)
            {
                Fill(142f, 18f, 18f, 46f, new Color(1f, 0.85f, 0.5f, 0.9f * doorOpen));
                Fill(110f, 0f, 50f, 18f, new Color(1f, 0.85f, 0.5f, 0.18f * doorOpen));
            }
            var door = IntroArt.Get("door");
            float doorWidth = Mathf.Lerp(16f, 3f, doorOpen);
            GUI.DrawTexture(ToScreen(143f, 19f, doorWidth, 44f), door);

            if (father)
            {
                // Papa läuft von der Tür ans Bett.
                float progress = Mathf.Clamp01(walkTime / 3f);
                float x = Mathf.Lerp(144f, 62f, progress);
                float bob = progress < 1f ? Mathf.Abs(Mathf.Sin(walkTime * 8f)) * 1.2f : 0f;
                var tex = IntroArt.Get("father");
                var rect = ToScreen(x, 14f + bob, tex.width, tex.height);
                if (progress < 1f) rect = new Rect(rect.xMax, rect.y, -rect.width, rect.height);
                GUI.DrawTexture(rect, tex);
            }

            if (dark > 0f) Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(0.02f, 0.02f, 0.08f, dark));
        }

        void DrawTwinkle(float t, float x, float y, float w, float h)
        {
            for (int i = 0; i < 8; i++)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(t * (1.3f + i * 0.4f) + i);
                Fill(x + Mathf.Repeat(i * 17.3f, w), y + Mathf.Repeat(i * 9.7f, h), 1f, 1f, new Color(1f, 1f, 0.85f, alpha));
            }
        }

        void DrawYearsPass(float t)
        {
            Draw(IntroArt.Get("nightSky"), 0f, 0f);
            DrawTwinkle(t, 0f, 20f, IntroArt.Width, 70f);
            // Monde ziehen immer wieder im Bogen über den Himmel.
            for (int i = 0; i < 2; i++)
            {
                float phase = Mathf.Repeat(t * 0.35f + i * 0.5f, 1f);
                float x = Mathf.Lerp(-14f, IntroArt.Width, phase);
                float y = 30f + Mathf.Sin(phase * Mathf.PI) * 45f;
                Draw(IntroArt.Get(i == 0 ? "moonRed" : "moonGold"), x, y);
            }
            // Die Figur wächst heran.
            float size = Mathf.Lerp(12f, 22f, Mathf.Clamp01(t / 6f));
            DrawPlayer(80f - size / 2f, 2f, size);
        }

        void DrawKitchen(float t)
        {
            Draw(IntroArt.Get("kitchen"), 0f, 0f);
            DrawPlayer(30f, 10f, 22f);
            Draw(IntroArt.Get("father"), 124f, 10f);
            Draw(IntroArt.Get("cake"), 75f, 30f);
            // Kerzenflammen flackern
            for (int i = 0; i < 3; i++)
            {
                float flicker = Mathf.PerlinNoise(t * 6f, i * 3.1f);
                Fill(79f + i * 5f, 46f, 1f, 2f + flicker, new Color(1f, 0.75f + flicker * 0.25f, 0.2f, 1f));
            }
            // Ab dem zweiten Satz hält Papa den Schlüssel hoch, dann schwebt er zum Geburtstagskind.
            if (lineIndex >= 1)
            {
                float k = lineIndex >= 2 ? Mathf.Clamp01(LineTime / 1.5f) : 0f;
                float x = Mathf.Lerp(120f, 76f, k);
                float y = Mathf.Lerp(30f, 58f, k) + Mathf.Sin(t * 2f) * 1f;
                Draw(IntroArt.Get("key"), x, y);
                if (k >= 1f)
                {
                    float sparkle = Mathf.Abs(Mathf.Sin(t * 4f));
                    Fill(x - 2f, y + 8f, 1f, 1f, new Color(1f, 1f, 0.8f, sparkle));
                    Fill(x + 14f, y + 2f, 1f, 1f, new Color(1f, 1f, 0.8f, 1f - sparkle));
                }
            }
        }

        void DrawShopFront(float t)
        {
            Draw(IntroArt.Get("shopFront"), 0f, 0f);
            float progress = Mathf.Clamp01(t / 3f);
            float bob = progress < 1f ? Mathf.Abs(Mathf.Sin(t * 8f)) : 0f;
            Draw(IntroArt.Get("father"), Mathf.Lerp(-20f, 118f, progress), 6f + bob);
            DrawPlayer(Mathf.Lerp(-40f, 96f, progress), 6f + bob, 22f, flip: progress >= 1f);
            // Ein Vogel fliegt vorbei.
            float bird = Mathf.Repeat(t * 25f, 260f) - 50f;
            Fill(bird, 82f + Mathf.Sin(t * 3f) * 2f, 3f, 1f, new Color(0.2f, 0.2f, 0.25f));
            Fill(bird + 1f, 83f + Mathf.Sin(t * 3f) * 2f, 1f, 1f, new Color(0.2f, 0.2f, 0.25f));
        }

        void DrawShopEmpty(float t)
        {
            Draw(IntroArt.Get("shopEmpty"), 0f, 0f);
            // Lichtkegel durchs Fenster und tanzender Staub
            Fill(62f, 0f, 30f, 72f, new Color(1f, 0.95f, 0.75f, 0.07f));
            for (int i = 0; i < 14; i++)
            {
                float x = 62f + Mathf.Repeat(i * 7.3f + Mathf.Sin(t * 0.7f + i) * 3f, 30f);
                float y = Mathf.Repeat(i * 11f + t * (2f + i % 3), 70f);
                Fill(x, y, 1f, 1f, new Color(1f, 0.95f, 0.8f, 0.6f));
            }
            Draw(IntroArt.Get("father"), 30f, 4f);
            DrawPlayer(48f, 4f, 22f);

            // Beim letzten Satz leuchtet der Name über allem auf.
            if (IsLastLine)
            {
                float a = Mathf.Clamp01(LineTime / 2f);
                Draw(IntroArt.Get("moonRed"), 66f, 76f, a);
                Draw(IntroArt.Get("moonGold"), 80f, 74f, a);
            }
        }

        void EnsureWhite()
        {
            if (white != null) return;
            white = new Texture2D(1, 1, TextureFormat.RGBA32, false, false) { hideFlags = HideFlags.HideAndDontSave };
            white.SetPixel(0, 0, Color.white);
            white.Apply();
        }
    }
}
