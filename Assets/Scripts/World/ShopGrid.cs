using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Das Raster des Ladens. Ein Feld = 1 Unity-Einheit = 16 Pixel.
    /// Innenraum: x 1–10, y 2–9 (10×8 Felder). y = 0 ist die Straße, y = 1 die untere Wand mit der Tür.
    /// </summary>
    public class ShopGrid : MonoBehaviour
    {
        public const int Width = 12;
        public const int Height = 11;

        public static readonly Vector2Int SpawnCell = new Vector2Int(6, 0);
        public static readonly Vector2Int DoorCell = new Vector2Int(6, 1);
        public static readonly Vector2Int SignCell = new Vector2Int(5, 1);
        public static readonly Vector2Int RegisterCell = new Vector2Int(3, 7);
        public static readonly Vector2Int ComputerCell = new Vector2Int(1, 9);
        public static readonly Vector2Int PlayerStartCell = new Vector2Int(6, 4);

        public static readonly Vector2Int[] CounterCells =
            { new Vector2Int(1, 7), new Vector2Int(2, 7), new Vector2Int(3, 7), new Vector2Int(4, 7) };

        /// <summary>Warteschlange vor der Kasse, vorne zuerst.</summary>
        public static readonly Vector2Int[] QueueCells =
            { new Vector2Int(3, 6), new Vector2Int(3, 5), new Vector2Int(3, 4), new Vector2Int(3, 3) };

        /// <summary>Hier stellt der Lieferdienst morgens die Kartons ab.</summary>
        public static readonly Vector2Int[] DeliveryCells =
        {
            new Vector2Int(8, 2), new Vector2Int(9, 2), new Vector2Int(10, 2),
            new Vector2Int(8, 3), new Vector2Int(9, 3), new Vector2Int(10, 3),
        };

        /// <summary>Felder, die immer erreichbar bleiben müssen (hinter der Kasse, am Computer).</summary>
        public static readonly Vector2Int[] StaffCells = { new Vector2Int(3, 8), new Vector2Int(2, 9) };

        readonly bool[,] blocked = new bool[Width, Height];

        public void Init()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                bool wall = x == 0 || x == Width - 1 || y == 1 || y == Height - 1;
                bool street = y == 0;
                blocked[x, y] = (wall && !street && new Vector2Int(x, y) != DoorCell) || (street && x != SpawnCell.x);
            }
        }

        public static bool InBounds(Vector2Int c) => c.x >= 0 && c.x < Width && c.y >= 0 && c.y < Height;

        public static bool IsInterior(Vector2Int c) => c.x >= 1 && c.x <= Width - 2 && c.y >= 2 && c.y <= Height - 2;

        public static Vector3 CellToWorld(Vector2Int c) => new Vector3(c.x + 0.5f, c.y + 0.5f, 0f);

        public static Vector2Int WorldToCell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));

        public bool IsWalkable(Vector2Int c) => InBounds(c) && !blocked[c.x, c.y];

        public void SetBlocked(Vector2Int c, bool value)
        {
            if (InBounds(c)) blocked[c.x, c.y] = value;
        }

        public static bool IsReserved(Vector2Int c)
        {
            if (c == DoorCell + Vector2Int.up) return true;
            if (c.x <= 4 && c.y >= 8) return true; // hinter der Theke
            foreach (var q in QueueCells) if (q == c) return true;
            foreach (var d in DeliveryCells) if (d == c) return true;
            return false;
        }

        static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public IEnumerable<Vector2Int> WalkableNeighbours(Vector2Int c)
        {
            foreach (var d in Directions)
            {
                var n = c + d;
                if (IsWalkable(n)) yield return n;
            }
        }

        /// <summary>Alle Felder, die von <paramref name="start"/> aus zu Fuß erreichbar sind.</summary>
        public HashSet<Vector2Int> ReachableFrom(Vector2Int start)
        {
            var seen = new HashSet<Vector2Int>();
            if (!IsWalkable(start)) return seen;
            var open = new Queue<Vector2Int>();
            open.Enqueue(start);
            seen.Add(start);
            while (open.Count > 0)
            {
                var c = open.Dequeue();
                foreach (var n in WalkableNeighbours(c))
                    if (seen.Add(n)) open.Enqueue(n);
            }
            return seen;
        }

        /// <summary>Kürzester Weg (Breitensuche), ohne das Startfeld. Null, wenn es keinen Weg gibt.</summary>
        public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            if (!IsWalkable(to)) return null;
            var cameFrom = new Dictionary<Vector2Int, Vector2Int> { [from] = from };
            var open = new Queue<Vector2Int>();
            open.Enqueue(from);
            while (open.Count > 0)
            {
                var c = open.Dequeue();
                if (c == to)
                {
                    var path = new List<Vector2Int>();
                    for (var p = to; p != from; p = cameFrom[p]) path.Add(p);
                    path.Reverse();
                    return path;
                }
                foreach (var n in WalkableNeighbours(c))
                {
                    if (cameFrom.ContainsKey(n)) continue;
                    cameFrom[n] = c;
                    open.Enqueue(n);
                }
            }
            return null;
        }
    }
}
