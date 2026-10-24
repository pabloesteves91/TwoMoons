using System;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Spielt Story-Szenen ab (Intro, Papa-Momente) und die Titelkarte „Kapitel X“ vor jedem Kapitel.
    /// Szenen werden durchgeklickt (Klick / Leertaste), [Esc] überspringt.
    /// </summary>
    public class CutscenePlayer : MonoBehaviour
    {
        const float FadeSeconds = 0.6f;
        const float CharsPerSecond = 45f;
        const float TitleCardSeconds = 4.5f;

        Cutscene cutscene;
        Action onDone;
        int titleCardChapter;

        int sceneIndex;
        int lineIndex;
        float sceneStart;
        float lineStart;
        float fadeOutStart = -1f;

        Rect canvas;
        float pixel;
        Texture2D white;

        static GameManager Game => GameManager.Instance;

        public bool IsPlaying => cutscene != null || titleCardChapter > 0;

        internal bool IsLastLine => lineIndex == cutscene.Scenes[sceneIndex].Lines.Length - 1;
        internal float LineTime => Time.unscaledTime - lineStart;
        internal int LineIndex => lineIndex;

        public void Play(Cutscene scenes, Action done)
        {
            cutscene = scenes;
            titleCardChapter = 0;
            Begin(done);
        }

        /// <summary>Die immer gleiche Animation zum Start eines Kapitels.</summary>
        public void PlayTitleCard(int chapter, Action done)
        {
            cutscene = null;
            titleCardChapter = chapter;
            Begin(done);
            SoundManager.Play(Sfx.Quest);
        }

        void Begin(Action done)
        {
            onDone = done;
            sceneIndex = 0;
            lineIndex = 0;
            sceneStart = lineStart = Time.unscaledTime;
            fadeOutStart = -1f;
        }

        void Update()
        {
            if (!IsPlaying) return;

            if (GameInput.ConsumeCancel())
            {
                Finish();
                return;
            }

            if (titleCardChapter > 0)
            {
                bool over = Time.unscaledTime - sceneStart > TitleCardSeconds;
                if (over || (GameInput.ConfirmPressed && Time.unscaledTime - sceneStart > 1f)) Finish();
                return;
            }

            if (fadeOutStart >= 0f)
            {
                if (Time.unscaledTime - fadeOutStart < FadeSeconds) return;
                fadeOutStart = -1f;
                sceneIndex++;
                if (sceneIndex >= cutscene.Scenes.Length)
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
            if (lineIndex < cutscene.Scenes[sceneIndex].Lines.Length - 1)
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
            var done = onDone;
            cutscene = null;
            titleCardChapter = 0;
            onDone = null;
            done?.Invoke();
        }

        string CurrentText => string.Format(cutscene.Scenes[sceneIndex].Lines[lineIndex].text, Game.PlayerName);

        int VisibleChars => Mathf.FloorToInt(LineTime * CharsPerSecond);

        void OnGUI()
        {
            if (!IsPlaying) return;

            float width = UiKit.Begin();
            EnsureWhite();

            // Schwarzer Hintergrund, Szene im 16:9-Format darüber.
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, 0f, width, UiKit.VirtualHeight), white);
            GUI.color = Color.white;

            pixel = Mathf.Min(width / IntroArt.Width, UiKit.VirtualHeight / IntroArt.Height);
            float w = IntroArt.Width * pixel, h = IntroArt.Height * pixel;
            var outer = new Rect((width - w) / 2f, (UiKit.VirtualHeight - h) / 2f, w, h);

            GUI.BeginGroup(outer);
            canvas = new Rect(0f, 0f, w, h);
            float t = Time.unscaledTime - sceneStart;
            if (titleCardChapter > 0) DrawTitleCard(t);
            else cutscene.Scenes[sceneIndex].Draw(this, t);
            GUI.EndGroup();
            canvas = outer;

            // Ein- und Ausblenden
            float fade = Mathf.Clamp01(1f - t / FadeSeconds);
            if (titleCardChapter > 0) fade = Mathf.Max(fade, Mathf.Clamp01((t - (TitleCardSeconds - FadeSeconds)) / FadeSeconds));
            if (fadeOutStart >= 0f) fade = Mathf.Max(fade, Mathf.Clamp01((Time.unscaledTime - fadeOutStart) / FadeSeconds));
            if (fade > 0f)
            {
                GUI.color = new Color(0f, 0f, 0f, fade);
                GUI.DrawTexture(new Rect(0f, 0f, width, UiKit.VirtualHeight), white);
                GUI.color = Color.white;
            }

            if (titleCardChapter > 0)
            {
                DrawTitleText(width, t);
                return;
            }

            DrawTextBox(width);

            if (GUI.Button(new Rect(width - 222f, 12f, 210f, 32f), "Überspringen [Esc]", UiKit.ButtonStyle))
            {
                SoundManager.Play(Sfx.UiClick);
                Finish();
            }
        }

        void DrawTextBox(float width)
        {
            if (fadeOutStart >= 0f) return;
            var (speaker, _) = cutscene.Scenes[sceneIndex].Lines[lineIndex];
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

        // ---------- Titelkarte „Kapitel X“ ----------

        /// <summary>Nachthimmel, die zwei Monde schweben von links und rechts herein.</summary>
        void DrawTitleCard(float t)
        {
            Draw(IntroArt.Get("nightSky"), 0f, 0f);
            DrawTwinkle(t, 0f, 20f, IntroArt.Width, 70f);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 1.6f));
            Draw(IntroArt.Get("moonRed"), Mathf.Lerp(-20f, 64f, k), 62f + Mathf.Sin(t * 1.2f));
            Draw(IntroArt.Get("moonGold"), Mathf.Lerp(IntroArt.Width + 6f, 82f, k), 60f + Mathf.Sin(t * 1.2f + 1f));
        }

        void DrawTitleText(float width, float t)
        {
            float alpha = Mathf.Clamp01((t - 0.9f) / 0.8f) * (1f - Mathf.Clamp01((t - (TitleCardSeconds - FadeSeconds)) / FadeSeconds));
            float grow = Mathf.Lerp(0.85f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.9f) / 0.8f)));
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(0f, 250f, width, 70f), $"Kapitel {titleCardChapter}", UiKit.BigTitle.WithFontSize(Mathf.RoundToInt(56 * grow)));
            string title = titleCardChapter < Cutscenes.ChapterTitles.Length ? Cutscenes.ChapterTitles[titleCardChapter] : "";
            GUI.Label(new Rect(0f, 320f, width, 40f), title, UiKit.Title.WithAlignment(TextAnchor.MiddleCenter).WithFontSize(26));
            GUI.color = Color.white;
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
        internal void DrawPlayer(float x, float y, float size, bool flip = false)
        {
            var rect = ToScreen(x, y, size, size);
            if (flip) rect = new Rect(rect.xMax, rect.y, -rect.width, rect.height);
            UiKit.DrawSprite(PlayerSprite, rect);
        }

        /// <summary>Nur den Kopf der Figur zeichnen (obere 7 von 16 Pixeln) – für das Kind im Bett.</summary>
        internal void DrawPlayerHead(float x, float y)
        {
            var sprite = PlayerSprite;
            var texture = sprite.texture;
            var r = sprite.textureRect;
            float headRows = r.height * 7f / 16f;
            var uv = new Rect(r.x / texture.width, (r.yMax - headRows) / texture.height, r.width / texture.width, headRows / texture.height);
            GUI.DrawTextureWithTexCoords(ToScreen(x, y, 16f, 7f), texture, uv);
        }

        internal void DrawBedroom(float t, bool thought, bool father, float doorOpen, float dark, float walkTime = 0f)
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

        internal void DrawTwinkle(float t, float x, float y, float w, float h)
        {
            for (int i = 0; i < 8; i++)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(t * (1.3f + i * 0.4f) + i);
                Fill(x + Mathf.Repeat(i * 17.3f, w), y + Mathf.Repeat(i * 9.7f, h), 1f, 1f, new Color(1f, 1f, 0.85f, alpha));
            }
        }

        internal void DrawYearsPass(float t)
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

        internal void DrawKitchen(float t)
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
            if (LineIndex >= 1)
            {
                float k = LineIndex >= 2 ? Mathf.Clamp01(LineTime / 1.5f) : 0f;
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

        internal void DrawShopFront(float t)
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

        internal void DrawShopEmpty(float t)
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

        /// <summary>Der Laden nachts: dunkel, nur die Lampe über der Kasse. Optional mit Papa am Telefon.</summary>
        internal void DrawShopNight(float t, bool phoneCall)
        {
            Draw(IntroArt.Get("shopEmpty"), 0f, 0f);
            Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(0.03f, 0.03f, 0.1f, 0.7f));

            // Lampe über der Theke, leicht flackernd
            float flicker = 0.9f + 0.1f * Mathf.PerlinNoise(t * 3f, 0f);
            Fill(8f, 30f, 44f, 40f, new Color(1f, 0.8f, 0.45f, 0.12f * flicker));
            Fill(16f, 44f, 28f, 26f, new Color(1f, 0.8f, 0.45f, 0.12f * flicker));
            Fill(28f, 74f, 4f, 3f, new Color(1f, 0.9f, 0.6f, flicker));
            Fill(29f, 77f, 2f, 13f, new Color(0.15f, 0.12f, 0.12f));

            DrawPlayer(22f, 30f, 20f);
            // Handy am Ohr bzw. in der Hand
            float buzz = phoneCall ? 0f : Mathf.Sin(t * 30f) * 0.4f * (LineIndex == 0 ? 1f : 0f);
            Fill(37f + buzz, 41f, 2f, 3f, new Color(0.2f, 0.2f, 0.25f));
            Fill(37.5f + buzz, 42f, 1f, 1.5f, new Color(0.5f, 0.8f, 1f));

            if (!phoneCall || LineIndex < 2) return;

            // Papa am anderen Ende: kleines Bild rechts, in seinem Wohnzimmer im Sessel
            float appear = Mathf.Clamp01(LineTime / 0.5f);
            if (LineIndex > 2) appear = 1f;
            Fill(96f, 34f, 56f, 46f, new Color(0.1f, 0.07f, 0.06f, appear));
            Fill(98f, 36f, 52f, 42f, new Color(0.35f, 0.25f, 0.22f, appear));
            Fill(98f, 36f, 52f, 8f, new Color(0.45f, 0.32f, 0.22f, appear));
            Fill(104f, 44f, 22f, 14f, new Color(0.45f, 0.18f, 0.16f, appear));
            Fill(136f, 44f, 6f, 20f, new Color(0.95f, 0.85f, 0.6f, 0.35f * appear));
            var father = IntroArt.Get("father");
            float cough = LineIndex == 5 ? Mathf.Abs(Mathf.Sin(LineTime * 9f)) * Mathf.Clamp01(2f - LineTime) : 0f;
            GUI.color = new Color(1f, 1f, 1f, appear);
            GUI.DrawTexture(ToScreen(107f, 44f - cough, father.width, father.height), father);
            GUI.color = Color.white;
        }

        /// <summary>Die Ladenfront bei Nacht: Zum ersten Mal brennt wieder Licht, über der Tür steht „TwoMoons“.</summary>
        internal void DrawShopFrontNight(float t, bool papaClose)
        {
            Draw(IntroArt.Get("shopFront"), 0f, 0f);
            Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(0.04f, 0.04f, 0.14f, 0.72f));
            DrawTwinkle(t, 0f, 70f, IntroArt.Width, 20f);

            // Warmes Licht im Schaufenster und aus der Tür
            float glow = 0.85f + 0.15f * Mathf.Sin(t * 1.5f);
            Fill(36f, 22f, 44f, 34f, new Color(1f, 0.8f, 0.45f, 0.85f * glow));
            Fill(40f, 26f, 36f, 2f, new Color(0.45f, 0.3f, 0.2f));
            Fill(40f, 36f, 36f, 2f, new Color(0.45f, 0.3f, 0.2f));
            Fill(36f, 10f, 44f, 12f, new Color(1f, 0.8f, 0.45f, 0.15f * glow));
            Fill(97f, 18f, 12f, 30f, new Color(1f, 0.8f, 0.45f, 0.35f * glow));

            // Schild mit dem Namen
            Fill(50f, 62f, 60f, 10f, new Color(0.18f, 0.1f, 0.08f));
            var sign = ToScreen(50f, 62f, 60f, 10f);
            GUI.Label(sign, "TwoMoons", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter).WithFontSize(Mathf.RoundToInt(pixel * 6f)));

            // Papa auf der anderen Straßenseite (vorne im Bild)
            var father = IntroArt.Get("father");
            if (papaClose)
            {
                float walk = Mathf.Clamp01((t - 3f) / 6f);
                float x = Mathf.Lerp(124f, 175f, walk);
                float bob = walk > 0f && walk < 1f ? Mathf.Abs(Mathf.Sin(t * 6f)) : 0f;
                var rect = ToScreen(x, -6f + bob, father.width * 1.6f, father.height * 1.6f);
                if (walk > 0f) rect = new Rect(rect.xMax, rect.y, -rect.width, rect.height);
                GUI.color = new Color(0.55f, 0.55f, 0.7f, 1f);
                GUI.DrawTexture(rect, father);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.45f, 0.45f, 0.6f, 1f);
                GUI.DrawTexture(ToScreen(130f, -2f, father.width * 1.3f, father.height * 1.3f), father);
                GUI.color = Color.white;
            }
        }

        /// <summary>Kleine Figur aus dem Spiel (Kunde, Papa …) auf der Leinwand.</summary>
        void DrawFigure(string sprite, float x, float y, float size, bool flip = false, float alpha = 1f)
        {
            var rect = ToScreen(x, y, size, size);
            if (flip) rect = new Rect(rect.xMax, rect.y, -rect.width, rect.height);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            UiKit.DrawSprite(Art.Get(sprite), rect);
            GUI.color = Color.white;
        }

        /// <summary>Ein Brief auf dem Tresen, im Licht der Lampe. Optional mit den zwei gezeichneten Monden.</summary>
        internal void DrawLetter(float t, bool moons)
        {
            DrawShopNight(t, phoneCall: false);
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 1.2f));
            float y = Mathf.Lerp(-40f, 14f, rise);
            Fill(54f, y - 1f, 54f, 64f, new Color(0.3f, 0.22f, 0.15f));
            Fill(55f, y, 52f, 62f, new Color(0.96f, 0.92f, 0.8f));
            for (int i = 0; i < 7; i++) Fill(60f, y + 50f - i * 6f, 30f + (i * 7) % 12, 1f, new Color(0.35f, 0.3f, 0.35f));
            if (moons)
            {
                Draw(IntroArt.Get("moonRed"), 80f, y + 8f, rise);
                Draw(IntroArt.Get("moonGold"), 92f, y + 12f, rise);
            }
        }

        /// <summary>Der volle Laden am Tag. Mit <paramref name="papaAtRegister"/> steht Papa an der Kasse.</summary>
        internal void DrawShopBusy(float t, bool papaAtRegister)
        {
            Draw(IntroArt.Get("shopEmpty"), 0f, 0f);
            Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(1f, 0.9f, 0.7f, 0.12f));
            // Regale voller Packs
            for (int i = 0; i < 3; i++)
            {
                float x = 70f + i * 28f;
                Fill(x, 20f, 20f, 34f, new Color(0.45f, 0.3f, 0.2f));
                for (int row = 0; row < 3; row++)
                for (int p = 0; p < 4; p++)
                    Fill(x + 2f + p * 4.5f, 24f + row * 10f, 3f, 6f, Color.HSVToRGB(Mathf.Repeat(i * 0.3f + p * 0.13f, 1f), 0.6f, 0.9f));
            }
            // Kunden laufen hin und her
            for (int i = 0; i < 4; i++)
            {
                float phase = Mathf.Repeat(t * 0.08f + i * 0.27f, 1f);
                float x = Mathf.Lerp(150f, 50f, phase);
                float bob = Mathf.Abs(Mathf.Sin(t * 7f + i)) * 0.8f;
                DrawFigure($"customer{i % 3 + 1}", x, 4f + bob, 18f, flip: true);
            }
            // Theke mit Kasse
            Fill(6f, 4f, 40f, 16f, new Color(0.4f, 0.26f, 0.18f));
            Fill(6f, 18f, 40f, 3f, new Color(0.55f, 0.38f, 0.25f));
            Fill(30f, 21f, 9f, 6f, new Color(0.25f, 0.25f, 0.3f));
            if (papaAtRegister)
            {
                float wave = LineIndex >= 1 ? Mathf.Sin(t * 5f) * 0.6f : 0f;
                Draw(IntroArt.Get("father"), 14f, 14f + Mathf.Abs(wave));
            }
            DrawPlayer(48f, 4f, 20f, flip: true);
        }

        /// <summary>Das Stadtturnier: volle Tische, Banner, Papa in der ersten Reihe.</summary>
        internal void DrawCityTournament(float t)
        {
            Draw(IntroArt.Get("shopEmpty"), 0f, 0f);
            Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(1f, 0.85f, 0.6f, 0.15f));
            // Banner
            Fill(30f, 70f, 100f, 12f, new Color(0.55f, 0.12f, 0.1f));
            Fill(31f, 71f, 98f, 10f, new Color(0.75f, 0.2f, 0.15f));
            GUI.Label(ToScreen(30f, 70f, 100f, 12f), "STADTTURNIER", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter).WithFontSize(Mathf.RoundToInt(pixel * 7f)));
            // Wimpelkette
            for (int i = 0; i < 16; i++)
                Fill(4f + i * 10f, 86f - (i % 2), 4f, 3f, Color.HSVToRGB(Mathf.Repeat(i * 0.17f, 1f), 0.6f, 1f));
            // Spieltische mit Spielern
            for (int table = 0; table < 2; table++)
            {
                float x = 34f + table * 56f;
                Fill(x, 24f, 36f, 10f, new Color(0.2f, 0.45f, 0.3f));
                Fill(x, 22f, 36f, 2f, new Color(0.35f, 0.22f, 0.15f));
                for (int seat = 0; seat < 3; seat++)
                {
                    float bob = Mathf.Abs(Mathf.Sin(t * 3f + seat + table * 2f)) * 0.6f;
                    DrawFigure($"customer{(seat + table) % 3 + 1}", x + seat * 12f, 30f + bob, 14f);
                }
            }
            // Zuschauer und Papa vorne
            for (int i = 0; i < 6; i++) DrawFigure($"customer{i % 3 + 1}", 8f + i * 24f, -2f, 18f, alpha: 0.85f);
            Draw(IntroArt.Get("father"), 66f, -6f);
            DrawPlayer(88f, 0f, 20f, flip: true);
        }

        /// <summary>Die legendäre Karte erscheint im Licht der zwei Monde.</summary>
        internal void DrawLegendCard(float t)
        {
            Draw(IntroArt.Get("nightSky"), 0f, 0f);
            DrawTwinkle(t, 0f, 10f, IntroArt.Width, 80f);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 2f));
            Draw(IntroArt.Get("moonRed"), Mathf.Lerp(-20f, 40f, k), 58f);
            Draw(IntroArt.Get("moonGold"), Mathf.Lerp(IntroArt.Width + 6f, 106f, k), 60f);

            // Lichtschein
            float glow = 0.5f + 0.5f * Mathf.Sin(t * 2f);
            Fill(56f, 6f, 48f, 78f, new Color(1f, 0.9f, 0.5f, 0.12f + 0.08f * glow));
            Fill(62f, 10f, 36f, 70f, new Color(1f, 0.9f, 0.5f, 0.12f + 0.08f * glow));

            float appear = Mathf.Clamp01((t - 0.8f) / 1.5f);
            float scale = Mathf.Lerp(0.6f, 1f, appear);
            float w = 42f * scale, h = 56f * scale;
            var rect = ToScreen(80f - w / 2f, 46f - h / 2f + Mathf.Sin(t * 1.5f) * 1.5f, w, h);
            GUI.color = new Color(1f, 1f, 1f, appear);
            CardUI.Draw(rect, CardDatabase.TwoMoons);
            GUI.color = Color.white;
        }

        /// <summary>Schluss: Papa und {0} vor dem Laden, über ihnen stehen beide Monde am Himmel.</summary>
        internal void DrawFinaleNight(float t)
        {
            Draw(IntroArt.Get("shopFront"), 0f, 0f);
            Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(0.04f, 0.04f, 0.14f, 0.65f));
            DrawTwinkle(t, 0f, 66f, IntroArt.Width, 24f);
            float rise = Mathf.Min(t, 12f) * 0.6f;
            Draw(IntroArt.Get("moonRed"), 20f, 66f + rise * 0.2f);
            Draw(IntroArt.Get("moonGold"), 128f, 68f + rise * 0.2f);
            float glow = 0.85f + 0.15f * Mathf.Sin(t * 1.5f);
            Fill(36f, 22f, 44f, 34f, new Color(1f, 0.8f, 0.45f, 0.85f * glow));
            Fill(50f, 62f, 60f, 10f, new Color(0.18f, 0.1f, 0.08f));
            GUI.Label(ToScreen(50f, 62f, 60f, 10f), "TwoMoons", UiKit.Title.WithAlignment(TextAnchor.MiddleCenter).WithFontSize(Mathf.RoundToInt(pixel * 6f)));
            Draw(IntroArt.Get("father"), 98f, 2f);
            DrawPlayer(82f, 2f, 22f);
            if (IsLastLine)
            {
                float a = Mathf.Clamp01(LineTime / 2f);
                Fill(0f, 0f, IntroArt.Width, IntroArt.Height, new Color(0f, 0f, 0f, a * 0.5f));
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
