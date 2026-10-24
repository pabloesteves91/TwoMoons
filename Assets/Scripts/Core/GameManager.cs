using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public enum GamePhase
    {
        /// <summary>Hauptmenü, im Hintergrund läuft der Laden von selbst.</summary>
        MainMenu,
        CharacterSelect,
        /// <summary>Morgens, Laden noch zu: putzen, einräumen, bestellen.</summary>
        Preparation,
        Open,
        /// <summary>18 Uhr vorbei, die letzten Kunden werden noch bedient.</summary>
        Closing,
        Summary,
        /// <summary>Die Geschichte vor dem ersten Tag (steht am Ende, damit gespeicherte Phasen gleich bleiben).</summary>
        Intro,
    }

    [Serializable]
    public class DayStats
    {
        public int Income;
        public int Expenses;
        public int PacksSold;
        public int CustomersServed;
        public int CustomersUnhappy;
        public int CardsSold;
    }

    public class Toast
    {
        public string Text;
        public float Until;
    }

    /// <summary>Spielstand und Tagesablauf.</summary>
    public class GameManager : MonoBehaviour
    {
        public const int StartMoney = 6000;
        public const float DayLengthSeconds = 420f;
        public const int OpenMinute = 9 * 60;
        public const int CloseMinute = 18 * 60;

        static readonly (int points, string name)[] ReputationStages =
        {
            (0, "Unbekannt"), (20, "Geheimtipp"), (60, "Beliebt"), (150, "Stadtbekannt"), (300, "Legendär"),
        };

        public static GameManager Instance { get; private set; }

        public ShopGrid Grid;
        public CashRegister Register;
        public PlayerController Player;
        public readonly List<Shelf> Shelves = new List<Shelf>();
        public readonly List<Showcase> Showcases = new List<Showcase>();
        public readonly List<DecoItem> Decos = new List<DecoItem>();
        public readonly Collection Collection = new Collection();

        public GamePhase Phase { get; private set; } = GamePhase.MainMenu;
        /// <summary>Speicherplatz des laufenden Spiels (0–2).</summary>
        public int CurrentSlot { get; private set; } = -1;
        public int Figure { get; private set; }
        public string PlayerName { get; private set; } = "";
        public int Day { get; private set; } = 1;
        public int MoneyCents { get; private set; } = StartMoney;
        public int Reputation { get; private set; }
        /// <summary>Gekaufte Möbel, die noch aufgestellt werden müssen.</summary>
        readonly Dictionary<Furniture, int> toPlace = new Dictionary<Furniture, int>();

        public int StoreLevel { get; private set; } = 1;
        /// <summary>Laden-Erfahrung insgesamt.</summary>
        public int Xp { get; private set; }
        /// <summary>Gerade angezeigter Level-Up-Bildschirm (0 = keiner).</summary>
        public int LevelUpShown { get; private set; }
        public float DayTime { get; private set; }
        public DayStats Today { get; private set; } = new DayStats();

        readonly Dictionary<ProductId, int> prices = new Dictionary<ProductId, int>();

        public bool ComputerOpen;
        /// <summary>Esc-Menü im Spiel.</summary>
        public bool PauseOpen;
        public bool BuildMode;
        public bool CollectionOpen;
        /// <summary>Vitrine, deren Menü gerade offen ist.</summary>
        public Showcase OpenShowcase;

        /// <summary>Karten des gerade geöffneten Packs (beste zuletzt), null wenn keins offen ist.</summary>
        public List<CardDef> OpenedPack { get; private set; }
        /// <summary>Welche Karten im geöffneten Pack zum ersten Mal gezogen wurden.</summary>
        public List<bool> OpenedPackNew { get; } = new List<bool>();

        public string[] DialogLines { get; private set; }
        public int DialogIndex { get; private set; }
        public int DialogClosedFrame { get; private set; } = -1;
        int dialogOpenedFrame = -1;
        Action dialogDone;

        public readonly List<Toast> Toasts = new List<Toast>();

        public event Action DirtCleaned;
        public event Action ShelfPlaced;
        public event Action<int> ShelfStocked;
        public event Action ShopOpened;
        public event Action CustomerServed;

        public bool InputLocked =>
            Phase == GamePhase.MainMenu || Phase == GamePhase.CharacterSelect || Phase == GamePhase.Intro || Phase == GamePhase.Summary ||
            DialogLines != null || ComputerOpen || PauseOpen || BuildMode ||
            CollectionOpen || OpenShowcase != null || OpenedPack != null || LevelUpShown > 0;

        /// <summary>Menüs, die das Spiel anhalten.</summary>
        bool Paused =>
            DialogLines != null || ComputerOpen || PauseOpen || CollectionOpen || OpenShowcase != null || OpenedPack != null ||
            LevelUpShown > 0;

        public int ToPlace(Furniture furniture) => toPlace.TryGetValue(furniture, out int count) ? count : 0;

        public int TotalToPlace
        {
            get
            {
                int total = 0;
                foreach (var count in toPlace.Values) total += count;
                return total;
            }
        }

        public void AddToPlace(Furniture furniture, int amount = 1) => toPlace[furniture] = ToPlace(furniture) + amount;

        /// <summary>Summe der Gemütlichkeit aller Deko, abzüglich Flecken.</summary>
        public int Coziness
        {
            get
            {
                int cozy = 0;
                foreach (var deco in Decos) cozy += FurnitureCatalog.Get(deco.Type).Cozy;
                return Mathf.Max(0, cozy - DirtSpot.Existing.Count);
            }
        }

        /// <summary>
        /// Wie lange ein Kunde in der Schlange wartet (alle drei Launen zusammen).
        /// Mit besserem Ruf geduldiger, Schmutz im Laden macht ungeduldig.
        /// </summary>
        public float CustomerPatience =>
            (15f + 5f * ReputationStage) * Mathf.Max(0.5f, 1f - 0.08f * DirtSpot.Existing.Count) *
            (1f + Mathf.Min(0.3f, Coziness * 0.02f));

        /// <summary>Kapitel-Aufgaben des aktuellen Levels erledigt? (Kapitel 2–7 folgen.)</summary>
        public bool ChapterComplete => StoreLevel == 1 && GetComponent<ChapterOne>().Completed;

        public bool CanLevelUp => StoreLevel < StoreLevels.Max && Xp >= StoreLevels.XpFor(StoreLevel + 1) && ChapterComplete;

        public void AddXp(int amount)
        {
            if (IsDemo || amount <= 0) return;
            Xp += amount;
        }

        void ShowLevelUp()
        {
            StoreLevel++;
            LevelUpShown = StoreLevel;
            SoundManager.Play(Sfx.Fanfare);
        }

        public void CloseLevelUp()
        {
            LevelUpShown = 0;
            SaveGame();
        }

        bool IsDemo => Phase == GamePhase.MainMenu;

        public int ClockMinutes =>
            Phase == GamePhase.Preparation ? OpenMinute - 60 :
            OpenMinute + Mathf.FloorToInt((CloseMinute - OpenMinute) * Mathf.Clamp01(DayTime / DayLengthSeconds));

        public string ClockText => $"{ClockMinutes / 60:00}:{ClockMinutes % 60:00}";

        public int ReputationStage
        {
            get
            {
                int stage = 0;
                for (int i = 0; i < ReputationStages.Length; i++)
                    if (Reputation >= ReputationStages[i].points) stage = i;
                return stage;
            }
        }

        public string ReputationName => ReputationStages[ReputationStage].name;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        void Update()
        {
            Toasts.RemoveAll(t => t.Until < Time.unscaledTime);

            // Dialoge und Menüs halten das Spiel an.
            Time.timeScale = Paused ? 0f : 1f;

            if (IsDemo)
            {
                // Im Hauptmenü werden die Regale heimlich wieder aufgefüllt.
                foreach (var shelf in Shelves)
                    if (shelf.Stock < 8) shelf.SetStock(Catalog.MonsteraPack, Shelf.Capacity);
                return;
            }

            if (DialogLines != null)
            {
                if (GameInput.ConfirmPressed && Time.frameCount != dialogOpenedFrame) AdvanceDialog();
                return;
            }

            if (LevelUpShown > 0) return;
            if (CanLevelUp && !Paused && !BuildMode && Phase != GamePhase.Intro && Phase != GamePhase.CharacterSelect)
            {
                ShowLevelUp();
                return;
            }

            if (OpenedPack != null)
            {
                // Das Pack-Menü kümmert sich selbst um Tasten.
            }
            else if (CollectionOpen || OpenShowcase != null)
            {
                if (GameInput.ConsumeCancel())
                {
                    CollectionOpen = false;
                    OpenShowcase = null;
                }
            }
            else if (ComputerOpen)
            {
                if (GameInput.ConsumeCancel()) ComputerOpen = false;
            }
            else if (!InputLocked && GameInput.CollectionPressed)
            {
                CollectionOpen = true;
                SoundManager.Play(Sfx.UiClick);
            }
            else if (PauseOpen)
            {
                if (GameInput.ConsumeCancel()) PauseOpen = false;
            }
            else if (!InputLocked && GameInput.ConsumeCancel())
            {
                PauseOpen = true;
                SoundManager.Play(Sfx.UiClick);
            }

            if (Phase == GamePhase.Open)
            {
                DayTime += Time.deltaTime;
                if (DayTime >= DayLengthSeconds)
                {
                    Phase = GamePhase.Closing;
                    ShowToast("18:00 – Ladenschluss! Die letzten Kunden werden noch bedient.");
                    SoundManager.Play(Sfx.DayEnd);
                }
            }
            else if (Phase == GamePhase.Closing && Customer.Active.Count == 0)
            {
                Phase = GamePhase.Summary;
                ComputerOpen = false;
                PauseOpen = false;
                BuildMode = false;
            }
        }

        public void InitMainMenu()
        {
            Phase = GamePhase.MainMenu;
            GameBootstrap.SetUpDemoWorld(this);
        }

        public void InitNewGame(int slot)
        {
            Phase = GamePhase.CharacterSelect;
            CurrentSlot = slot;
            GameBootstrap.SetUpNewGameWorld(this);
        }

        public void InitFromSave(int slot, SaveData data)
        {
            CurrentSlot = slot;
            PlayerName = data.playerName;
            Figure = data.figure;
            Player.SetFigure(Figure);
            Day = Mathf.Max(1, data.day);
            MoneyCents = data.money;
            Reputation = data.reputation;
            StoreLevel = Mathf.Clamp(data.storeLevel, 1, StoreLevels.Max);
            Xp = data.xp;
            // Ältere Spielstände kannten nur Regale und Vitrinen.
            AddToPlace(Furniture.Shelf, data.shelvesToPlace);
            AddToPlace(Furniture.Showcase, data.showcasesToPlace);
            foreach (var saved in data.toPlace) AddToPlace((Furniture)saved.type, saved.count);
            foreach (var saved in data.decos) WorldFactory.SpawnDeco((Furniture)saved.type, new Vector2Int(saved.x, saved.y));
            Collection.LoadFrom(data);

            foreach (var price in data.prices) prices[(ProductId)price.product] = price.cents;
            foreach (var saved in data.shelves)
            {
                var shelf = WorldFactory.SpawnShelf(new Vector2Int(saved.x, saved.y));
                if (saved.product >= 0) shelf.SetStock(Catalog.Get((ProductId)saved.product), saved.stock);
            }
            foreach (var box in data.boxes)
                WorldFactory.SpawnBox(new Vector2Int(box.x, box.y), Catalog.Get((ProductId)box.product), box.amount);
            foreach (var cell in data.dirt) WorldFactory.SpawnDirt(new Vector2Int(cell.x, cell.y));
            foreach (var saved in data.showcases)
            {
                var showcase = WorldFactory.SpawnShowcase(new Vector2Int(saved.x, saved.y));
                for (int i = 0; i < Showcase.SlotCount && i < saved.cards.Length; i++)
                {
                    var card = CardDatabase.Get(saved.cards[i]);
                    if (card != null) showcase.Put(i, card, saved.prices[i]);
                }
            }
            GetComponent<ChapterOne>().RestoreProgress(data.questProgress);

            // Weiter mit der gespeicherten Tageszeit. Kunden, die gerade im Laden waren, kommen nicht mit.
            var phase = (GamePhase)data.phase;
            Phase = phase == GamePhase.Open || phase == GamePhase.Closing || phase == GamePhase.Summary ? phase : GamePhase.Preparation;
            DayTime = Mathf.Clamp(data.dayTime, 0f, DayLengthSeconds);
            if (data.today != null) Today = data.today;
            ShowToast($"Willkommen zurück, {PlayerName}! Tag {Day}, {ClockText}.");
        }

        SaveData CreateSaveData()
        {
            var data = new SaveData
            {
                playerName = PlayerName,
                figure = Figure,
                day = Day,
                money = MoneyCents,
                reputation = Reputation,
                storeLevel = StoreLevel,
                xp = Xp,
                phase = (int)Phase,
                dayTime = DayTime,
                today = Today,
                questProgress = GetComponent<ChapterOne>().SaveProgress(),
            };
            foreach (var price in prices) data.prices.Add(new SavedPrice { product = (int)price.Key, cents = price.Value });
            foreach (var shelf in Shelves)
            {
                data.shelves.Add(new SavedShelf
                {
                    x = shelf.Cell.x,
                    y = shelf.Cell.y,
                    stock = shelf.Stock,
                    product = shelf.Product != null ? (int)shelf.Product.Id : -1,
                });
            }
            foreach (var box in DeliveryBox.Existing)
            {
                // Ein getragener Karton wird dort gespeichert, wo die Figur gerade steht.
                var cell = ShopGrid.WorldToCell(box.transform.position);
                if (!box.enabled) cell = ShopGrid.WorldToCell(Player.transform.position);
                data.boxes.Add(new SavedBox { x = cell.x, y = cell.y, product = (int)box.Product.Id, amount = box.Amount });
            }
            Collection.SaveTo(data);
            foreach (var pair in toPlace)
                if (pair.Value > 0) data.toPlace.Add(new SavedCount { type = (int)pair.Key, count = pair.Value });
            foreach (var deco in Decos) data.decos.Add(new SavedDeco { type = (int)deco.Type, x = deco.Cell.x, y = deco.Cell.y });
            foreach (var showcase in Showcases)
            {
                data.showcases.Add(new SavedShowcase
                {
                    x = showcase.Cell.x,
                    y = showcase.Cell.y,
                    cards = (int[])showcase.Cards.Clone(),
                    prices = (int[])showcase.Prices.Clone(),
                });
            }
            foreach (var dirt in DirtSpot.Existing)
            {
                var cell = ShopGrid.WorldToCell(dirt.transform.position);
                data.dirt.Add(new SavedCell { x = cell.x, y = cell.y });
            }
            return data;
        }

        public void SaveGame()
        {
            if (CurrentSlot < 0 || Phase == GamePhase.MainMenu || Phase == GamePhase.CharacterSelect || Phase == GamePhase.Intro) return;
            try
            {
                SaveSystem.Save(CurrentSlot, CreateSaveData());
                ShowToast($"Gespeichert (Platz {CurrentSlot + 1}).");
            }
            catch (Exception e)
            {
                Debug.LogError($"Speichern fehlgeschlagen: {e}");
                ShowToast("Speichern hat nicht geklappt.");
                SoundManager.Play(Sfx.Error);
            }
        }

        public void StartGame(string playerName, int figure)
        {
            PlayerName = playerName.Trim();
            Figure = figure;
            Player.SetFigure(figure);
            Phase = GamePhase.Intro;
        }

        /// <summary>Nach der Intro-Geschichte (oder beim Überspringen): erster Tag im leeren Laden.</summary>
        public void FinishIntro()
        {
            if (Phase != GamePhase.Intro) return;
            Phase = GamePhase.Preparation;
            SaveGame();
            ShowDialog(new[]
            {
                $"{PlayerName}: Mein eigener Laden. TwoMoons. Papa, ich mach dich stolz.",
                "Und eines Tages finde ich die Two-Moons-Karte. Für dich.",
                "Viel Geld habe ich nicht. Aber es ist ein Anfang: ein Karton Monstera-Packs und Papas altes Regal.",
                "Erst mal aufräumen. Dann das Regal aufstellen, Packs einräumen und das Schild an der Tür umdrehen!",
            });
        }

        public void ShowDialog(string[] lines, Action onDone = null)
        {
            DialogLines = lines;
            DialogIndex = 0;
            dialogOpenedFrame = Time.frameCount;
            dialogDone = onDone;
        }

        void AdvanceDialog()
        {
            SoundManager.Play(Sfx.Dialog);
            DialogIndex++;
            if (DialogIndex < DialogLines.Length) return;
            DialogLines = null;
            DialogClosedFrame = Time.frameCount;
            var done = dialogDone;
            dialogDone = null;
            done?.Invoke();
        }

        public void ShowToast(string text, float seconds = 4f) =>
            Toasts.Add(new Toast { Text = text, Until = Time.unscaledTime + seconds });

        public void OpenShop()
        {
            if (Phase != GamePhase.Preparation) return;
            Phase = GamePhase.Open;
            DayTime = 0f;
            ShowToast("Der Laden ist geöffnet!");
            SoundManager.Play(Sfx.Doorbell);
            ShopOpened?.Invoke();
        }

        /// <summary>Bestellung bezahlen. Die Ware wird sofort am Eingang geliefert.</summary>
        public bool Order(OrderOption option)
        {
            if (option.MinLevel > StoreLevel)
            {
                ShowToast($"{option.Name} gibt es erst ab Store-Level {option.MinLevel}.");
                SoundManager.Play(Sfx.Error);
                return false;
            }
            if (MoneyCents < option.Cost)
            {
                ShowToast("Dafür reicht das Geld nicht.");
                SoundManager.Play(Sfx.Error);
                return false;
            }
            MoneyCents -= option.Cost;
            Today.Expenses += option.Cost;
            SoundManager.Play(Sfx.Order);

            if (option.Kind == OrderKind.Furniture)
            {
                AddToPlace(option.Furniture);
                ShowToast($"{option.Name} geliefert! Drücke [B], um es aufzustellen.");
            }
            else
            {
                WorldFactory.SpawnBox(WorldFactory.FreeDeliveryCell(), Catalog.Get(option.Product), option.Amount);
                ShowToast($"{option.Name} geliefert! Der Karton steht am Eingang.");
            }
            return true;
        }

        public void RegisterSale(int cents, int packs)
        {
            if (IsDemo) return;
            MoneyCents += cents;
            Today.Income += cents;
            Today.PacksSold += packs;
            Today.CustomersServed++;
            ShowToast($"+{Money.Format(cents)}", 2f);
            SoundManager.Play(Sfx.Register);
            AddXp(cents / 100);
            CustomerServed?.Invoke();
        }

        public const int MinPrice = 100;
        public const int MaxPrice = 1000;

        /// <summary>Verkaufspreis pro Stück. Ohne eigene Einstellung gilt der Marktwert.</summary>
        public int GetPrice(Product product) =>
            prices.TryGetValue(product.Id, out int cents) ? cents : product.MarketValue;

        public void SetPrice(Product product, int cents) =>
            prices[product.Id] = Mathf.Clamp(cents, MinPrice, MaxPrice);

        /// <summary>Schnäppchen freuen die Kunden besonders.</summary>
        public void RegisterCardSale(CardDef card, int cents)
        {
            if (IsDemo) return;
            MoneyCents += cents;
            Today.Income += cents;
            Today.CardsSold++;
            Today.CustomersServed++;
            ShowToast($"{card.Name} verkauft: +{Money.Format(cents)}", 3f);
            SoundManager.Play(Sfx.Register);
            AddXp(cents / 100);
            CustomerServed?.Invoke();
        }

        /// <summary>Ein Pack öffnen: Karten ziehen und sofort in die Sammlung legen.</summary>
        public void OpenPack()
        {
            OpenedPack = CardDatabase.RollPack();
            OpenedPackNew.Clear();
            foreach (var card in OpenedPack) OpenedPackNew.Add(Collection.Add(card.Id));
        }

        public void ClosePack() => OpenedPack = null;

        public void CustomerLeftHappy(bool bargain)
        {
            if (IsDemo) return;
            ChangeReputation(bargain ? 3 : 2);
            AddXp(5);
        }

        public void CustomerLeftUnhappy(string reason)
        {
            if (IsDemo) return;
            Today.CustomersUnhappy++;
            ChangeReputation(-1);
            if (reason != null) ShowToast(reason);
        }

        public void ChangeReputation(int delta)
        {
            int before = ReputationStage;
            Reputation = Mathf.Max(0, Reputation + delta);
            if (ReputationStage <= before) return;
            ShowToast($"Dein Laden ist jetzt „{ReputationName}“!", 5f);
            SoundManager.Play(Sfx.Quest);
        }

        /// <summary>Nach der Abrechnung beginnt ein neuer Tag.</summary>
        public void Sleep()
        {
            Day++;
            Today = new DayStats();
            DayTime = 0f;
            Phase = GamePhase.Preparation;
            if (TotalToPlace > 0) ShowToast("Es stehen noch Möbel zum Aufstellen bereit – drücke [B].");

            // Ein gemütlicher Laden spricht sich herum.
            int cozyBonus = Coziness / 5;
            if (cozyBonus > 0)
            {
                ChangeReputation(cozyBonus);
                ShowToast($"Dein gemütlicher Laden spricht sich herum: Ruf +{cozyBonus}");
            }

            int dirt = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < dirt; i++) WorldFactory.SpawnDirtSomewhere();

            // Automatisch speichern, jeden Morgen nach dem Schlafen.
            SaveGame();
        }

        public void NotifyDirtCleaned() => DirtCleaned?.Invoke();

        public void NotifyShelfPlaced() => ShelfPlaced?.Invoke();

        public void NotifyShelfStocked(int packs) => ShelfStocked?.Invoke(packs);
    }
}
