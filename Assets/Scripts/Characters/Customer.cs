using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Kunde: läuft zu einem Regal (oder als Sammler zur Vitrine), nimmt Ware, stellt sich an die Kasse und geht wieder.
    /// In der Schlange zeigt er seine Laune: 🙂 → 😐 → 😠, danach geht er.
    /// </summary>
    public class Customer : MonoBehaviour
    {
        public static readonly List<Customer> Active = new List<Customer>();

        enum State { ToShelf, Browsing, ToQueue, Queued, Leaving }

        const float Speed = 2.2f;
        const float BrowseSeconds = 2f;
        /// <summary>Anteil der Kunden, die Einzelkarten aus der Vitrine suchen (wenn eine gefüllt ist).</summary>
        const float CollectorChance = 0.3f;

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
        float patience;

        Showcase showcase;
        int showcaseSlot = -1;
        CardDef card;

        SpriteRenderer spriteRenderer;
        SpriteRenderer moodIcon;

        public int Total => card != null ? unitPrice : itemCount * unitPrice;

        public bool IsWaitingAtRegister => state == State.Queued && queueIndex == 0 && Arrived;

        bool Arrived => path == null || pathIndex >= path.Count;

        static GameManager Game => GameManager.Instance;

        Vector2Int CurrentCell => ShopGrid.WorldToCell(transform.position);

        void OnEnable() => Active.Add(this);

        void OnDisable() => Active.Remove(this);

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            moodIcon = WorldFactory.CreateSprite("Laune", "moodHappy", transform.position + new Vector3(0f, 0.85f, 0f), 50, transform);
            moodIcon.transform.localScale = Vector3.one * 0.6f;
            moodIcon.enabled = false;
            GetComponent<YSort>().Collect();

            var showcases = Game.Showcases.FindAll(s => s.HasCards);
            if (showcases.Count > 0 && Random.value < CollectorChance) PickShowcase(showcases);
            else PickShelf();
        }

        void PickShowcase(List<Showcase> showcases)
        {
            showcase = showcases[Random.Range(0, showcases.Count)];
            if (!WalkNextTo(showcase.Cell)) Leave(false, "Ein Sammler ist gegangen: Er kam nicht an die Vitrine.");
            else state = State.ToShelf;
        }

        void PickShelf()
        {
            // Etwa jeder Vierte schaut nach Zubehör, wenn es welches gibt.
            var accessories = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Accessory);
            var packs = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Pack);
            List<Shelf> withStock = accessories.Count > 0 && (packs.Count == 0 || Random.value < 0.25f) ? accessories : packs;
            var candidates = withStock.Count > 0 ? withStock : Game.Shelves;
            if (candidates.Count == 0)
            {
                Leave(false, "Ein Kunde ist gegangen: Es gibt noch keine Regale.");
                return;
            }

            shelf = candidates[Random.Range(0, candidates.Count)];
            if (!WalkNextTo(shelf.Cell)) Leave(false, "Ein Kunde ist gegangen: Er kam nicht ans Regal.");
            else state = State.ToShelf;
        }

        bool WalkNextTo(Vector2Int target)
        {
            List<Vector2Int> best = null;
            foreach (var spot in Game.Grid.WalkableNeighbours(target))
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
                    if (showcase != null)
                    {
                        BuyFromShowcase();
                        break;
                    }
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
                    // Meist 1–2 Packs (Zubehör: 1 Stück), bei einem Schnäppchen eins mehr.
                    int wanted = product.Category == ProductCategory.Accessory ? 1 : Random.Range(1, 3);
                    itemCount = shelf.Take(wanted + (bargain ? 1 : 0));

                    if (itemCount == 0) Leave(false, "Ein Kunde ist gegangen: Das Regal war leer.");
                    else JoinQueue();
                    break;

                case State.ToQueue:
                case State.Queued:
                    state = State.Queued;
                    timer -= Time.deltaTime;
                    UpdateMood();
                    if (timer <= 0f) Leave(false, "Ein Kunde ist gegangen: Er hat zu lange gewartet.");
                    break;

                case State.Leaving:
                    if (Arrived) Destroy(gameObject);
                    break;
            }
        }

        void BuyFromShowcase()
        {
            var filled = new List<int>();
            for (int i = 0; i < Showcase.SlotCount; i++) if (showcase.Cards[i] >= 0) filled.Add(i);
            if (filled.Count == 0)
            {
                Leave(false, "Ein Sammler ist gegangen: Die Vitrine war leer.");
                return;
            }

            int slot = filled[Random.Range(0, filled.Count)];
            var wanted = CardDatabase.Get(showcase.Cards[slot]);
            int price = showcase.Prices[slot];
            float ratio = price / (float)wanted.Value;
            if (Random.value < Mathf.Clamp01((ratio - 1f) / PainRange))
            {
                Leave(false, $"Ein Sammler fand {wanted.Name} zu teuer ({Money.Format(price)}).");
                return;
            }

            bargain = ratio <= BargainRatio;
            showcaseSlot = slot;
            card = CardDatabase.Get(showcase.Take(slot));
            unitPrice = price;
            JoinQueue();
        }

        void JoinQueue()
        {
            if (!Game.Register.TryJoin(this))
            {
                Leave(false, "Ein Kunde ist gegangen: Die Schlange war zu lang.");
                return;
            }
            state = State.ToQueue;
            patience = Game.CustomerPatience;
            timer = patience;
            moodIcon.enabled = true;
            UpdateMood();
        }

        /// <summary>Laune in drei Stufen: das erste Drittel froh, dann neutral, dann verärgert.</summary>
        void UpdateMood()
        {
            float left = timer / patience;
            string key = left > 2f / 3f ? "moodHappy" : left > 1f / 3f ? "moodNeutral" : "moodAngry";
            moodIcon.sprite = Art.Get(key);
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

        /// <summary>Bezahlt immer den vollen Preis, egal wie die Laune ist.</summary>
        public void Pay()
        {
            if (card != null) Game.RegisterCardSale(card, unitPrice);
            else Game.RegisterSale(Total, itemCount, product);
            itemCount = 0;
            card = null;
            Leave(true, null);
        }

        void Leave(bool happy, string reason)
        {
            if (state == State.Leaving) return;

            Game.Register.Remove(this);
            if (itemCount > 0 && shelf != null) shelf.PutBack(product, itemCount);
            itemCount = 0;
            ReturnCard();
            if (moodIcon != null) moodIcon.enabled = false;

            if (happy) Game.CustomerLeftHappy(bargain);
            else Game.CustomerLeftUnhappy(reason);

            state = State.Leaving;
            if (!WalkTo(ShopGrid.SpawnCell)) Destroy(gameObject);
        }

        /// <summary>Nicht gekaufte Karte zurück in die Vitrine, oder in die Sammlung, falls das Fach inzwischen belegt ist.</summary>
        void ReturnCard()
        {
            if (card == null) return;
            if (showcase != null && showcase.Cards[showcaseSlot] < 0) showcase.Put(showcaseSlot, card, unitPrice);
            else Game.Collection.Add(card.Id);
            card = null;
        }
    }
}
