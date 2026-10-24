using UnityEngine;

namespace TwoMoons
{
    /// <summary>Deko im Laden (Pflanze, Poster, Teppich …). Zählt zur Gemütlichkeit.</summary>
    public class DecoItem : MonoBehaviour
    {
        public Furniture Type { get; private set; }
        public Vector2Int Cell { get; private set; }
        /// <summary>Nach dem Verschieben im Bau-Modus.</summary>
        public void SetCell(Vector2Int cell) => Cell = cell;

        SpriteRenderer spriteRenderer;
        /// <summary>Pokale auf der Trophäenwand, einer pro Turnier (höchstens 6).</summary>
        SpriteRenderer[] trophies;

        public void Init(Furniture type, Vector2Int cell)
        {
            Type = type;
            Cell = cell;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (type == Furniture.TrophyWall)
            {
                trophies = new SpriteRenderer[6];
                for (int i = 0; i < trophies.Length; i++)
                {
                    var offset = new Vector3(-0.3f + (i % 3) * 0.3f, 0.12f - (i / 3) * 0.32f, 0f);
                    trophies[i] = WorldFactory.CreateSprite("Pokal", "trophy", transform.position + offset, spriteRenderer.sortingOrder + 1, transform);
                    trophies[i].transform.localScale = Vector3.one * 0.3f;
                }
            }
        }

        /// <summary>Spielt gerade jemand an diesem Automaten bzw. sitzt auf dieser Couch?</summary>
        public bool InUse => Customer.Active.Exists(c => c.Couch == this);

        void Update()
        {
            // Die Lichterkette funkelt sanft, der Fernseher flackert, wenn jemand zockt.
            if (Type == Furniture.FairyLights)
            {
                float glow = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f + Cell.x);
                spriteRenderer.color = new Color(glow, glow, glow, 1f);
            }
            else if (Type == Furniture.Arcade)
            {
                float hue = InUse ? Mathf.Repeat(Time.time * 0.8f, 1f) : 0f;
                spriteRenderer.color = InUse ? Color.Lerp(Color.white, Color.HSVToRGB(hue, 0.5f, 1f), 0.5f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else if (Type == Furniture.NeonSign)
            {
                float glow = 0.85f + 0.15f * Mathf.Sin(Time.time * 2f + Cell.x);
                spriteRenderer.color = new Color(glow, glow, glow, 1f);
            }
            else if (Type == Furniture.TrophyWall)
            {
                int held = GameManager.Instance.Tournament.Held;
                for (int i = 0; i < trophies.Length; i++) trophies[i].enabled = i < held;
            }
            else if (Type == Furniture.Tv)
            {
                bool playing = Customer.Active.Exists(c => c.Couch != null && c.Couch.Type == Furniture.Couch);
                float flicker = playing ? 0.8f + 0.2f * Mathf.PerlinNoise(Time.time * 8f, 0f) : 0.7f;
                spriteRenderer.color = playing ? new Color(flicker, flicker, 1f, 1f) : new Color(flicker, flicker, flicker, 1f);
            }
        }
    }
}
