using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Kunde: läuft zu einem Regal, nimmt Packs, stellt sich an die Kasse und geht wieder.</summary>
    public class Customer : MonoBehaviour
    {
        public static readonly List<Customer> Active = new List<Customer>();

        enum State { ToShelf, Browsing, ToQueue, Queued, Leaving }

        const float Speed = 2.2f;
        const float BrowseSeconds = 2f;
        const float PatienceSeconds = 30f;

        /// <summary>Bis zu diesem Anteil vom Marktwert gilt der Preis als Schnäppchen.</summary>
        const float BargainRatio = 0.85f;
        /// <summary>Ab Marktwert steigt die Chance, dass der Kunde nicht kauft; bei +60 % kauft keiner mehr.</summary>
        const float PainRange = 0.6f;

        State state;
        List<Vector2Int> path;
        int pathIndex;
        float timer;
        int queueIndex = -1;
        Shelf shelf;
        Product product;
        int itemCount;
        int unitPrice;
        bool bargain;
        SpriteRenderer spriteRenderer;

        public int Total => itemCount * unitPrice;

        public bool IsWaitingAtRegister => state == State.Queued && queueIndex == 0 && Arrived;

        bool Arrived => path == null || pathIndex >= path.Count;

        static GameManager Game => GameManager.Instance;

        Vector2Int CurrentCell => ShopGrid.WorldToCell(transform.position);

        void OnEnable() => Active.Add(this);

        void OnDisable() => Active.Remove(this);

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            PickShelf();
        }

        void PickShelf()
        {
            var withStock = Game.Shelves.FindAll(s => s.Stock > 0);
            var candidates = withStock.Count > 0 ? withStock : Game.Shelves;
            if (candidates.Count == 0)
            {
                Leave(false, "Ein Kunde ist gegangen: Es gibt noch keine Regale.");
                return;
            }

            shelf = candidates[Random.Range(0, candidates.Count)];
            if (!WalkToShelf(shelf)) Leave(false, "Ein Kunde ist gegangen: Er kam nicht ans Regal.");
            else state = State.ToShelf;
        }

        bool WalkToShelf(Shelf target)
        {
            List<Vector2Int> best = null;
            foreach (var spot in Game.Grid.WalkableNeighbours(target.Cell))
            {
                var candidate = Game.Grid.FindPath(CurrentCell, spot);
                if (candidate != null && (best == null || candidate.Count < best.Count)) best = candidate;
            }
            if (best == null) return false;
            path = best;
            pathIndex = 0;
            return true;
        }

        bool WalkTo(Vector2Int cell)
        {
            path = Game.Grid.FindPath(CurrentCell, cell);
            pathIndex = 0;
            return path != null;
        }

        public void SetQueueIndex(int index)
        {
            queueIndex = index;
            WalkTo(ShopGrid.QueueCells[index]);
        }

        void Update()
        {
            MoveAlongPath();

            switch (state)
            {
                case State.ToShelf:
                    if (Arrived)
                    {
                        state = State.Browsing;
                        timer = BrowseSeconds;
                    }
                    break;

                case State.Browsing:
                    timer -= Time.deltaTime;
                    if (timer > 0f) break;
                    product = shelf != null ? shelf.Product : null;
                    if (product == null)
                    {
                        Leave(false, "Ein Kunde ist gegangen: Das Regal war leer.");
                        break;
                    }

                    // Preis mit dem Marktwert vergleichen.
                    unitPrice = Game.GetPrice(product);
                    float ratio = unitPrice / (float)product.MarketValue;
                    if (Random.value < Mathf.Clamp01((ratio - 1f) / PainRange))
                    {
                        Leave(false, $"Ein Kunde fand {product.Name} zu teuer ({Money.Format(unitPrice)}).");
                        break;
                    }
                    bargain = ratio <= BargainRatio;
                    itemCount = shelf.Take(Random.Range(1, 4) + (bargain ? 1 : 0));

                    if (itemCount == 0)
                    {
                        Leave(false, "Ein Kunde ist gegangen: Das Regal war leer.");
                    }
                    else if (Game.Register.TryJoin(this))
                    {
                        state = State.ToQueue;
                        timer = PatienceSeconds;
                    }
                    else
                    {
                        Leave(false, "Ein Kunde ist gegangen: Die Schlange war zu lang.");
                    }
                    break;

                case State.ToQueue:
                case State.Queued:
                    state = State.Queued;
                    timer -= Time.deltaTime;
                    if (timer <= 0f) Leave(false, "Ein Kunde ist gegangen: Er hat zu lange gewartet.");
                    break;

                case State.Leaving:
                    if (Arrived) Destroy(gameObject);
                    break;
            }
        }

        void MoveAlongPath()
        {
            if (Arrived) return;
            Vector3 target = ShopGrid.CellToWorld(path[pathIndex]);
            Vector3 step = target - transform.position;
            if (Mathf.Abs(step.x) > 0.01f) spriteRenderer.flipX = step.x < 0;
            transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude < 0.0001f) pathIndex++;
        }

        public void Pay()
        {
            Game.RegisterSale(Total, itemCount);
            itemCount = 0;
            Leave(true, null);
        }

        void Leave(bool happy, string reason)
        {
            if (state == State.Leaving) return;

            Game.Register.Remove(this);
            if (itemCount > 0 && shelf != null) shelf.PutBack(product, itemCount);
            itemCount = 0;

            if (happy) Game.CustomerLeftHappy(bargain);
            else Game.CustomerLeftUnhappy(reason);

            state = State.Leaving;
            if (!WalkTo(ShopGrid.SpawnCell)) Destroy(gameObject);
        }
    }
}
