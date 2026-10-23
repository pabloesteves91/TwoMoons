using UnityEngine;

namespace TwoMoons
{
    /// <summary>Zoomt die Kamera so, dass der Laden bei jeder Fenstergröße möglichst groß zu sehen ist.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        /// <summary>Platz um den Laden herum (in Feldern), oben für die Kopfzeile, unten für die Hinweise.</summary>
        const float PaddingTop = 1.0f;
        const float PaddingBottom = 0.6f;
        const float PaddingSides = 0.5f;

        /// <summary>Etwas nach rechts verschoben, damit rechts Platz für die Aufgaben bleibt.</summary>
        const float ShiftRight = 1f;

        Camera cam;

        void Awake() => cam = GetComponent<Camera>();

        void LateUpdate()
        {
            float height = ShopGrid.Height + PaddingTop + PaddingBottom;
            float width = ShopGrid.Width + 2f * PaddingSides + 2f * ShiftRight;
            cam.orthographicSize = Mathf.Max(height / 2f, width / 2f / cam.aspect);

            float centerY = (ShopGrid.Height + PaddingTop - PaddingBottom) / 2f;
            transform.position = new Vector3(ShopGrid.Width / 2f + ShiftRight, centerY, -10f);
        }
    }
}
