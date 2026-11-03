using UnityEngine;

namespace TwoMoons
{
    /// <summary>Fenster des Lagerraums: Bestand ansehen und Ware herausnehmen.</summary>
    public class StorageUI : MonoBehaviour
    {
        Vector2 scroll;

        static GameManager Game => GameManager.Instance;

        void OnGUI()
        {
            if (Game == null || !Game.StorageOpen || Game.Phase == GamePhase.MainMenu) return;
            float width = UiKit.Begin();
            UiKit.DarkenScreen(width, 0.5f);
            UiKit.BeginWindow(UiKit.Centered(width, 480f, 420f));
            GUILayout.Label("Lagerraum", UiKit.Title);
            GUILayout.Label("Ware herausnehmen: du trägst sie als Karton (so viel, wie in ein Regal passt). " +
                            "Kartons stellst du mit [E] an der Tür hinein.", UiKit.Hint);
            GUILayout.Space(6f);

            scroll = GUILayout.BeginScrollView(scroll);
            if (Game.StorageTotal == 0) GUILayout.Label("Das Lager ist leer.", UiKit.Text);
            foreach (var product in Catalog.Products)
            {
                int count = Game.StorageCount(product.Id);
                if (count <= 0) continue;
                GUILayout.BeginHorizontal(UiKit.Panel);
                var icon = GUILayoutUtility.GetRect(32f, 32f, GUILayout.Width(32f), GUILayout.Height(32f));
                UiKit.DrawSprite(Art.Get(product.SpriteKey), icon);
                GUILayout.Label($"{product.Name}\n{count} Stück", UiKit.Bold);
                GUILayout.FlexibleSpace();
                if (UiKit.Button("Herausnehmen", GUILayout.Width(130f), GUILayout.Height(34f)))
                {
                    var box = Game.TakeFromStorage(product, Shelf.Capacity);
                    if (box != null) Game.Player.PickUp(box);
                    Game.StorageOpen = false;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (UiKit.Button("Schließen [Esc]", GUILayout.Height(32f))) Game.StorageOpen = false;
            UiKit.EndWindow();
        }
    }
}
