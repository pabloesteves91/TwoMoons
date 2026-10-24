using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Mitarbeiter (ab Level 3): kassiert an der Kasse oder füllt Regale aus Kartons nach.
    /// Kostet jeden Abend Lohn.
    /// </summary>
    public class Employee : MonoBehaviour
    {
        public enum Role { Register, Restock }

        public const int DailyWage = 3000;
        public const string Name = "Kai";
        const float Speed = 2.4f;
        const float ServeSeconds = 2.5f;

        public Role Job;

        List<Vector2Int> path;
        int pathIndex;
        float timer;
        DeliveryBox targetBox;
        Shelf targetShelf;
        SpriteRenderer body;

        static GameManager Game => GameManager.Instance;
        bool Arrived => path == null || pathIndex >= path.Count;

        void Start() => body = GetComponent<SpriteRenderer>();

        void WalkTo(Vector2Int cell)
        {
            path = Game.Grid.FindPath(ShopGrid.WorldToCell(transform.position), cell);
            pathIndex = 0;
        }

        bool WalkNextTo(Vector2Int cell)
        {
            foreach (var spot in Game.Grid.WalkableNeighbours(cell))
            {
                var candidate = Game.Grid.FindPath(ShopGrid.WorldToCell(transform.position), spot);
                if (candidate == null) continue;
                path = candidate;
                pathIndex = 0;
                return true;
            }
            return false;
        }

        void Update()
        {
            if (!Arrived)
            {
                Vector3 target = ShopGrid.CellToWorld(path[pathIndex]);
                Vector3 step = target - transform.position;
                if (Mathf.Abs(step.x) > 0.01f) body.flipX = step.x < 0;
                transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);
                if ((transform.position - target).sqrMagnitude < 0.0001f) pathIndex++;
                return;
            }

            if (Job == Role.Register) WorkRegister();
            else WorkRestock();
        }

        /// <summary>Steht hinter der Kasse und kassiert den vordersten Kunden.</summary>
        void WorkRegister()
        {
            var post = ShopGrid.StaffCells[0];
            if (ShopGrid.WorldToCell(transform.position) != post)
            {
                WalkTo(post);
                return;
            }
            if (!Game.Register.CanServe)
            {
                timer = ServeSeconds;
                return;
            }
            timer -= Time.deltaTime;
            if (timer <= 0f) Game.Register.ServeFront();
        }

        /// <summary>Holt Ware aus Kartons am Boden und räumt sie ins passende Regal.</summary>
        void WorkRestock()
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = 1f;

            if (targetBox != null && targetShelf != null)
            {
                // Am Karton angekommen? Dann zum Regal. Am Regal? Einräumen.
                if (targetBox.transform.parent == transform)
                {
                    targetShelf.StockFrom(targetBox);
                    if (targetBox.Amount <= 0) Destroy(targetBox.gameObject);
                    else DropBox();
                    targetBox = null;
                    targetShelf = null;
                    return;
                }
                if (!targetBox.enabled) { targetBox = null; return; }
                targetBox.enabled = false;
                targetBox.transform.SetParent(transform);
                targetBox.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                targetBox.GetComponent<YSort>().bias = 60;
                if (!WalkNextTo(targetShelf.Cell)) DropBox();
                return;
            }

            // Neue Arbeit suchen: ein Karton, für den es ein Regal mit Platz gibt.
            foreach (var box in DeliveryBox.Existing)
            {
                if (!box.enabled || box.Product.Category == ProductCategory.Promo) continue;
                var shelf = Game.Shelves.Find(s => s.CanTake(box.Product));
                if (shelf == null || !WalkNextTo(ShopGrid.WorldToCell(box.transform.position))) continue;
                targetBox = box;
                targetShelf = shelf;
                return;
            }
        }

        void DropBox()
        {
            if (targetBox == null) return;
            targetBox.transform.SetParent(WorldFactory.Root);
            targetBox.transform.position = ShopGrid.CellToWorld(ShopGrid.WorldToCell(transform.position));
            targetBox.GetComponent<YSort>().bias = 0;
            targetBox.enabled = true;
            targetBox = null;
            targetShelf = null;
        }
    }
}
