using UnityEngine;

namespace TwoMoons
{
    /// <summary>Zoomt die Kamera so, dass der Laden bei jeder Fenstergröße möglichst groß zu sehen ist.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        /// <summary>Platz um den Laden herum (in Feldern), oben für die Kopfzeile, unten für die Hinweise.</summary>
        const float PaddingTop = 1.5f;
        const float PaddingBottom = 0.5f;
        const float PaddingSides = 0.3f;

        Camera cam;

        void Awake() => cam = GetComponent<Camera>();

        void LateUpdate()
        {
            float height = ShopGrid.Height + PaddingTop + PaddingBottom;
            float width = ShopGrid.Width + 2f * PaddingSides;
            cam.orthographicSize = Mathf.Max(height / 2f, width / 2f / cam.aspect);

            float centerY = (ShopGrid.Height + PaddingTop - PaddingBottom) / 2f;
            transform.position = new Vector3(ShopGrid.Width / 2f, centerY, -10f);
        }
    }
}
