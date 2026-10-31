using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Liefert Sprites aus der SpriteLibrary oder, falls dort leer, einen Platzhalter.</summary>
    public static class Art
    {
        public const int PixelsPerUnit = 16;

        static SpriteLibrary library;
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        /// <summary>Das TwoMoons-Logo für das Hauptmenü, oder null, wenn keins eingetragen ist.</summary>
        /// Ein hochkantiges Bild (z. B. versehentlich eine Karte eingetragen) zählt nicht – dann kommt logo.png.
        public static Sprite Logo =>
            library != null && library.logo != null && UiKit.IsLogoShaped(library.logo.rect.width, library.logo.rect.height)
                ? library.logo
                : null;

        public static void Init(SpriteLibrary spriteLibrary)
        {
            library = spriteLibrary;
            cache.Clear();
        }

        public static Sprite Get(string key)
        {
            if (cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;

            if (library != null)
            {
                var field = typeof(SpriteLibrary).GetField(key);
                sprite = field?.GetValue(library) as Sprite;
            }
            if (sprite == null) sprite = PlaceholderSprites.CreateSprite(key);

            cache[key] = sprite;
            return sprite;
        }
    }
}
