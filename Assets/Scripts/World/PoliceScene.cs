using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Nach einem Überfall bis Ladenschluss im Laden: zwei Polizisten am Eingang, die Spurensicherung an der Kasse,
    /// Absperrband und Beweis-Markierungen. Reine Deko – Kunden laufen ganz normal.
    /// </summary>
    public class PoliceScene : MonoBehaviour
    {
        static PoliceScene current;

        public static bool Active => current != null;

        public static void Create()
        {
            if (current != null) return;
            current = new GameObject("Polizei").AddComponent<PoliceScene>();
            current.transform.SetParent(WorldFactory.Root);
            current.Build();
        }

        public static void Remove()
        {
            if (current != null) Destroy(current.gameObject);
            current = null;
        }

        readonly List<SpriteRenderer> people = new List<SpriteRenderer>();
        float turnTimer;

        void Build()
        {
            var grid = GameManager.Instance.Grid;
            var door = ShopGrid.DoorCell;
            var queue = ShopGrid.QueueCells[0];

            // Polizisten links und rechts hinter der Tür
            Person("Polizist", "police", Free(grid, door + new Vector2Int(-1, 1)), false);
            Person("Polizistin", "police", Free(grid, door + new Vector2Int(1, 1)), true);
            // Spurensicherung im weißen Anzug neben der Kasse
            Person("Spurensicherung", "forensic", Free(grid, queue + Vector2Int.left), false);

            // Absperrband vor dem Kassenbereich
            for (int x = 1; x <= queue.x + 2; x++)
                Prop("Absperrband", "policeTape", new Vector2Int(x, ShopGrid.QueueCells[ShopGrid.QueueCells.Length - 1].y - 1));

            // Gelbe Beweis-Markierungen am Boden
            foreach (var offset in new[] { new Vector2Int(-1, 1), new Vector2Int(1, 0), new Vector2Int(1, -2) })
            {
                var cell = queue + offset;
                if (grid.IsWalkable(cell)) Prop("Beweis", "evidenceMarker", cell, WorldFactory.FloorDecalOrder + 1);
            }
        }

        static Vector2Int Free(ShopGrid grid, Vector2Int cell)
        {
            if (grid.IsWalkable(cell)) return cell;
            foreach (var neighbour in grid.WalkableNeighbours(cell)) return neighbour;
            return cell;
        }

        void Person(string name, string sprite, Vector2Int cell, bool flip)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(cell), 0, transform);
            renderer.flipX = flip;
            renderer.gameObject.AddComponent<YSort>();
            people.Add(renderer);
        }

        void Prop(string name, string sprite, Vector2Int cell, int order = 0)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(cell), order, transform);
            if (order == 0) renderer.gameObject.AddComponent<YSort>();
        }

        /// <summary>Ab und zu schaut jemand in die andere Richtung.</summary>
        void Update()
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer > 0f || people.Count == 0) return;
            turnTimer = Random.Range(2f, 5f);
            var person = people[Random.Range(0, people.Count)];
            if (person != null) person.flipX = !person.flipX;
        }
    }
}
