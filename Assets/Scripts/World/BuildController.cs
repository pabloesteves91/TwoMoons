using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Bau-Modus mit der Maus: [B] öffnet ihn, Linksklick stellt das gewählte Möbelstück auf.
    /// Liegen verschiedene Möbel bereit, wechselt ein weiteres [B] zum nächsten.
    /// Möbel stehen im Raum, Wand-Deko kommt an die hintere Wand, Teppiche auf den Boden.
    /// </summary>
    public class BuildController : MonoBehaviour
    {
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
                    if (game.TotalToPlace > 0)
                    {
                        game.BuildMode = true;
                        SwitchTo(NextAvailable(game, Current, includeCurrent: true));
                    }
                    else
                    {
                        game.ShowToast("Nichts zum Aufstellen – kauf Möbel und Deko am Computer (Tab „Einrichtung“).");
                        SoundManager.Play(Sfx.Error);
                    }
                }
                return;
            }

            if (GameInput.BuildPressed)
            {
                var next = NextAvailable(game, Current, includeCurrent: false);
                if (next != Current) SwitchTo(next);
                else game.BuildMode = false;
                return;
            }
            if (GameInput.RightClickPressed || GameInput.ConsumeCancel())
            {
                game.BuildMode = false;
                return;
            }

            Vector3 world = mainCamera.ScreenToWorldPoint(GameInput.MousePosition);
            var cell = ShopGrid.WorldToCell(world);
            bool valid = CanPlace(Current, cell);
            ghost.enabled = true;
            ghost.transform.position = ShopGrid.CellToWorld(cell);
            ghost.color = valid ? ValidColor : InvalidColor;

            if (!GameInput.ClickPressed) return;
            if (!valid)
            {
                SoundManager.Play(Sfx.Error);
                return;
            }

            SoundManager.Play(Sfx.Build);
            switch (Current)
            {
                case Furniture.Shelf:
                    WorldFactory.SpawnShelf(cell);
                    game.NotifyShelfPlaced();
                    break;
                case Furniture.Showcase:
                    WorldFactory.SpawnShowcase(cell);
                    break;
                default:
                    WorldFactory.SpawnDeco(Current, cell);
                    game.NotifyDecoPlaced();
                    break;
            }
            game.AddToPlace(Current, -1);

            if (game.TotalToPlace == 0) game.BuildMode = false;
            else if (game.ToPlace(Current) == 0) SwitchTo(NextAvailable(game, Current, includeCurrent: false));
        }

        /// <summary>Nächstes Möbelstück in der Reihenfolge, das noch aufgestellt werden kann.</summary>
        static Furniture NextAvailable(GameManager game, Furniture from, bool includeCurrent)
        {
            var all = (Furniture[])System.Enum.GetValues(typeof(Furniture));
            int start = System.Array.IndexOf(all, from);
            for (int step = includeCurrent ? 0 : 1; step <= all.Length; step++)
            {
                var candidate = all[(start + step) % all.Length];
                if (game.ToPlace(candidate) > 0) return candidate;
            }
            return from;
        }

        void SwitchTo(Furniture furniture)
        {
            Current = furniture;
            ghost.sprite = Art.Get(FurnitureCatalog.Get(furniture).SpriteKey);
            SoundManager.Play(Sfx.UiClick);
        }

        static bool CanPlace(Furniture furniture, Vector2Int cell)
        {
            var game = GameManager.Instance;
            switch (FurnitureCatalog.Get(furniture).Placement)
            {
                case Placement.Wall:
                    // Hintere Wand, ein Stück pro Feld.
                    if (cell.y != ShopGrid.Height - 1 || cell.x < 1 || cell.x > ShopGrid.Width - 2) return false;
                    return !HasDeco(game, cell, Placement.Wall);
                case Placement.Rug:
                    return ShopGrid.IsInterior(cell) && game.Grid.IsWalkable(cell) && !HasDeco(game, cell, Placement.Rug);
                default:
                    return CanPlaceBlocking(cell);
            }
        }

        static bool HasDeco(GameManager game, Vector2Int cell, Placement placement)
        {
            foreach (var deco in game.Decos)
                if (deco.Cell == cell && FurnitureCatalog.Get(deco.Type).Placement == placement) return true;
            return false;
        }

        /// <summary>Möbel, die ein Feld blockieren: Es muss frei sein und alles muss erreichbar bleiben.</summary>
        static bool CanPlaceBlocking(Vector2Int cell)
        {
            var game = GameManager.Instance;
            var grid = game.Grid;
            if (!ShopGrid.IsInterior(cell) || !grid.IsWalkable(cell) || ShopGrid.IsReserved(cell)) return false;
            if (WorldFactory.HasBoxAt(cell)) return false;
            if (ShopGrid.WorldToCell(game.Player.transform.position) == cell) return false;
            foreach (var customer in Customer.Active)
                if (ShopGrid.WorldToCell(customer.transform.position) == cell) return false;
            if (HasDeco(game, cell, Placement.Rug)) return false;

            // Probeweise blockieren und prüfen, ob noch alles erreichbar ist.
            grid.SetBlocked(cell, true);
            bool reachable = EverythingReachable(grid, cell);
            grid.SetBlocked(cell, false);
            return reachable;
        }

        static bool EverythingReachable(ShopGrid grid, Vector2Int newCell)
        {
            var game = GameManager.Instance;
            var reach = grid.ReachableFrom(ShopGrid.DoorCell);

            foreach (var cell in ShopGrid.QueueCells) if (!reach.Contains(cell)) return false;
            foreach (var cell in ShopGrid.StaffCells) if (!reach.Contains(cell)) return false;
            if (!reach.Contains(ShopGrid.WorldToCell(game.Player.transform.position))) return false;
            foreach (var box in DeliveryBox.Existing)
                if (box.enabled && !reach.Contains(ShopGrid.WorldToCell(box.transform.position))) return false;

            if (!HasReachableSide(grid, reach, newCell)) return false;
            foreach (var shelf in game.Shelves)
                if (!HasReachableSide(grid, reach, shelf.Cell)) return false;
            foreach (var showcase in game.Showcases)
                if (!HasReachableSide(grid, reach, showcase.Cell)) return false;
            return true;
        }

        static bool HasReachableSide(ShopGrid grid, HashSet<Vector2Int> reach, Vector2Int cell)
        {
            foreach (var side in grid.WalkableNeighbours(cell))
                if (reach.Contains(side)) return true;
            return false;
        }
    }
}
