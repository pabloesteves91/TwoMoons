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
