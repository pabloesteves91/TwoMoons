using UnityEngine;

namespace TwoMoons
{
    /// <summary>Deko im Laden (Pflanze, Poster, Teppich …). Zählt zur Gemütlichkeit.</summary>
    public class DecoItem : MonoBehaviour
    {
        public Furniture Type { get; private set; }
        public Vector2Int Cell { get; private set; }

        SpriteRenderer spriteRenderer;

        public void Init(Furniture type, Vector2Int cell)
        {
            Type = type;
            Cell = cell;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            // Die Lichterkette funkelt sanft.
            if (Type != Furniture.FairyLights) return;
            float glow = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f + Cell.x);
            spriteRenderer.color = new Color(glow, glow, glow, 1f);
        }
    }
}
