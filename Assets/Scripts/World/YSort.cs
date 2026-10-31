using UnityEngine;

namespace TwoMoons
{
    /// <summary>Was weiter unten steht, wird davor gezeichnet (Top-Down-Tiefe).</summary>
    public class YSort : MonoBehaviour
    {
        /// <summary>Zusätzliche Verschiebung, z. B. damit ein getragener Karton vor der Figur liegt.</summary>
        public int bias;

        SpriteRenderer[] renderers;
        int[] offsets;

        void Awake() => Collect();

        /// <summary>Nach dem Hinzufügen von Kind-Sprites erneut aufrufen.</summary>
        public void Collect()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            offsets = new int[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) offsets[i] = renderers[i].sortingOrder;
        }

        void LateUpdate()
        {
            int order = -Mathf.RoundToInt(transform.position.y * 100f) + bias;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].sortingOrder = order + offsets[i];
        }
    }
}
