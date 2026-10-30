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

        enum State { ToShelf, Browsing, ToQueue, Queued, Leaving, ToCouch, Gaming, Sneaking, ToBin }

        const float Speed = 2.2f;
        const float BrowseSeconds = 2f;
        /// <summary>Anteil der Kunden, die Einzelkarten aus der Vitrine suchen (wenn eine gefüllt ist).</summary>
        const float CollectorChance = 0.3f;
        /// <summary>So oft lässt ein Kunde beim Gehen etwas fallen oder verschüttet etwas (tagsüber).</summary>
        const float MessChance = 0.12f;
        /// <summary>Höchstens so viele Flecken auf einmal – sonst wird es unspielbar.</summary>
        const int MaxDirt = 8;

        /// <summary>Anteil der Kunden, die nach dem Bezahlen noch eine Runde auf der Couch zocken.</summary>
        const float CouchChance = 0.35f;
        /// <summary>Anteil der Pack-Käufer, die sich an einen Tisch setzen, ihre Booster öffnen und tauschen.</summary>
        const float TableChance = 0.45f;
        /// <summary>So oft lässt ein Kunde seine Folien einfach auf dem Tisch liegen.</summary>
        const float LitterChance = 0.3f;

        static readonly string[] PullLines = { "Ooh, eine Seltene!", "Schon wieder die Mooskröte …", "Endlich! Die fehlte mir!", "Glänzt die etwa?!", "Nicht schlecht!" };
        static readonly string[] TradeLines = { "Tauschst du die?", "Deal!", "Zwei gegen eine?", "Die hab ich doppelt!", "Abgemacht!" };

        /// <summary>Folie der geöffneten Booster, die in den Mülleimer muss.</summary>
        bool carriesTrash;
        bool saidPull, saidTrade;
        float seatTime;
        TrashBin bin;

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
        int cardGrade;

        SpriteRenderer spriteRenderer;
        SpriteRenderer moodIcon;

        public int Total => card != null ? unitPrice : itemCount * unitPrice;

        /// <summary>Was der Kunde gerade sagt (Sprechblase über dem Kopf), null = nichts.</summary>
        public string Speech => Time.time < speechUntil ? speech : null;
        string speech;
        float speechUntil;
        bool complained;
        /// <summary>Wie oft der Kunde schon ein anderes Regal gesucht hat, weil eins leer war.</summary>
        int shelfRetries;

        static readonly string[] AngryLines = { "Geht das heute noch?", "Ich warte schon ewig!", "Hallo?! Kasse!", "Wird das noch was?" };

        public void Say(string text, float seconds = 3f)
        {
            speech = text;
            speechUntil = Time.time + seconds;
        }

        /// <summary>Kommt nur, um sich fürs Turnier anzumelden (zahlt Startgeld an der Kasse).</summary>
        public bool IsTournamentEntrant;

        /// <summary>Ladendieb: steckt Packs ein und schleicht zum Ausgang. Mit [E] erwischen!</summary>
        public bool IsThief;
        public bool IsSneaking => state == State.Sneaking;
        /// <summary>Influencerin (Tagesereignis): gut bedient gibt es viel Ruf, sonst Ärger.</summary>
        public bool IsInfluencer;
        bool influencerReported;
        const float SneakSpeed = 1.3f;

        /// <summary>Will dir Karten verkaufen (Ankauf ab Level 4).</summary>
        public bool IsSeller;
        public readonly List<CardDef> OfferCards = new List<CardDef>();
        public int AskingPrice { get; private set; }
        public bool CounterTried { get; private set; }

        public Sprite Sprite => spriteRenderer != null ? spriteRenderer.sprite : GetComponent<SpriteRenderer>().sprite;

        public bool IsWaitingAtRegister => state == State.Queued && queueIndex == 0 && Arrived;

        /// <summary>Sitzt gerade auf dieser Couch (für „Couch frei?“).</summary>
        public DecoItem Couch { get; private set; }
        bool hasGamed;

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

            if (IsThief) gameObject.AddComponent<ThiefCatch>().Init(this);
            if (IsInfluencer)
            {
                spriteRenderer.color = new Color(1f, 0.75f, 0.95f);
                Say("Hey Leute, ich bin heute bei TwoMoons!", 4f);
            }

            if (IsTournamentEntrant)
            {
                unitPrice = TournamentManager.EntryFee;
                itemCount = 1;
                JoinQueue();
                return;
            }

            if (IsSeller)
            {
                PrepareOffer();
                JoinQueue();
                return;
            }

            // Bewertete Karten locken mehr Sammler an.
            var showcases = Game.Showcases.FindAll(s => s.HasCards);
            float collectorChance = CollectorChance + (showcases.Exists(s => s.HasGradedCards) ? 0.1f : 0f) +
                                    (Game.TodayEvent == DayEvent.Collectors ? 0.2f : 0f);
            if (!IsThief && showcases.Count > 0 && Random.value < collectorChance) PickShowcase(showcases);
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
            // Meist Boosterpacks; etwa jeder Vierte schaut nach Zubehör, jeder Sechste nach Getränken.
            var packs = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Pack);
            var accessories = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Accessory);
            var drinks = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Drink);
            var prints = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Print);
            float roll = Random.value;
            List<Shelf> withStock = packs;
            if (accessories.Count > 0 && (roll < 0.25f || packs.Count == 0)) withStock = accessories;
            else if (prints.Count > 0 && roll < 0.38f) withStock = prints;
            else if (drinks.Count > 0 && (roll > 0.83f || packs.Count == 0)) withStock = drinks;
            var candidates = withStock.Count > 0 ? withStock : Game.Shelves.FindAll(s => s.Category == ProductCategory.Pack);
            if (candidates.Count == 0)
            {
                Leave(false, "Ein Kunde ist gegangen: Es gibt noch keine Regale.");
                return;
            }

            shelf = candidates[Random.Range(0, candidates.Count)];
            if (!WalkNextTo(shelf.Cell)) Leave(false, "Ein Kunde ist gegangen: Er kam nicht ans Regal.");
            else state = State.ToShelf;
        }

        /// <summary>
        /// Das Regal ist leer: erst in einem anderen Regal derselben Art schauen (z. B. das zweite Pack-Regal),
        /// sonst in irgendeinem gefüllten Pack-Regal. Gibt false zurück, wenn es keins gibt.
        /// </summary>
        bool TryOtherShelf()
        {
            if (shelfRetries >= 2) return false;
            var category = shelf != null ? shelf.Category : ProductCategory.Pack;
            var others = Game.Shelves.FindAll(s => s != shelf && s.Stock > 0 && s.Category == category);
            if (others.Count == 0) others = Game.Shelves.FindAll(s => s != shelf && s.Stock > 0 && s.Category == ProductCategory.Pack);
            others.Sort((a, b) => (a.Cell - CurrentCell).sqrMagnitude.CompareTo((b.Cell - CurrentCell).sqrMagnitude));
            foreach (var other in others)
            {
                if (!WalkNextTo(other.Cell)) continue;
                shelfRetries++;
                shelf = other;
                product = null;
                state = State.ToShelf;
                Say("Leer … mal da drüben schauen.", 2f);
                return true;
            }
            return false;
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
                    product = shelf != null && shelf.Stock > 0 ? shelf.PickProduct() : null;
                    if (product == null && TryOtherShelf()) break;
                    if (product == null)
                    {
                        Say("Leer?! Schade …");
                        Leave(false, "Ein Kunde ist gegangen: Das Regal war leer.");
                        break;
                    }

                    // Preis mit dem Marktwert vergleichen.
                    unitPrice = Game.GetPrice(product);
                    float ratio = unitPrice / (float)product.MarketValue;
                    if (!IsThief && Random.value < Mathf.Clamp01((ratio - 1f) / PainRange))
                    {
                        Say($"{Money.Format(unitPrice)}? Viel zu teuer!");
                        Leave(false, $"Ein Kunde fand {product.Name} zu teuer ({Money.Format(unitPrice)}).");
                        break;
                    }
                    bargain = ratio <= BargainRatio;
                    // Meist 1–2 Packs (Zubehör: 1 Stück), bei einem Schnäppchen eins mehr.
                    int wanted = product.Category == ProductCategory.Accessory || product.Category == ProductCategory.Print ||
                                 product.Category == ProductCategory.BoardGame ? 1 : Random.Range(1, 3);
                    // Nach dem Zocken: nur ein Getränk.
                    if (hasGamed) wanted = 1;
                    else if (Game.TodayEvent == DayEvent.Collectors) wanted++;
                    if (IsThief) wanted = Random.Range(1, 4);
                    itemCount = shelf.Take(product, wanted + (bargain && !IsThief ? 1 : 0));

                    if (itemCount == 0 && TryOtherShelf()) break;
                    if (itemCount == 0) Leave(false, "Ein Kunde ist gegangen: Das Regal war leer.");
                    else if (IsThief) StartSneaking();
                    else JoinQueue();
                    break;

                case State.Sneaking:
                    // Erst kurz umschauen, dann langsam Richtung Ausgang.
                    if (timer > 0f)
                    {
                        timer -= Time.deltaTime;
                        if (timer <= 0f && !WalkTo(ShopGrid.SpawnCell)) Destroy(gameObject);
                        break;
                    }
                    if (Arrived)
                    {
                        Game.ThiefEscaped(product, itemCount);
                        itemCount = 0;
                        Destroy(gameObject);
                    }
                    break;

                case State.ToQueue:
                case State.Queued:
                    state = State.Queued;
                    timer -= Time.deltaTime;
                    UpdateMood();
                    if (timer <= 0f)
                    {
                        Say("Mir reicht's, ich gehe!");
                        Leave(false, "Ein Kunde ist gegangen: Er hat zu lange gewartet.");
                    }
                    break;

                case State.Leaving:
                    if (Arrived) Destroy(gameObject);
                    break;

                case State.ToCouch:
                    if (!Arrived) break;
                    // Auf Couch oder Stuhl setzen (am Arcade-Automaten davor stehen bleiben).
                    if (Couch.Type == Furniture.Couch || Couch.Type == Furniture.Chair)
                        transform.position = ShopGrid.CellToWorld(DecoCell(Couch)) + new Vector3(0f, 0.1f, 0f);
                    // Am Tisch zum Tisch schauen, damit sich die Gäste gegenübersitzen.
                    var facingTable = Couch.Type == Furniture.Chair ? TableNextTo(Couch.Cell) : null;
                    if (facingTable != null && facingTable.Cell.x != Couch.Cell.x) spriteRenderer.flipX = facingTable.Cell.x < Couch.Cell.x;
                    state = State.Gaming;
                    bool atTable = Couch.Type == Furniture.Chair;
                    timer = atTable ? Random.Range(14f, 22f) : Random.Range(20f, 40f);
                    seatTime = timer;
                    moodIcon.enabled = !atTable;
                    moodIcon.sprite = Art.Get("moodHappy");
                    if (atTable) Say("*ratsch* … mal sehen!", 2.5f);
                    break;

                case State.Gaming:
                    timer -= Time.deltaTime;
                    if (Couch != null && Couch.Type == Furniture.Chair) TableTalk();
                    else moodIcon.enabled = Mathf.Repeat(timer, 3f) < 1.5f;
                    if (timer > 0f) break;
                    FinishGaming();
                    break;

                case State.ToBin:
                    if (!Arrived) break;
                    ThrowAwayTrash();
                    break;
            }
        }

        void BuyFromShowcase()
        {
            var filled = new List<int>();
            for (int i = 0; i < showcase.SlotCount; i++) if (showcase.Cards[i] >= 0) filled.Add(i);
            if (filled.Count == 0)
            {
                Leave(false, "Ein Sammler ist gegangen: Die Vitrine war leer.");
                return;
            }

            // Sammler greifen bevorzugt zu bewerteten Karten.
            var graded = filled.FindAll(i => showcase.Grades[i] > 0);
            if (graded.Count > 0 && Random.value < 0.7f) filled = graded;

            int slot = filled[Random.Range(0, filled.Count)];
            var wanted = CardDatabase.Get(showcase.Cards[slot]);
            int price = showcase.Prices[slot];
            float fairValue = showcase.ValueAt(slot) * (showcase.Premium ? Showcase.PremiumBonus : 1f) *
                              (Game.Skills.Has(Skill.Negotiator) ? 1.1f : 1f);
            float ratio = price / fairValue;
            if (Random.value < Mathf.Clamp01((ratio - 1f) / PainRange))
            {
                Say($"{Money.Format(price)} für {wanted.Name}? Nein danke!");
                Leave(false, $"Ein Sammler fand {wanted.Name} zu teuer ({Money.Format(price)}).");
                return;
            }

            bargain = ratio <= BargainRatio;
            showcaseSlot = slot;
            cardGrade = showcase.Grades[slot];
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
            // Beim roten Emoji beschwert sich der Kunde laut – bis er dran ist.
            if (key == "moodAngry" && !complained)
            {
                complained = true;
                Say(AngryLines[Random.Range(0, AngryLines.Length)], 60f);
            }
        }

        void MoveAlongPath()
        {
            if (Arrived) return;
            Vector3 target = ShopGrid.CellToWorld(path[pathIndex]);
            Vector3 step = target - transform.position;
            if (Mathf.Abs(step.x) > 0.01f) spriteRenderer.flipX = step.x < 0;
            float speed = state == State.Sneaking ? SneakSpeed : Speed;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude < 0.0001f) pathIndex++;
        }

        static Vector2Int DecoCell(DecoItem deco) => deco.Cell;

        /// <summary>3–5 Karten aus seiner Sammlung, meist Gewöhnliche, manchmal etwas Seltenes.</summary>
        void PrepareOffer()
        {
            int count = Random.Range(3, 6);
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                var game = CardDatabase.Games[Random.Range(0, CardDatabase.Games.Length)];
                float roll = Random.value;
                var rarity = roll < 0.08f ? Rarity.Ultra : roll < 0.38f ? Rarity.Rare : Rarity.Common;
                var options = CardDatabase.ForGame(game).FindAll(c => c.Rarity == rarity);
                var card = options[Random.Range(0, options.Count)];
                OfferCards.Add(card);
                value += card.Value;
            }
            float discount = Game.Skills.Has(Skill.Negotiator) ? 0.85f : 1f;
            AskingPrice = Mathf.RoundToInt(value * Random.Range(0.6f, 0.9f) * discount / 10f) * 10;
        }

        /// <summary>Gegenangebot (15 % weniger). Gibt true zurück, wenn der Verkäufer annimmt.</summary>
        public bool TryCounterOffer()
        {
            CounterTried = true;
            if (Random.value > 0.6f) return false;
            AskingPrice = Mathf.RoundToInt(AskingPrice * 0.85f / 10f) * 10;
            return true;
        }

        /// <summary>Ankauf abgeschlossen oder abgelehnt – der Verkäufer geht.</summary>
        public void ResolveOffer(bool bought)
        {
            itemCount = 0;
            if (bought)
            {
                Game.BuyCards(OfferCards, AskingPrice);
                Leave(true, null);
            }
            else
            {
                Leave(true, null, alreadyRewarded: true);
            }
        }

        /// <summary>
        /// Freier Platz zum Zocken: eine Couch, wenn die Couch-Ecke komplett ist (Couch + Fernseher + Konsole),
        /// oder ein Arcade-Automat.
        /// </summary>
        static DecoItem FreeGamingSpot()
        {
            var decos = Game.Decos;
            bool couchCorner = decos.Exists(d => d.Type == Furniture.Tv) && decos.Exists(d => d.Type == Furniture.Console);
            var free = decos.FindAll(deco =>
                ((deco.Type == Furniture.Couch && couchCorner) || deco.Type == Furniture.Arcade) && !deco.InUse);
            return free.Count > 0 ? free[Random.Range(0, free.Count)] : null;
        }

        /// <summary>Nach dem Zocken: glücklich, Ruf-Bonus, und gern noch ein Getränk.</summary>
        void FinishGaming()
        {
            var seat = Couch;
            Couch = null;
            moodIcon.enabled = false;
            if (seat.Type == Furniture.Arcade) Game.RegisterArcade(Random.Range(2, 6));
            else transform.position = ShopGrid.CellToWorld(DecoCell(seat));
            Game.ChangeReputation(1);
            if (seat.Type == Furniture.Chair)
            {
                // Am Tisch wurden Booster geöffnet: Folie in den Mülleimer – oder einfach liegen lassen.
                carriesTrash = true;
                var table = TableNextTo(seat.Cell);
                if (table != null && Random.value < LitterChance)
                {
                    carriesTrash = false;
                    table.GetComponent<TableTrash>().Add();
                    Say("*lässt die Folie liegen*", 2.5f);
                    Game.HintOnce("ersterTischMuell", "Ein Kunde hat seinen Müll auf dem Tisch liegen lassen. Mit [E] einsammeln und in den Mülleimer werfen!");
                    state = State.Queued;
                    Leave(true, null, alreadyRewarded: true);
                    return;
                }
                GoToBin();
                return;
            }
            var drinks = Game.Shelves.FindAll(s => s.Stock > 0 && s.Category == ProductCategory.Drink);
            if (drinks.Count > 0 && Random.value < 0.7f)
            {
                shelf = drinks[Random.Range(0, drinks.Count)];
                product = null;
                if (WalkNextTo(shelf.Cell))
                {
                    state = State.ToShelf;
                    return;
                }
            }
            state = State.Queued; // nur, damit Leave() nicht übersprungen wird
            Leave(true, null, alreadyRewarded: true);
        }

        /// <summary>Bezahlt immer den vollen Preis, egal wie die Laune ist.</summary>
        public void Pay()
        {
            if (IsSeller)
            {
                // Statt zu bezahlen zeigt der Verkäufer sein Angebot.
                Game.SellerOffer = this;
                return;
            }
            if (IsTournamentEntrant)
            {
                // Startgeld bezahlt: weiter an den Spieltisch.
                Game.RegisterEntryFee();
                Game.Register.Remove(this);
                Game.Tournament.Register(this);
                Destroy(gameObject);
                return;
            }

            // Wer sich beschwert hat, ist jetzt erleichtert; die Sprechblase verschwindet bald.
            if (complained) Say("Na endlich!", 2f);
            bool boughtPacks = card == null && product != null && product.Category == ProductCategory.Pack && itemCount > 0;
            if (card != null) Game.RegisterCardSale(card, unitPrice, cardGrade);
            else Game.RegisterSale(Total, itemCount, product);
            itemCount = 0;
            card = null;

            // Pack-Käufer setzen sich gern an einen Tisch, sonst ab und zu Couch oder Arcade-Automat.
            var couch = !hasGamed && boughtPacks && Random.value < TableChance ? FreeChair() : null;
            if (couch == null) couch = !hasGamed && Random.value < CouchChance ? FreeGamingSpot() : null;
            if (couch != null)
            {
                Game.Register.Remove(this);
                Game.CustomerLeftHappy(bargain);
                ReportInfluencer(true);
                hasGamed = true;
                Couch = couch;
                moodIcon.enabled = false;
                if (WalkNextTo(couch.Cell))
                {
                    state = State.ToCouch;
                    return;
                }
                Couch = null;
                state = State.Queued;
                Leave(true, null, alreadyRewarded: true);
                return;
            }
            Leave(true, null);
        }

        void Leave(bool happy, string reason, bool alreadyRewarded = false)
        {
            if (state == State.Leaving) return;
            Couch = null;

            Game.Register.Remove(this);
            if (itemCount > 0 && shelf != null) shelf.PutBack(product, itemCount);
            itemCount = 0;
            ReturnCard();
            if (moodIcon != null) moodIcon.enabled = false;

            if (!alreadyRewarded)
            {
                if (happy) Game.CustomerLeftHappy(bargain);
                else Game.CustomerLeftUnhappy(reason);
                ReportInfluencer(happy);
            }

            MaybeMakeMess();
            state = State.Leaving;
            if (!WalkTo(ShopGrid.SpawnCell)) Destroy(gameObject);
        }

        /// <summary>Freier Stuhl direkt neben einem Tisch.</summary>
        static DecoItem FreeChair()
        {
            // An einen vermüllten Tisch setzt sich keiner.
            var free = Game.Decos.FindAll(d =>
            {
                if (d.Type != Furniture.Chair || d.InUse) return false;
                var table = TableNextTo(d.Cell);
                return table != null && table.GetComponent<TableTrash>().Count < TableTrash.TooMessy;
            });
            return free.Count > 0 ? free[Random.Range(0, free.Count)] : null;
        }

        static DecoItem TableNextTo(Vector2Int cell) =>
            Game.Decos.Find(d => d.Type == Furniture.Table && Mathf.Abs(d.Cell.x - cell.x) + Mathf.Abs(d.Cell.y - cell.y) == 1);

        /// <summary>Am Tisch: erst Booster öffnen und kommentieren, dann mit anderen am selben Tisch tauschen.</summary>
        void TableTalk()
        {
            if (!saidPull && timer < seatTime * 0.65f)
            {
                saidPull = true;
                Say(PullLines[Random.Range(0, PullLines.Length)], 2.5f);
            }
            if (!saidTrade && timer < seatTime * 0.35f)
            {
                saidTrade = true;
                var table = TableNextTo(Couch.Cell);
                bool partner = table != null && Active.Exists(c => c != this && c.Couch != null && c.Couch.Type == Furniture.Chair && TableNextTo(c.Couch.Cell) == table);
                if (partner)
                {
                    Say(TradeLines[Random.Range(0, TradeLines.Length)], 2.5f);
                    Game.ChangeReputation(1);
                }
            }
        }

        /// <summary>Zum nächsten Mülleimer mit Platz (sonst zum nächsten überhaupt).</summary>
        void GoToBin()
        {
            var bins = new List<TrashBin>();
            foreach (var deco in Game.Decos)
                if (deco.Type == Furniture.TrashBin) bins.Add(deco.GetComponent<TrashBin>());
            bins.Sort((a, b) => (a.IsFull ? 1 : 0).CompareTo(b.IsFull ? 1 : 0) != 0
                ? (a.IsFull ? 1 : 0).CompareTo(b.IsFull ? 1 : 0)
                : (a.Cell - CurrentCell).sqrMagnitude.CompareTo((b.Cell - CurrentCell).sqrMagnitude));
            foreach (var candidate in bins)
            {
                if (!WalkNextTo(candidate.Cell)) continue;
                bin = candidate;
                state = State.ToBin;
                return;
            }
            // Kein Mülleimer: die Folie landet auf dem Boden.
            DropTrash("Wo ist denn hier ein Mülleimer?");
            state = State.Queued;
            Leave(true, null, alreadyRewarded: true);
        }

        void ThrowAwayTrash()
        {
            if (bin != null && bin.Add())
            {
                carriesTrash = false;
                SoundManager.Play(Sfx.Drop);
            }
            else DropTrash("Mülleimer voll …");
            state = State.Queued;
            Leave(true, null, alreadyRewarded: true);
        }

        void DropTrash(string complaint)
        {
            if (!carriesTrash) return;
            carriesTrash = false;
            Say(complaint, 2.5f);
            var cell = CurrentCell;
            if (ShopGrid.IsInterior(cell) && !DirtSpot.Existing.Exists(d => ShopGrid.WorldToCell(d.transform.position) == cell))
                WorldFactory.SpawnDirt(cell);
        }

        void ReportInfluencer(bool happy)
        {
            if (!IsInfluencer || influencerReported) return;
            influencerReported = true;
            Say(happy ? "Daumen hoch für TwoMoons!" : "Hmm … das war nichts.", 3f);
            Game.InfluencerLeft(happy);
        }

        /// <summary>Packs eingesteckt: pfeifend umschauen, dann ohne zu bezahlen zum Ausgang.</summary>
        void StartSneaking()
        {
            state = State.Sneaking;
            timer = Random.Range(2.5f, 4f);
            Say("*pfeif* …", 4f);
            Game.HintOnce("ersterDieb", "Psst! Jemand hat gerade Packs eingesteckt und schleicht zur Tür. Lauf hin und drück [E]!");
        }

        /// <summary>Vom Spieler erwischt: Ware zurück ins Regal, peinlich berührt raus.</summary>
        public void Caught()
        {
            if (state != State.Sneaking) return;
            if (itemCount > 0 && shelf != null) shelf.PutBack(product, itemCount);
            itemCount = 0;
            Say("Ertappt … tut mir leid!", 3f);
            Game.ThiefCaught();
            state = State.Queued;
            Leave(true, null, alreadyRewarded: true);
        }

        /// <summary>Ab und zu hinterlässt ein Kunde einen Fleck (Krümel, verschüttete Limo …), den man aufwischen muss.</summary>
        void MaybeMakeMess()
        {
            var phase = Game.Phase;
            if (phase != GamePhase.Open && phase != GamePhase.Closing) return;
            if (DirtSpot.Existing.Count >= MaxDirt) return;
            // Mit Getränk oder nach dem Zocken kleckert man öfter; Ordnungsfanatiker halbiert das.
            float chance = MessChance * (hasGamed ? 2f : 1f) * (Game.Skills.Has(Skill.Tidy) ? 0.5f : 1f);
            if (Random.value > chance) return;

            var cell = ShopGrid.WorldToCell(transform.position);
            if (!ShopGrid.IsInterior(cell) || !Game.Grid.IsWalkable(cell)) return;
            if (DirtSpot.Existing.Exists(d => ShopGrid.WorldToCell(d.transform.position) == cell)) return;
            WorldFactory.SpawnDirt(cell);
            if (speech == null || Time.time >= speechUntil) Say("Ups …", 2f);
            Game.ShowToast("Ein Kunde hat gekleckert – wisch den Fleck auf ([E]), sonst werden alle ungeduldiger.", 4f);
        }

        /// <summary>Nicht gekaufte Karte zurück in die Vitrine, oder in die Sammlung, falls das Fach inzwischen belegt ist.</summary>
        void ReturnCard()
        {
            if (card == null) return;
            if (showcase != null && showcase.Cards[showcaseSlot] < 0) showcase.Put(showcaseSlot, card, unitPrice, cardGrade);
            else if (cardGrade > 0) Game.AddSlab(new Slab { card = card.Id, grade = cardGrade }, false);
            else Game.Collection.Add(card.Id);
            card = null;
        }
    }
}
