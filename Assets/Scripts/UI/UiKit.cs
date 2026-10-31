using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Gemeinsame Stile und Helfer für alle Menüs (Unity IMGUI), in den Farben des TwoMoons-Logos.
    /// Alles wird auf eine virtuelle Höhe von 620 Pixeln skaliert.
    /// </summary>
    public static class UiKit
    {
        /// <summary>Höhe der Oberfläche in virtuellen Pixeln – größer = alles etwas kleiner und mehr Platz.</summary>
        public const float VirtualHeight = 620f;
        public const float Margin = 10f;

        // Logo-Farben: Dunkelbraun, Rot-Orange, Gold.
        public static readonly Color PanelColor = new Color(0.17f, 0.09f, 0.06f, 0.92f);
        public static readonly Color ButtonColor = new Color(0.72f, 0.25f, 0.1f, 1f);
        public static readonly Color ButtonHoverColor = new Color(0.9f, 0.45f, 0.12f, 1f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.25f, 1f);
        public static readonly Color Cream = new Color(1f, 0.94f, 0.82f, 1f);
        public static readonly Color Good = new Color(0.6f, 1f, 0.6f, 1f);
        public static readonly Color Bad = new Color(1f, 0.55f, 0.5f, 1f);
        public static readonly Color Dimmed = new Color(1f, 1f, 1f, 0.55f);

        public static GUIStyle Panel, Text, Bold, Title, BigTitle, Prompt, Hint, ButtonStyle, BigButtonStyle;
        static Texture2D overlay;

        /// <summary>Am Anfang jedes OnGUI aufrufen. Gibt die virtuelle Breite zurück.</summary>
        public static float Begin()
        {
            EnsureStyles();
            float scale = Screen.height / VirtualHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            return Screen.width / scale;
        }

        /// <summary>Knopf mit Klick-Sound.</summary>
        public static bool Button(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!GUILayout.Button(content, style, options)) return false;
            SoundManager.Play(Sfx.UiClick);
            return true;
        }

        public static bool Button(string label, params GUILayoutOption[] options) =>
            Button(new GUIContent(label), ButtonStyle, options);

        public static bool BigButtonLayout(string label) =>
            Button(new GUIContent(label), BigButtonStyle, GUILayout.Height(40f));

        /// <summary>Kasten mit Hintergrund, Inhalt per GUILayout.</summary>
        public static void BeginWindow(Rect area, float padding = 18f)
        {
            GUI.Box(area, GUIContent.none, Panel);
            GUILayout.BeginArea(new Rect(area.x + padding, area.y + padding * 0.75f, area.width - 2f * padding, area.height - 1.5f * padding));
        }

        public static void EndWindow() => GUILayout.EndArea();

        public static Rect Centered(float width, float windowWidth, float windowHeight, float top = -1f) =>
            new Rect((width - windowWidth) / 2f, top >= 0f ? top : (VirtualHeight - windowHeight) / 2f, windowWidth, windowHeight);

        public static void DarkenScreen(float width, float alpha)
        {
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, width, VirtualHeight), overlay);
            GUI.color = Color.white;
        }

        /// <summary>Einfarbiges Rechteck (z. B. Sprechblasen).</summary>
        public static void DrawRect(Rect rect, Color color)
        {
            EnsureStyles();
            GUI.color = color;
            GUI.DrawTexture(rect, overlay);
            GUI.color = Color.white;
        }

        public static void DrawSprite(Sprite sprite, Rect rect)
        {
            var texture = sprite.texture;
            var r = sprite.textureRect;
            var uv = new Rect(r.x / texture.width, r.y / texture.height, r.width / texture.width, r.height / texture.height);
            GUI.DrawTextureWithTexCoords(rect, texture, uv);
        }

        static Texture2D logoTexture;

        /// <summary>
        /// Zeichnet das Logo in die Box (Seitenverhältnis bleibt erhalten).
        /// Erst aus der SpriteLibrary, sonst direkt aus Assets/Art/UI/Resources (beliebiger Dateiname), sonst als Schriftzug.
        /// </summary>
        public static void DrawLogo(Rect box)
        {
            var logo = Art.Logo;
            if (logo != null)
            {
                var r = logo.textureRect;
                DrawSprite(logo, Fit(box, r.width, r.height));
                return;
            }

            if (logoTexture == null) logoTexture = LoadLogoTexture();
            if (logoTexture != null) GUI.DrawTexture(Fit(box, logoTexture.width, logoTexture.height), logoTexture);
            else GUI.Label(box, "TwoMoons", BigTitle);
        }

        /// <summary>
        /// Ein Bild aus einem Resources-Ordner, dessen Name „logo“ oder „twomoons“ enthält
        /// (z. B. logo.png oder twomoonspixel.png). Andere Bilder, etwa aus Unity-Paketen, werden ignoriert.
        /// </summary>
        public static Texture2D LoadLogoTexture()
        {
            var exact = Resources.Load<Texture2D>("logo");
            if (exact != null) return exact;
            foreach (var texture in Resources.LoadAll<Texture2D>(""))
            {
                // Kartenbilder (z. B. eine Two-Moons-Karte) sind hochkant – das Logo ist immer breiter als hoch.
                if (!IsLogoShaped(texture.width, texture.height)) continue;
                string name = texture.name.ToLowerInvariant();
                if (name.Contains("logo") || name.Contains("twomoons")) return texture;
            }
            return null;
        }

        public static bool IsLogoShaped(float width, float height) => width > height * 1.2f;

        static Rect Fit(Rect box, float width, float height)
        {
            float scale = Mathf.Min(box.width / width, box.height / height);
            var size = new Vector2(width * scale, height * scale);
            return new Rect(box.center.x - size.x / 2f, box.center.y - size.y / 2f, size.x, size.y);
        }

        /// <summary>Lautstärke-Regler und Vollbild – im Hauptmenü und im Esc-Menü gleich.</summary>
        public static void SettingsContent()
        {
            float music = VolumeSlider("Musik", SoundManager.MusicVolume);
            if (!Mathf.Approximately(music, SoundManager.MusicVolume)) SoundManager.MusicVolume = music;
            GUILayout.Space(8f);
            float effects = VolumeSlider("Effekte", SoundManager.EffectsVolume);
            if (!Mathf.Approximately(effects, SoundManager.EffectsVolume)) SoundManager.EffectsVolume = effects;
            GUILayout.Space(8f);
            if (Button(Screen.fullScreen ? "Vollbild: an  [F11]" : "Vollbild: aus  [F11]", GUILayout.Height(32f)))
                Screen.fullScreen = !Screen.fullScreen;
        }

        static float VolumeSlider(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Bold, GUILayout.Width(80f));
            float result = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.Height(20f));
            GUILayout.Label($"{Mathf.RoundToInt(result * 100f)} %", Text, GUILayout.Width(50f));
            GUILayout.EndHorizontal();
            return result;
        }

        /// <summary>Im Browser gibt es kein „Beenden“ – man schließt einfach den Tab.</summary>
        public static bool CanQuit => Application.platform != RuntimePlatform.WebGLPlayer;

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static Texture2D Solid(Color color)
        {
            // Ausdrücklich als sRGB-Textur anlegen, sonst wirken die Farben im Linear-Farbraum des Projekts zu hell.
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, texture.isDataSRGB ? color : color.linear);
            texture.Apply();
            return texture;
        }

        static void EnsureStyles()
        {
            if (Panel != null && overlay != null) return;

            overlay = Solid(Color.white);

            // Eigene Schrift mit allen Zeichen (€, –, „“, …): die eingebaute hat sie im Web-Build nicht.
            var font = Resources.Load<Font>("Fonts/LiberationSans-Regular");
            if (font != null) GUI.skin.font = font;

            Panel = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 12, 6, 6),
                wordWrap = true,
            };
            Panel.normal.background = Solid(PanelColor);
            Panel.normal.textColor = Cream;

            Text = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            Text.normal.textColor = Cream;

            Bold = new GUIStyle(Text) { fontStyle = FontStyle.Bold };
            Title = new GUIStyle(Text) { fontSize = 22, fontStyle = FontStyle.Bold };
            Title.normal.textColor = Gold;
            BigTitle = new GUIStyle(Title) { fontSize = 48, alignment = TextAnchor.MiddleCenter };

            Prompt = new GUIStyle(Panel) { alignment = TextAnchor.MiddleCenter };
            Prompt.normal.textColor = Gold;

            Hint = new GUIStyle(Text) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            Hint.normal.textColor = Gold;

            var buttonNormal = Solid(ButtonColor);
            var buttonHover = Solid(ButtonHoverColor);
            ButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };
            ButtonStyle.normal.background = buttonNormal;
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.hover.background = buttonHover;
            ButtonStyle.hover.textColor = Color.white;
            ButtonStyle.active.background = buttonHover;
            ButtonStyle.active.textColor = Color.white;
            ButtonStyle.focused.background = buttonNormal;
            ButtonStyle.focused.textColor = Color.white;

            BigButtonStyle = new GUIStyle(ButtonStyle) { fontSize = 18 };
        }
    }
}
