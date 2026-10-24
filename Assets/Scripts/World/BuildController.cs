using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Bau-Modus mit der Maus: [B] öffnet ihn, Linksklick stellt ein Möbelstück auf.
    /// Sind Regale und Vitrinen da, wechselt ein weiteres [B] zwischen beiden.
    /// </summary>
    public class BuildController : MonoBehaviour
    {
        public enum Furniture { Shelf, Showcase }

        /// <summary>Was gerade aufgestellt wird.</summary>
        public static Furniture Current { get; private set; }

        static readonly Color ValidColor = new Color(0.5f, 1f, 0.5f, 0.7f);
        static readonly Color InvalidColor = new Color(1f, 0.4f, 0.4f, 0.7f);

        SpriteRenderer ghost;
        Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
            ghost = WorldFactory.CreateSprite("Bau-Vorschau", "shelf", Vector3.zero, 30000, transform);
            ghost.transform.position = Vector3.zero;
            ghost.enabled = false;
        }

        void Update()
        {
            var game = GameManager.Instance;

            if (!game.BuildMode)
            {
                ghost.enabled = false;
                if (GameInput.BuildPressed && !game.InputLocked && game.Phase != GamePhase.Summary)
                {
                    if (game.ShelvesToPlace + game.ShowcasesToPlace > 0)
                    {
                        Current = game.ShelvesToPlace > 0 ? Furniture.Shelf : Furniture.Showcase;
                        game.BuildMode = true;
                        ghost.sprite = Art.Get(Current == Furniture.Shelf ? "shelf" : "showcase");
                        SoundManager.Play(Sfx.UiClick);
                    }
                    else
                    {
                        game.ShowToast("Nichts zum Aufstellen – bestell Regale oder Vitrinen am Computer.");
                        SoundManager.Play(Sfx.Error);
                    }
                }
                return;
            }

            if (GameInput.BuildPressed && Current == Furniture.Shelf && game.ShowcasesToPlace > 0)
            {
                SwitchTo(Furniture.Showcase);
                return;
            }
            if (GameInput.BuildPressed && Current == Furniture.Showcase && game.ShelvesToPlace > 0)
            {
                SwitchTo(Furniture.Shelf);
                return;
            }
            if (GameInput.BuildPressed || GameInput.RightClickPressed || GameInput.ConsumeCancel())
            {
                game.BuildMode = false;
                return;
            }

            Vector3 world = mainCamera.ScreenToWorldPoint(GameInput.MousePosition);
            var cell = ShopGrid.WorldToCell(world);
            bool valid = CanPlace(cell);
            ghost.enabled = true;
            ghost.transform.position = ShopGrid.CellToWorld(cell);
            ghost.color = valid ? ValidColor : InvalidColor;

            if (!GameInput.ClickPressed) return;
            if (!valid)
            {
                SoundManager.Play(Sfx.Error);
            }
            else
            {
                SoundManager.Play(Sfx.Build);
                if (Current == Furniture.Shelf)
                {
                    WorldFactory.SpawnShelf(cell);
                    game.ShelvesToPlace--;
                    game.NotifyShelfPlaced();
                }
                else
                {
                    WorldFactory.SpawnShowcase(cell);
                    game.ShowcasesToPlace--;
                }

                if (Current == Furniture.Shelf && game.ShelvesToPlace == 0 && game.ShowcasesToPlace > 0) SwitchTo(Furniture.Showcase);
                else if (Current == Furniture.Showcase && game.ShowcasesToPlace == 0 && game.ShelvesToPlace > 0) SwitchTo(Furniture.Shelf);
                else if (game.ShelvesToPlace + game.ShowcasesToPlace == 0) game.BuildMode = false;
            }
        }

        void SwitchTo(Furniture furniture)
        {
            Current = furniture;
            ghost.sprite = Art.Get(furniture == Furniture.Shelf ? "shelf" : "showcase");
            SoundManager.Play(Sfx.UiClick);
        }

        static bool CanPlace(Vector2Int cell)
        {
            var game = GameManager.Instance;
            var grid = game.Grid;
            if (!ShopGrid.IsInterior(cell) || !grid.IsWalkable(cell) || ShopGrid.IsReserved(cell)) return false;
            if (WorldFactory.HasBoxAt(cell)) return false;
            if (ShopGrid.WorldToCell(game.Player.transform.position) == cell) return false;
            foreach (var customer in Customer.Active)
                if (ShopGrid.WorldToCell(customer.transform.position) == cell) return false;

            // Probeweise blockieren und prüfen, ob noch alles erreichbar ist.
            grid.SetBlocked(cell, true);
            bool reachable = EverythingReachable(grid, cell);
            grid.SetBlocked(cell, false);
            return reachable;
        }

        static bool EverythingReachable(ShopGrid grid, Vector2Int newShelf)
        {
            var game = GameManager.Instance;
            var reach = grid.ReachableFrom(ShopGrid.DoorCell);

            foreach (var cell in ShopGrid.QueueCells) if (!reach.Contains(cell)) return false;
            foreach (var cell in ShopGrid.StaffCells) if (!reach.Contains(cell)) return false;
            if (!reach.Contains(ShopGrid.WorldToCell(game.Player.transform.position))) return false;
            foreach (var box in DeliveryBox.Existing)
                if (box.enabled && !reach.Contains(ShopGrid.WorldToCell(box.transform.position))) return false;

            if (!HasReachableSide(grid, reach, newShelf)) return false;
            foreach (var shelf in game.Shelves)
                if (!HasReachableSide(grid, reach, shelf.Cell)) return false;
            foreach (var showcase in game.Showcases)
                if (!HasReachableSide(grid, reach, showcase.Cell)) return false;
            return true;
        }

        static bool HasReachableSide(ShopGrid grid, System.Collections.Generic.HashSet<Vector2Int> reach, Vector2Int cell)
        {
            foreach (var side in grid.WalkableNeighbours(cell))
                if (reach.Contains(side)) return true;
            return false;
        }
    }
}
