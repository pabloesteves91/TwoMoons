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
            // Die Lichterkette funkelt sanft, der Fernseher flackert, wenn jemand zockt.
            if (Type == Furniture.FairyLights)
            {
                float glow = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f + Cell.x);
                spriteRenderer.color = new Color(glow, glow, glow, 1f);
            }
            else if (Type == Furniture.Tv)
            {
                bool playing = Customer.Active.Exists(c => c.Couch != null);
                float flicker = playing ? 0.8f + 0.2f * Mathf.PerlinNoise(Time.time * 8f, 0f) : 0.7f;
                spriteRenderer.color = playing ? new Color(flicker, flicker, 1f, 1f) : new Color(flicker, flicker, flicker, 1f);
            }
        }
    }
}
