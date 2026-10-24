using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Bau-Modus mit der Maus: [B] öffnet ihn, Linksklick stellt das gewählte Möbelstück auf.
    /// Liegen verschiedene Möbel bereit, wechselt ein weiteres [B] zum nächsten.
    /// Möbel stehen im Raum, Wand-Deko kommt an die hintere Wand, Teppiche auf den Boden.
    /// Morgens vor Ladenöffnung lassen sich aufgestellte Möbel verschieben: anklicken, dann woanders hinklicken.
    /// </summary>
    public class BuildController : MonoBehaviour
    {
        /// <summary>Was gerade aufgestellt wird.</summary>
        public static Furniture Current { get; private set; }

        /// <summary>Nach dem Auspacken eines Pakets: Bau-Modus sofort öffnen.</summary>
        public static bool StartRequested;

        static readonly Color ValidColor = new Color(0.5f, 1f, 0.5f, 0.7f);
        static readonly Color InvalidColor = new Color(1f, 0.4f, 0.4f, 0.7f);

        SpriteRenderer ghost;
        Camera mainCamera;

        /// <summary>Möbelstück, das gerade verschoben wird (null = keins).</summary>
        static GameObject moving;
        static Vector2Int movingFrom;
        static Furniture movingType;

        public static bool IsMoving => moving != null;
        /// <summary>Bau-Modus ohne neue Möbel: nur verschieben.</summary>
        public static bool MoveOnly => GameManager.Instance.TotalToPlace == 0;
        public static bool CanMoveNow => GameManager.Instance.Phase == GamePhase.Preparation;

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
                if (moving != null) CancelMove();
                ghost.enabled = false;
                bool requested = StartRequested;
                StartRequested = false;
                if ((GameInput.BuildPressed || requested) && !game.InputLocked && game.Phase != GamePhase.Summary)
                {
                    if (game.TotalToPlace > 0)
                    {
                        game.BuildMode = true;
                        SwitchTo(NextAvailable(game, Current, includeCurrent: true));
                    }
                    else if (CanMoveNow)
                    {
                        // Nichts Neues zum Aufstellen: Bau-Modus nur zum Verschieben.
                        game.BuildMode = true;
                        SoundManager.Play(Sfx.UiClick);
                    }
                    else
                    {
                        game.ShowToast(game.Package != null
                            ? "Erst das Paket am Eingang auspacken ([E])."
                            : "Nichts zum Aufstellen. Möbel verschieben geht morgens vor Ladenöffnung mit [B].");
                        SoundManager.Play(Sfx.Error);
                    }
                }
                return;
            }

            if (GameInput.RightClickPressed || GameInput.ConsumeCancel())
            {
                // Erst das Verschieben abbrechen, dann den Bau-Modus.
                if (moving != null) CancelMove();
                else game.BuildMode = false;
                return;
            }
            if (GameInput.BuildPressed)
            {
                if (moving != null) CancelMove();
                var next = MoveOnly ? Current : NextAvailable(game, Current, includeCurrent: false);
                if (next != Current) SwitchTo(next);
                else game.BuildMode = false;
                return;
            }

            Vector3 world = mainCamera.ScreenToWorldPoint(GameInput.MousePosition);
            var cell = ShopGrid.WorldToCell(world);

            if (moving != null)
            {
                UpdateMove(cell);
                return;
            }

            // Klick auf ein aufgestelltes Möbelstück: aufnehmen und verschieben.
            var placed = PlacedAt(game, cell);
            if (MoveOnly)
            {
                ghost.enabled = placed != null;
                if (placed != null)
                {
                    ghost.sprite = placed.GetComponent<SpriteRenderer>().sprite;
                    ghost.transform.position = ShopGrid.CellToWorld(cell);
                    ghost.color = CanMoveNow ? ValidColor : InvalidColor;
                }
                if (GameInput.ClickPressed && placed != null) StartMove(placed, cell);
                return;
            }

            bool valid = CanPlace(Current, cell);
            if (!valid && placed != null && GameInput.ClickPressed)
            {
                StartMove(placed, cell);
                return;
            }
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
                case Furniture.AccessoryStand:
                    WorldFactory.SpawnShelf(cell, ProductCategory.Accessory);
                    break;
                case Furniture.Fridge:
                    WorldFactory.SpawnShelf(cell, ProductCategory.Drink);
                    break;
                case Furniture.PrintShelf:
                    WorldFactory.SpawnShelf(cell, ProductCategory.Print);
                    break;
                case Furniture.Showcase:
                    WorldFactory.SpawnShowcase(cell);
                    break;
                case Furniture.PremiumShowcase:
                    WorldFactory.SpawnShowcase(cell, true);
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

        // ---------- Verschieben ----------

        /// <summary>Was an diesem Feld steht: zuerst Möbel im Raum, dann Wand-Deko, zuletzt Teppiche.</summary>
        static GameObject PlacedAt(GameManager game, Vector2Int cell)
        {
            foreach (var shelf in game.Shelves) if (shelf.Cell == cell) return shelf.gameObject;
            foreach (var showcase in game.Showcases) if (showcase.Cell == cell) return showcase.gameObject;
            GameObject rug = null;
            foreach (var deco in game.Decos)
            {
                if (deco.Cell != cell) continue;
                if (FurnitureCatalog.Get(deco.Type).Placement == Placement.Rug) rug = deco.gameObject;
                else return deco.gameObject;
            }
            return rug;
        }

        static Furniture TypeOf(GameObject go)
        {
            var deco = go.GetComponent<DecoItem>();
            if (deco != null) return deco.Type;
            var showcase = go.GetComponent<Showcase>();
            if (showcase != null) return showcase.Premium ? Furniture.PremiumShowcase : Furniture.Showcase;
            switch (go.GetComponent<Shelf>().Category)
            {
                case ProductCategory.Accessory: return Furniture.AccessoryStand;
                case ProductCategory.Drink: return Furniture.Fridge;
                case ProductCategory.Print: return Furniture.PrintShelf;
                default: return Furniture.Shelf;
            }
        }

        void StartMove(GameObject placed, Vector2Int cell)
        {
            var game = GameManager.Instance;
            if (!CanMoveNow)
            {
                game.ShowToast("Möbel verschieben geht nur morgens, bevor der Laden öffnet.");
                SoundManager.Play(Sfx.Error);
                return;
            }
            var tournament = placed.GetComponent<GameTable>() != null || placed.GetComponent<PrizeCounter>() != null;
            if (tournament && game.Tournament.Busy)
            {
                game.ShowToast("Während eines Turniers bleiben Spieltisch und Counter stehen.");
                SoundManager.Play(Sfx.Error);
                return;
            }

            moving = placed;
            movingFrom = cell;
            movingType = TypeOf(placed);
            // Solange es in der Hand ist, zählt es nicht als aufgestellt.
            SetListed(game, placed, false);
            if (FurnitureCatalog.Get(movingType).Placement == Placement.Floor) game.Grid.SetBlocked(cell, false);
            SetVisible(placed, false);
            ghost.sprite = placed.GetComponent<SpriteRenderer>().sprite;
            SoundManager.Play(Sfx.UiClick);
        }

        void UpdateMove(Vector2Int cell)
        {
            bool valid = CanPlace(movingType, cell);
            ghost.enabled = true;
            ghost.transform.position = ShopGrid.CellToWorld(cell);
            ghost.color = valid ? ValidColor : InvalidColor;
            if (!GameInput.ClickPressed) return;
            if (!valid)
            {
                SoundManager.Play(Sfx.Error);
                return;
            }
            Drop(cell);
            SoundManager.Play(Sfx.Build);
        }

        static void CancelMove() => Drop(movingFrom);

        static void Drop(Vector2Int cell)
        {
            var game = GameManager.Instance;
            var go = moving;
            moving = null;
            if (go == null) return;
            go.transform.position = ShopGrid.CellToWorld(cell);
            if (FurnitureCatalog.Get(movingType).Placement == Placement.Floor) game.Grid.SetBlocked(cell, true);
            go.GetComponent<Shelf>()?.SetCell(cell);
            go.GetComponent<Showcase>()?.SetCell(cell);
            go.GetComponent<DecoItem>()?.SetCell(cell);
            go.GetComponent<GameTable>()?.SetCell(cell);
            go.GetComponent<PrizeCounter>()?.SetCell(cell);
            go.GetComponent<Printer3D>()?.SetCell(cell);
            go.GetComponent<GradingStation>()?.SetCell(cell);
            go.GetComponent<TrashBin>()?.SetCell(cell);
            SetListed(game, go, true);
            SetVisible(go, true);
        }

        static void SetListed(GameManager game, GameObject go, bool listed)
        {
            var shelf = go.GetComponent<Shelf>();
            var showcase = go.GetComponent<Showcase>();
            var deco = go.GetComponent<DecoItem>();
            if (listed)
            {
                if (shelf != null) game.Shelves.Add(shelf);
                if (showcase != null) game.Showcases.Add(showcase);
                if (deco != null) game.Decos.Add(deco);
            }
            else
            {
                if (shelf != null) game.Shelves.Remove(shelf);
                if (showcase != null) game.Showcases.Remove(showcase);
                if (deco != null) game.Decos.Remove(deco);
            }
        }

        static void SetVisible(GameObject go, bool visible)
        {
            foreach (var renderer in go.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = visible;
            foreach (var collider in go.GetComponentsInChildren<Collider2D>(true)) collider.enabled = visible;
            // Symbole (Füllstand, Balken …) wieder richtig setzen.
            if (visible) go.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
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
