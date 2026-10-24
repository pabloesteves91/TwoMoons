using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Spieltisch: bis zu 4 Plätze auf den freien Feldern rundherum.</summary>
    public class GameTable : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }

        public void Init(Vector2Int cell) => Cell = cell;

        public List<Vector2Int> Seats()
        {
            var seats = new List<Vector2Int>();
            foreach (var cell in GameManager.Instance.Grid.WalkableNeighbours(Cell))
                if (!ShopGrid.IsReserved(cell)) seats.Add(cell);
            return seats;
        }
    }
}
