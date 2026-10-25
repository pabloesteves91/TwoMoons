using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Spieltisch für 2 Spieler: einer links, einer rechts. Diese beiden Felder müssen frei bleiben.</summary>
    public class GameTable : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }
        /// <summary>Nach dem Verschieben im Bau-Modus.</summary>
        public void SetCell(Vector2Int cell) => Cell = cell;

        public void Init(Vector2Int cell) => Cell = cell;

        public List<Vector2Int> Seats()
        {
            var seats = new List<Vector2Int>();
            var grid = GameManager.Instance.Grid;
            foreach (var cell in SeatCells(Cell))
                if (grid.IsWalkable(cell) && !ShopGrid.IsReserved(cell)) seats.Add(cell);
            return seats;
        }

        /// <summary>Die beiden Spielerplätze eines Spieltischs: links und rechts daneben.</summary>
        public static Vector2Int[] SeatCells(Vector2Int table) => new[] { table + Vector2Int.left, table + Vector2Int.right };
    }
}
