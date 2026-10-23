using UnityEngine;

namespace TwoMoons
{
    /// <summary>Erzeugt alle Objekte im Laden aus Code, damit nur die Bilder ausgetauscht werden müssen.</summary>
    public static class WorldFactory
    {
        public const int FloorOrder = -30000;
        public const int FloorDecalOrder = -29000;

        static GameManager Game => GameManager.Instance;

        /// <summary>Alles im Laden hängt hier drunter, damit die Welt für ein neues Spiel komplett abgebaut werden kann.</summary>
        public static Transform Root;

        public static SpriteRenderer CreateSprite(string name, string spriteKey, Vector3 position, int order = 0, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : Root, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Art.Get(spriteKey);
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Fester Block (Wand, Theke, Regal), an dem die Figur nicht vorbeikommt.</summary>
        public static GameObject CreateSolid(string name, string spriteKey, Vector2Int cell, Transform parent)
        {
            var renderer = CreateSprite(name, spriteKey, ShopGrid.CellToWorld(cell), 0, parent);
            renderer.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
            renderer.gameObject.AddComponent<YSort>();
            Game.Grid.SetBlocked(cell, true);
            return renderer.gameObject;
        }

        public static void CreateBlocker(Vector2Int cell, Transform parent)
        {
            var go = new GameObject("Blocker");
            go.transform.SetParent(parent != null ? parent : Root, false);
            go.transform.position = ShopGrid.CellToWorld(cell);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        public static PlayerController SpawnPlayer(Vector2Int cell)
        {
            var renderer = CreateSprite("Spieler", "player1", ShopGrid.CellToWorld(cell));
            var go = renderer.gameObject;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            go.AddComponent<YSort>();
            return go.AddComponent<PlayerController>();
        }

        public static Customer SpawnCustomer()
        {
            string key = $"customer{Random.Range(1, 4)}";
            var renderer = CreateSprite("Kunde", key, ShopGrid.CellToWorld(ShopGrid.SpawnCell));
            renderer.gameObject.AddComponent<YSort>();
            return renderer.gameObject.AddComponent<Customer>();
        }

        public static DeliveryBox SpawnBox(Vector2Int cell, Product product, int amount)
        {
            var renderer = CreateSprite("Karton", "box", ShopGrid.CellToWorld(cell));
            renderer.gameObject.AddComponent<YSort>();
            var box = renderer.gameObject.AddComponent<DeliveryBox>();
            box.Product = product;
            box.Amount = amount;
            return box;
        }

        public static DirtSpot SpawnDirt(Vector2Int cell)
        {
            var renderer = CreateSprite("Fleck", "dirt", ShopGrid.CellToWorld(cell), FloorDecalOrder);
            return renderer.gameObject.AddComponent<DirtSpot>();
        }

        public static void SpawnDirtSomewhere()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var cell = new Vector2Int(Random.Range(1, ShopGrid.Width - 1), Random.Range(2, ShopGrid.Height - 1));
                if (Game.Grid.IsWalkable(cell) && !ShopGrid.IsReserved(cell))
                {
                    SpawnDirt(cell);
                    return;
                }
            }
        }

        public static Shelf SpawnShelf(Vector2Int cell)
        {
            var go = CreateSolid("Regal", "shelf", cell, null);
            var icons = new SpriteRenderer[3];
            for (int i = 0; i < icons.Length; i++)
            {
                var position = ShopGrid.CellToWorld(cell) + new Vector3(-0.25f + i * 0.25f, -0.15f + (i % 2) * 0.3f, 0f);
                icons[i] = CreateSprite("Pack", "packMonstera", position, 1 + i, go.transform);
                icons[i].transform.localScale = Vector3.one * 0.45f;
            }
            go.GetComponent<YSort>().Collect();

            var shelf = go.AddComponent<Shelf>();
            shelf.Init(cell, icons);
            Game.Shelves.Add(shelf);
            return shelf;
        }

        public static bool HasBoxAt(Vector2Int cell)
        {
            foreach (var box in DeliveryBox.Existing)
                if (box.enabled && ShopGrid.WorldToCell(box.transform.position) == cell) return true;
            return false;
        }

        /// <summary>Freier Abstellplatz für eine Lieferung, notfalls irgendwo im Laden.</summary>
        public static Vector2Int FreeDeliveryCell()
        {
            foreach (var cell in ShopGrid.DeliveryCells)
                if (!HasBoxAt(cell)) return cell;
            for (int y = 2; y < ShopGrid.Height - 1; y++)
            for (int x = ShopGrid.Width - 2; x >= 1; x--)
            {
                var cell = new Vector2Int(x, y);
                if (Game.Grid.IsWalkable(cell) && !ShopGrid.IsReserved(cell) && !HasBoxAt(cell)) return cell;
            }
            return ShopGrid.DeliveryCells[0];
        }
    }
}
