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
        public int Wages;
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
        /// <summary>Öffnungszeit in Echtzeit: Level 1–2 fünf Minuten, ab Level 3 (Turniere) sieben Minuten.</summary>
        public static float DayLengthFor(int storeLevel) => storeLevel >= 3 ? 420f : 300f;

        /// <summary>Länge des laufenden Tages. Wird beim Öffnen festgelegt, damit ein Level-Aufstieg die Uhr nicht verstellt.</summary>
        public float DayLength { get; private set; } = DayLengthFor(1);

        public static float DayLengthSeconds => Instance != null ? Instance.DayLength : DayLengthFor(1);
        public const int OpenMinute = 9 * 60;
        public const int CloseMinute = 18 * 60;

        static readonly (int points, string name)[] ReputationStages =
        {
            // Grob auf die Kapitel abgestimmt: Geheimtipp in Kapitel 2, Beliebt in 3, Stadtbekannt in 5, Legendär in 7.
            (0, "Unbekannt"), (60, "Geheimtipp"), (250, "Beliebt"), (800, "Stadtbekannt"), (2000, "Legendär"),
        };

        public static GameManager Instance { get; private set; }

        public ShopGrid Grid;
        public CashRegister Register;
        public PlayerController Player;
        public readonly List<Shelf> Shelves = new List<Shelf>();
        public readonly List<Showcase> Showcases = new List<Showcase>();
        public readonly List<DecoItem> Decos = new List<DecoItem>();
        /// <summary>Noch nicht ausgepacktes Möbel-Paket am Eingang (höchstens eins).</summary>
        public FurniturePackage Package;
        public readonly Collection Collection = new Collection();
        public readonly Skills Skills = new Skills();
        public int SkillPoints => Skills.FreePoints(StoreLevel);

        public bool LearnSkill(Skill skill)
        {
            if (!Skills.Learn(skill, StoreLevel)) return false;
            SoundManager.Play(Sfx.Quest);
            ShowToast($"Neue Fähigkeit: {Skills.All[(int)skill].Name}!", 4f);
            return true;
        }

        /// <summary>Lagerraum hinter der linken Wand: Ware, die gerade nicht im Laden steht.</summary>
        public readonly Dictionary<ProductId, int> Storage = new Dictionary<ProductId, int>();
        public bool StorageBuilt { get; private set; }
        public bool StorageOpen;
        public const int StorageCost = 6000;

        public int StorageTotal
        {
            get
            {
                int total = 0;
                foreach (var count in Storage.Values) total += count;
                return total;
            }
        }

        public int StorageCount(ProductId id) => Storage.TryGetValue(id, out int count) ? count : 0;

        public void AddToStorage(Product product, int amount) => Storage[product.Id] = StorageCount(product.Id) + amount;

        /// <summary>Ware aus dem Lager holen: ein Karton mit höchstens <paramref name="max"/> Stück vor der Lagertür.</summary>
        public DeliveryBox TakeFromStorage(Product product, int max)
        {
            int amount = Mathf.Min(max, StorageCount(product.Id));
            if (amount <= 0) return null;
            Storage[product.Id] = StorageCount(product.Id) - amount;
            if (Storage[product.Id] <= 0) Storage.Remove(product.Id);
            return WorldFactory.SpawnBox(ShopGrid.StorageFrontCell, product, amount);
        }

        public void BuildStorage()
        {
            if (StorageBuilt || !Spend(StorageCost)) return;
            StorageBuilt = true;
            WorldFactory.SpawnStorageDoor();
            SoundManager.Play(Sfx.Build);
            ShowToast("Der Lagerraum ist fertig: die Tür in der linken Wand. Kartons mit [E] hineinstellen, mit leeren Händen [E] zum Herausnehmen.", 7f);
        }

        /// <summary>Bewertete Karten, die gerade nicht in einer Vitrine liegen.</summary>
        public readonly List<Slab> Slabs = new List<Slab>();

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

        /// <summary>Story-Szenen, die schon gezeigt wurden (damit jede nur einmal kommt).</summary>
        readonly HashSet<string> seenStories = new HashSet<string>();

        CutscenePlayer cutscenePlayer;
        CutscenePlayer Cutscenes => cutscenePlayer != null ? cutscenePlayer : cutscenePlayer = GetComponent<CutscenePlayer>();

        public bool CutscenePlaying => Cutscenes != null && Cutscenes.IsPlaying;

        Tutorial tutorial;
        public Tutorial Guide => tutorial != null ? tutorial : tutorial = GetComponent<Tutorial>();
        bool TutorialAsking => Guide != null && Guide.Asking;
        public float DayTime { get; private set; }

        /// <summary>Zufalls-Ereignis des heutigen Tages.</summary>
        public DayEvent TodayEvent { get; private set; }
        /// <summary>Uhrzeit (Minuten), zu der die Influencerin kommt; danach -1.</summary>
        public int InfluencerMinute { get; private set; } = -1;
        public DayStats Today { get; private set; } = new DayStats();

        readonly Dictionary<ProductId, int> prices = new Dictionary<ProductId, int>();

        public bool ComputerOpen;
        /// <summary>Esc-Menü im Spiel.</summary>
        public bool PauseOpen;
        public bool BuildMode;
        public bool CollectionOpen;
        /// <summary>Prizepool- bzw. Siegerehrungs-Fenster am Turnier-Counter.</summary>
        public bool PrizeCounterOpen;
        /// <summary>Modellwahl am 3D-Drucker.</summary>
        public Printer3D OpenPrinter;
        /// <summary>Ankauf-Angebot eines Verkäufers an der Kasse.</summary>
        public Customer SellerOffer;
        /// <summary>Kartenwahl an der Grading-Station.</summary>
        public GradingStation OpenGrading;
        /// <summary>Frisch bewertete Karte, deren Note gerade gezeigt wird.</summary>
        public Slab GradeReveal;

        /// <summary>Der Mitarbeiter, wenn einer eingestellt ist.</summary>
        public Employee Staff { get; private set; }

        TournamentManager tournament;
        public TournamentManager Tournament => tournament != null ? tournament : tournament = GetComponent<TournamentManager>();
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
        public event Action<int> PacksSold;
        public event Action<Product, int> ProductSold;
        /// <summary>Ein Pack wurde geöffnet; Wert = Anzahl gezogener Ultra-Seltener.</summary>
        public event Action<int> PackOpened;
        /// <summary>Karte aus der Vitrine verkauft; Wert = Note (0 = nicht bewertet).</summary>
        public event Action<int> CardSold;
        /// <summary>Eine Karte wurde bewertet; Wert = Note.</summary>
        public event Action<int> Graded;
        public event Action ShopExpanded;
        public event Action CityTournamentHeld;
        public event Action TournamentHeld;
        public event Action EmployeeHired;
        public event Action Printed;
        public event Action CardsBought;
        public event Action DecoPlaced;
        /// <summary>Abends bei der Abrechnung, mit den Zahlen des Tages.</summary>
        public event Action<DayStats> DayEnded;

        public bool InputLocked =>
            Phase == GamePhase.MainMenu || Phase == GamePhase.CharacterSelect || Phase == GamePhase.Intro || Phase == GamePhase.Summary ||
            DialogLines != null || ComputerOpen || PauseOpen || BuildMode ||
            CollectionOpen || OpenShowcase != null || OpenedPack != null || LevelUpShown > 0 || CutscenePlaying || PrizeCounterOpen ||
            OpenPrinter != null || SellerOffer != null || OpenGrading != null || GradeReveal != null || TutorialAsking || StorageOpen;

        /// <summary>Menüs, die das Spiel anhalten.</summary>
        bool Paused =>
            DialogLines != null || ComputerOpen || PauseOpen || CollectionOpen || OpenShowcase != null || OpenedPack != null ||
            LevelUpShown > 0 || CutscenePlaying || PrizeCounterOpen || OpenPrinter != null || SellerOffer != null ||
            OpenGrading != null || GradeReveal != null || TutorialAsking || StorageOpen;

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
            (1f + Mathf.Min(0.3f, Coziness * 0.02f)) * (Skills.Has(Skill.Charmer) ? 1.2f : 1f) *
            (TodayEvent == DayEvent.Rain ? 1.3f : 1f);

        /// <summary>Wie oft heute Kunden kommen (kleiner = öfter).</summary>
        public float SpawnIntervalFactor
        {
            get
            {
                if (TodayEvent == DayEvent.Rain) return 1.4f;
                if (TodayEvent == DayEvent.Rush && ClockMinutes >= 13 * 60 && ClockMinutes < 15 * 60) return 0.4f;
                return 1f;
            }
        }

        /// <summary>Einkaufspreis einer Bestellung, mit Lagerprofi-Rabatt auf Ware.</summary>
        public int CostOf(OrderOption option)
        {
            if (option.Kind != OrderKind.ProductBox) return option.Cost;
            float factor = (Skills.Has(Skill.StockPro) ? 0.9f : 1f) * (TodayEvent == DayEvent.Sale ? 0.8f : 1f);
            return Mathf.RoundToInt(option.Cost * factor);
        }

        /// <summary>Kapitel-Aufgaben des aktuellen Levels erledigt? (Kapitel 2–7 folgen.)</summary>
        Chapters chapters;
        public Chapters ChapterProgress => chapters != null ? chapters : chapters = GetComponent<Chapters>();

        public bool ChapterComplete => ChapterProgress.Completed && ChapterProgress.Chapter == StoreLevel;

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
            ShowToast("Neuer Fähigkeitspunkt! Am Computer unter „Talente“ einlösen.", 6f);
        }

        /// <summary>Nur zum Testen: nächstes Level sofort, ohne XP und Kapitel-Aufgaben.</summary>
        public void TestUnlockNextLevel()
        {
            if (StoreLevel >= StoreLevels.Max) return;
            Xp = Mathf.Max(Xp, StoreLevels.XpFor(StoreLevel + 1));
            ShowLevelUp();
        }

        public void CloseLevelUp()
        {
            LevelUpShown = 0;
            SaveGame();
            // Jedes neue Kapitel beginnt mit derselben Titelkarte und neuen Aufgaben.
            ChapterProgress.StartChapter(StoreLevel);
            Cutscenes.PlayTitleCard(StoreLevel, null);
        }

        /// <summary>Alle Aufgaben eines Kapitels erledigt: kurzer Abschluss, dann ein Story-Moment mit Papa.</summary>
        public void ChapterFinished(int chapter)
        {
            string next = chapter < StoreLevels.Max
                ? $"Sobald genug XP da sind, steigt TwoMoons auf Level {chapter + 1}."
                : "TwoMoons ist der beste Laden der Stadt!";
            var cutscene = TwoMoons.Cutscenes.ChapterEnd(chapter);
            Action story = cutscene == null ? (Action)null
                : () => PlayStoryOnce($"kapitel{chapter}Ende", cutscene, chapter == StoreLevels.Max ? FinishStory : (Action)null);
            ShowDialog(new[]
            {
                $"{PlayerName}: Geschafft! Kapitel {chapter} „{TwoMoons.Cutscenes.ChapterTitles[chapter]}“ ist abgeschlossen.",
                next,
            }, story);
        }

        const string TwoMoonsStory = "twoMoonsKarte";

        /// <summary>Die legendäre Karte gehört dir (nach dem Finale).</summary>
        public bool HasTwoMoons => seenStories.Contains(TwoMoonsStory);

        /// <summary>Nach dem Finale: die Two-Moons-Karte ins Sammelbuch, dann geht das Spiel frei weiter.</summary>
        void FinishStory()
        {
            seenStories.Add(TwoMoonsStory);
            SoundManager.Play(Sfx.ShinyCard);
            ChangeReputation(50);
            SaveGame();
            ShowDialog(new[]
            {
                "Die Two-Moons-Karte hat jetzt einen Ehrenplatz im Sammelbuch [C].",
                "TwoMoons bleibt geöffnet: Verkaufe weiter, richte Turniere aus und vervollständige deine Sammlung!",
            });
        }

        /// <summary>Papa steht heute mit an der Kasse (Kapitel 6), bis zum Ladenschluss.</summary>
        public bool PapaHelping { get; private set; }
        GameObject papaHelper;

        public void StartPapaDay()
        {
            PlayStoryOnce("papaHilft", TwoMoons.Cutscenes.PapaHelps, () =>
            {
                if (PapaHelping) return;
                PapaHelping = true;
                var cell = ShopGrid.StaffCells[0] + Vector2Int.left;
                papaHelper = WorldFactory.CreateSprite("Papa", "papa", ShopGrid.CellToWorld(cell), 0).gameObject;
                papaHelper.AddComponent<YSort>();
                ShowToast("Papa kassiert heute mit – bis Ladenschluss.", 5f);
            });
        }

        void EndPapaDay()
        {
            if (!PapaHelping) return;
            PapaHelping = false;
            if (papaHelper != null) Destroy(papaHelper);
            ShowToast("Papa: „Das war der schönste Tag seit Langem. Danke.“", 6f);
        }

        /// <summary>Ein Hinweis, der pro Spielstand nur einmal kommt.</summary>
        public void HintOnce(string id, string text)
        {
            if (IsDemo || !seenStories.Add(id)) return;
            ShowToast(text, 7f);
        }

        /// <summary>Spielt eine Story-Szene genau einmal pro Spielstand.</summary>
        public void PlayStoryOnce(string id, Cutscene cutscene, Action then = null)
        {
            if (!seenStories.Add(id))
            {
                then?.Invoke();
                return;
            }
            Cutscenes.Play(cutscene, then);
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

            if (TutorialAsking)
            {
                // [Esc] bei der Tutorial-Frage = „Nein, danke“.
                if (GameInput.ConsumeCancel()) Guide.Stop();
            }
            else if (OpenedPack != null)
            {
                // Das Pack-Menü kümmert sich selbst um Tasten.
            }
            else if (CollectionOpen || OpenShowcase != null || PrizeCounterOpen || OpenPrinter != null || OpenGrading != null || GradeReveal != null || StorageOpen)
            {
                if (GameInput.ConsumeCancel())
                {
                    StorageOpen = false;
                    CollectionOpen = false;
                    OpenShowcase = null;
                    PrizeCounterOpen = false;
                    OpenPrinter = null;
                    OpenGrading = null;
                    GradeReveal = null;
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

            RefillShowcasesFromStaffCards();

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
            else if (Phase == GamePhase.Closing && Customer.Active.Count == 0 && !Tournament.Busy)
            {
                Phase = GamePhase.Summary;
                EndPapaDay();
                if (Staff != null)
                {
                    MoneyCents -= Employee.DailyWage;
                    Today.Expenses += Employee.DailyWage;
                    Today.Wages += Employee.DailyWage;
                }
                DayEnded?.Invoke(Today);
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
            if (data.package != null && data.package.contents.Count > 0)
            {
                Package = WorldFactory.SpawnPackage(new Vector2Int(data.package.x, data.package.y));
                foreach (var item in data.package.contents) Package.Add((Furniture)item.type, item.count);
            }
            Collection.LoadFrom(data);
            Skills.LoadFrom(data);

            foreach (var price in data.prices) prices[(ProductId)price.product] = price.cents;
            foreach (var saved in data.shelves)
            {
                var shelf = WorldFactory.SpawnShelf(new Vector2Int(saved.x, saved.y), (ProductCategory)saved.category);
                if (saved.mixed != null && saved.mixed.Count > 0)
                    foreach (var item in saved.mixed) shelf.AddStock(Catalog.Get((ProductId)item.type), item.count);
                else if (saved.product >= 0) shelf.SetStock(Catalog.Get((ProductId)saved.product), saved.stock);
            }
            foreach (var box in data.boxes)
                WorldFactory.SpawnBox(new Vector2Int(box.x, box.y), Catalog.Get((ProductId)box.product), box.amount).IsSingle = box.single;
            foreach (var cell in data.dirt) WorldFactory.SpawnDirt(new Vector2Int(cell.x, cell.y));
            foreach (var saved in data.showcases)
            {
                var showcase = WorldFactory.SpawnShowcase(new Vector2Int(saved.x, saved.y), saved.premium);
                for (int i = 0; i < showcase.SlotCount && i < saved.cards.Length; i++)
                {
                    var card = CardDatabase.Get(saved.cards[i]);
                    int grade = saved.grades != null && i < saved.grades.Length ? saved.grades[i] : 0;
                    if (card != null) showcase.Put(i, card, saved.prices[i], grade);
                }
            }
            if (data.slabs != null) Slabs.AddRange(data.slabs.FindAll(slab => slab.Card != null));
            // Passt der gespeicherte Kapitel-Fortschritt nicht zum Level (ältere Spielstände), beginnt das Kapitel neu.
            ChapterProgress.Restore(StoreLevel, data.chapter == StoreLevel ? data.questProgress : null);
            if (data.seenStories != null) foreach (var id in data.seenStories) seenStories.Add(id);
            Tournament.LoadFrom(data.tournament);
            foreach (var saved in data.printers)
            {
                var deco = Decos.Find(d => d.Type == Furniture.Printer3D && d.Cell == new Vector2Int(saved.x, saved.y));
                if (deco != null) deco.GetComponent<Printer3D>().Restore(saved.filament, saved.model, saved.progress, saved.jammed);
            }
            foreach (var saved in data.bins)
            {
                var deco = Decos.Find(d => d.Type == Furniture.TrashBin && d.Cell == new Vector2Int(saved.x, saved.y));
                if (deco != null) deco.GetComponent<TrashBin>().Restore(saved.fill);
            }
            if (data.staffCards != null) StaffCards.AddRange(data.staffCards);
            if (data.storageBuilt)
            {
                StorageBuilt = true;
                WorldFactory.SpawnStorageDoor();
            }
            foreach (var saved in data.storage) AddToStorage(Catalog.Get((ProductId)saved.type), saved.count);
            foreach (var saved in data.tableTrash)
            {
                var deco = Decos.Find(d => d.Type == Furniture.Table && d.Cell == new Vector2Int(saved.x, saved.y));
                if (deco != null) deco.GetComponent<TableTrash>().Restore(saved.fill);
            }
            foreach (var saved in data.graders)
            {
                var deco = Decos.Find(d => d.Type == Furniture.GradingStation && d.Cell == new Vector2Int(saved.x, saved.y));
                if (deco != null) deco.GetComponent<GradingStation>().Restore(saved.card, saved.progress);
            }
            if (data.employee >= 0) Staff = WorldFactory.SpawnEmployee(ShopGrid.StaffCells[0], (Employee.Role)data.employee);

            // Weiter mit der gespeicherten Tageszeit. Kunden, die gerade im Laden waren, kommen nicht mit.
            var phase = (GamePhase)data.phase;
            Phase = phase == GamePhase.Open || phase == GamePhase.Closing || phase == GamePhase.Summary ? phase : GamePhase.Preparation;
            DayLength = DayLengthFor(StoreLevel);
            DayTime = Mathf.Clamp(data.dayTime, 0f, DayLengthSeconds);
            TodayEvent = (DayEvent)data.dayEvent;
            InfluencerMinute = data.influencerMinute;
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
                dayEvent = (int)TodayEvent,
                influencerMinute = InfluencerMinute,
                today = Today,
                chapter = ChapterProgress.Chapter,
                questProgress = ChapterProgress.SaveProgress(),
            };
            foreach (var price in prices) data.prices.Add(new SavedPrice { product = (int)price.Key, cents = price.Value });
            foreach (var shelf in Shelves)
            {
                data.shelves.Add(new SavedShelf
                {
                    x = shelf.Cell.x,
                    y = shelf.Cell.y,
                    stock = shelf.Stock,
                    category = (int)shelf.Category,
                    product = shelf.Product != null ? (int)shelf.Product.Id : -1,
                });
                if (shelf.Mixed)
                    foreach (var item in shelf.Contents)
                        data.shelves[data.shelves.Count - 1].mixed.Add(new SavedCount { type = (int)item.Key.Id, count = item.Value });
            }
            foreach (var box in DeliveryBox.Existing)
            {
                // Ein getragener Karton wird dort gespeichert, wo die Figur gerade steht.
                var cell = ShopGrid.WorldToCell(box.transform.position);
                if (!box.enabled) cell = ShopGrid.WorldToCell(Player.transform.position);
                data.boxes.Add(new SavedBox { x = cell.x, y = cell.y, product = (int)box.Product.Id, amount = box.Amount, single = box.IsSingle });
            }
            Collection.SaveTo(data);
            Skills.SaveTo(data);
            data.seenStories.AddRange(seenStories);
            data.tournament = Tournament.Save();
            foreach (var deco in Decos)
            {
                if (deco.Type != Furniture.Printer3D) continue;
                var printer = deco.GetComponent<Printer3D>();
                data.printers.Add(new SavedPrinter
                {
                    x = deco.Cell.x, y = deco.Cell.y, filament = printer.Filament,
                    model = printer.ModelIndex, progress = printer.Progress, jammed = printer.Jammed,
                });
            }
            foreach (var deco in Decos)
            {
                if (deco.Type != Furniture.GradingStation) continue;
                var grader = deco.GetComponent<GradingStation>();
                data.graders.Add(new SavedGrader { x = deco.Cell.x, y = deco.Cell.y, card = grader.CardId, progress = grader.Progress });
            }
            foreach (var deco in Decos)
                if (deco.Type == Furniture.TrashBin)
                    data.bins.Add(new SavedBin { x = deco.Cell.x, y = deco.Cell.y, fill = deco.GetComponent<TrashBin>().Fill });
            foreach (var deco in Decos)
                if (deco.Type == Furniture.Table && deco.GetComponent<TableTrash>().Count > 0)
                    data.tableTrash.Add(new SavedBin { x = deco.Cell.x, y = deco.Cell.y, fill = deco.GetComponent<TableTrash>().Count });
            data.staffCards.AddRange(StaffCards);
            data.storageBuilt = StorageBuilt;
            foreach (var pair in Storage)
                if (pair.Value > 0) data.storage.Add(new SavedCount { type = (int)pair.Key, count = pair.Value });
            data.expanded = ShopGrid.Expanded;
            data.slabs.AddRange(Slabs);
            data.employee = Staff != null ? (int)Staff.Job : -1;
            foreach (var pair in toPlace)
                if (pair.Value > 0) data.toPlace.Add(new SavedCount { type = (int)pair.Key, count = pair.Value });
            foreach (var deco in Decos) data.decos.Add(new SavedDeco { type = (int)deco.Type, x = deco.Cell.x, y = deco.Cell.y });
            if (Package != null)
            {
                var cell = ShopGrid.WorldToCell(Package.transform.position);
                data.package = new SavedPackage { x = cell.x, y = cell.y };
                foreach (var item in Package.Contents)
                    data.package.contents.Add(new SavedCount { type = (int)item.Key, count = item.Value });
            }
            foreach (var showcase in Showcases)
            {
                data.showcases.Add(new SavedShowcase
                {
                    x = showcase.Cell.x,
                    y = showcase.Cell.y,
                    cards = (int[])showcase.Cards.Clone(),
                    prices = (int[])showcase.Prices.Clone(),
                    grades = (int[])showcase.Grades.Clone(),
                    premium = showcase.Premium,
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
            Cutscenes.Play(TwoMoons.Cutscenes.Intro, FinishIntro);
        }

        /// <summary>Nach der Intro-Geschichte (oder beim Überspringen): erster Tag im leeren Laden.</summary>
        public void FinishIntro()
        {
            if (Phase != GamePhase.Intro) return;
            Phase = GamePhase.Preparation;
            SaveGame();
            Cutscenes.PlayTitleCard(1, () => ShowDialog(new[]
            {
                $"{PlayerName}: Mein eigener Laden. TwoMoons. Papa, ich mach dich stolz.",
                "Und eines Tages finde ich die Two-Moons-Karte. Für dich.",
                "Viel Geld habe ich nicht. Aber es ist ein Anfang: ein Karton Monstera-Packs und Papas altes Regal.",
                "Erst mal aufräumen. Dann das Regal aufstellen, Packs einräumen und das Schild an der Tür umdrehen!",
            }, () => Guide?.Offer()));
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
            DayLength = DayLengthFor(StoreLevel);
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
            int cost = CostOf(option);
            if (MoneyCents < cost)
            {
                ShowToast("Dafür reicht das Geld nicht.");
                SoundManager.Play(Sfx.Error);
                return false;
            }
            MoneyCents -= cost;
            Today.Expenses += cost;
            SoundManager.Play(Sfx.Order);

            if (option.Kind == OrderKind.Furniture)
            {
                // Alle gekauften Möbel kommen ins selbe Paket am Eingang.
                if (Package == null) Package = WorldFactory.SpawnPackage(WorldFactory.FreeDeliveryCell());
                Package.Add(option.Furniture);
                ShowToast($"{option.Name} ist im Paket am Eingang. Dort mit [E] auspacken und aufstellen.");
            }
            else
            {
                WorldFactory.SpawnBox(WorldFactory.FreeDeliveryCell(), Catalog.Get(option.Product), option.Amount);
                ShowToast($"{option.Name} geliefert! Der Karton steht am Eingang.");
            }
            return true;
        }

        public void RegisterSale(int cents, int count, Product product)
        {
            if (IsDemo) return;
            MoneyCents += cents;
            Today.Income += cents;
            if (product.Category == ProductCategory.Pack) Today.PacksSold += count;
            Today.CustomersServed++;
            ShowToast($"+{Money.Format(cents)}", 2f);
            SoundManager.Play(Sfx.Register);
            AddXp(cents / 100);
            if (product.Category == ProductCategory.Pack) PacksSold?.Invoke(count);
            ProductSold?.Invoke(product, count);
            CustomerServed?.Invoke();
        }

        /// <summary>Preisspanne je Ware: von einem Viertel bis zum 2,5-fachen Marktwert, in 10-Cent-Schritten.</summary>
        public static int MinPriceFor(Product product) => Mathf.Max(10, Mathf.RoundToInt(product.MarketValue * 0.25f / 10f) * 10);

        public static int MaxPriceFor(Product product) => Mathf.Max(100, Mathf.RoundToInt(product.MarketValue * 2.5f / 10f) * 10);

        /// <summary>Verkaufspreis pro Stück. Ohne eigene Einstellung gilt der Marktwert.</summary>
        public int GetPrice(Product product) =>
            prices.TryGetValue(product.Id, out int cents) ? cents : product.MarketValue;

        public void SetPrice(Product product, int cents) =>
            prices[product.Id] = Mathf.Clamp(cents, MinPriceFor(product), MaxPriceFor(product));

        /// <summary>Einzelkarte aus der Vitrine verkauft (<paramref name="grade"/> 0 = nicht bewertet).</summary>
        public void RegisterCardSale(CardDef card, int cents, int grade)
        {
            if (IsDemo) return;
            MoneyCents += cents;
            Today.Income += cents;
            Today.CardsSold++;
            Today.CustomersServed++;
            ShowToast($"{card.Name}{(grade > 0 ? $" (Note {grade})" : "")} verkauft: +{Money.Format(cents)}", 3f);
            SoundManager.Play(Sfx.Register);
            AddXp(cents / 100);
            CardSold?.Invoke(grade);
            CustomerServed?.Invoke();
        }

        /// <summary>Geld ausgeben (z. B. fürs Grading). False, wenn es nicht reicht.</summary>
        public bool Spend(int cents)
        {
            if (MoneyCents < cents)
            {
                ShowToast("Dafür reicht das Geld nicht.");
                SoundManager.Play(Sfx.Error);
                return false;
            }
            MoneyCents -= cents;
            Today.Expenses += cents;
            return true;
        }

        /// <summary>Bewertete Karte aufnehmen. Frisch aus der Grading-Station mit großer Enthüllung.</summary>
        public void AddSlab(Slab slab, bool fresh)
        {
            Slabs.Add(slab);
            if (!fresh) return;
            Collection.RecordGrade(slab.card, slab.grade);
            GradeReveal = slab;
            if (slab.grade >= 10) SoundManager.Play(slab.Card.Rarity == Rarity.Ultra ? Sfx.Fanfare : Sfx.ShinyCard);
            else SoundManager.Play(slab.grade >= 8 ? Sfx.RareCard : Sfx.CardFlip);
            if (slab.grade >= 10 && slab.Card.Rarity == Rarity.Ultra) SoundManager.Play(Sfx.ShinyCard);
            Graded?.Invoke(slab.grade);
        }

        public const int ExtensionCost = 100000;
        public const int ExtensionLevel = 5;

        /// <summary>
        /// Ladenerweiterung: der Laden wird nach rechts breiter. Er wird gespeichert und größer neu aufgebaut,
        /// deshalb nur morgens, solange noch keine Kunden da sind.
        /// </summary>
        public void BuildExtension()
        {
            if (ShopGrid.Expanded || StoreLevel < ExtensionLevel || Phase != GamePhase.Preparation || !Spend(ExtensionCost)) return;
            ComputerOpen = false;
            var data = CreateSaveData();
            data.expanded = true;
            GameBootstrap.Reload(CurrentSlot, data, game => game.FinishExtension());
        }

        void FinishExtension()
        {
            Toasts.Clear();
            SoundManager.Play(Sfx.Fanfare);
            ShopExpanded?.Invoke();
            SaveGame();
            ShowDialog(new[]
            {
                "Die Handwerker sind fertig: Die rechte Wand ist weiter nach außen gerückt – TwoMoons ist jetzt viel größer!",
                $"{PlayerName}: So viel Platz! Hier passen neue Regale, Vitrinen und eine richtige Spielecke rein.",
            });
        }

        /// <summary>Ein Pack öffnen: Karten ziehen und sofort in die Sammlung legen.</summary>
        /// <summary>Welches Pack gerade geöffnet wird (für Bild und Titel).</summary>
        public Product OpenedPackProduct { get; private set; }

        public void OpenPack(Product product)
        {
            OpenedPackProduct = product;
            OpenedPack = CardDatabase.RollPack(product.Game);
            if (Skills.Has(Skill.Lucky)) SwapInMissingCards(OpenedPack);
            OpenedPackNew.Clear();
            foreach (var card in OpenedPack) OpenedPackNew.Add(Collection.Add(card.Id));
            PackOpened?.Invoke(OpenedPack.FindAll(c => c.Rarity == Rarity.Ultra).Count);
        }

        public void ClosePack() => OpenedPack = null;

        /// <summary>Glückspilz: bekannte Karten werden manchmal gegen noch fehlende derselben Seltenheit getauscht.</summary>
        void SwapInMissingCards(List<CardDef> pack)
        {
            for (int i = 0; i < pack.Count; i++)
            {
                var card = pack[i];
                if (!Collection.IsDiscovered(card.Id) || UnityEngine.Random.value > 0.25f) continue;
                var missing = CardDatabase.ForGame(card.Game).FindAll(c => c.Rarity == card.Rarity && !Collection.IsDiscovered(c.Id) && !pack.Contains(c));
                if (missing.Count > 0) pack[i] = missing[UnityEngine.Random.Range(0, missing.Count)];
            }
        }

        public void CustomerLeftHappy(bool bargain)
        {
            if (IsDemo) return;
            ChangeReputation(bargain ? 2 : 1);
            AddXp(5);
        }

        public void CustomerLeftUnhappy(string reason)
        {
            if (IsDemo) return;
            Today.CustomersUnhappy++;
            ChangeReputation(-1);
            if (reason != null) ShowToast(reason);
        }

        /// <summary>Die Influencerin ist da – ab jetzt kommt sie heute nicht mehr.</summary>
        public void InfluencerArrived() => InfluencerMinute = -1;

        public void InfluencerLeft(bool happy)
        {
            if (IsDemo) return;
            if (happy)
            {
                ChangeReputation(20);
                AddXp(40);
                ShowToast("Die Influencerin postet ein Video über TwoMoons: „Bester Kartenladen der Stadt!“ Ruf +20", 6f);
                SoundManager.Play(Sfx.Fanfare);
            }
            else
            {
                ChangeReputation(-10);
                ShowToast("Die Influencerin war enttäuscht und erzählt es ihren Followern … Ruf −10", 6f);
            }
        }

        /// <summary>Ladendieb erwischt: Ware zurück, ein bisschen Ruf für die Aufmerksamkeit.</summary>
        public void ThiefCaught()
        {
            if (IsDemo) return;
            ChangeReputation(2);
            AddXp(15);
            ShowToast("Ladendieb erwischt! Die Ware ist zurück im Regal. Ruf +2", 5f);
            SoundManager.Play(Sfx.Quest);
            ThiefStopped?.Invoke();
        }

        public void ThiefEscaped(Product product, int count)
        {
            if (IsDemo) return;
            ShowToast($"Ein Ladendieb ist mit {count}× {product.Name} entkommen!", 5f);
            SoundManager.Play(Sfx.Error);
        }

        public event Action ThiefStopped;
        public event Action TrashTakenOut;
        public event Action TableCleaned;

        public void NotifyTableCleaned() => TableCleaned?.Invoke();

        /// <summary>Voller Müllsack im Container: ordentlicher Laden, etwas Ruf.</summary>
        public void TrashDisposed()
        {
            if (IsDemo) return;
            ChangeReputation(1);
            AddXp(10);
            ShowToast("Müll entsorgt – der Laden bleibt sauber. Ruf +1", 3f);
            TrashTakenOut?.Invoke();
        }

        public void ChangeReputation(int delta)
        {
            int before = ReputationStage;
            Reputation = Mathf.Max(0, Reputation + delta);
            if (ReputationStage <= before) return;
            ShowToast($"Dein Laden ist jetzt „{ReputationName}“!", 5f);
            SoundManager.Play(Sfx.Quest);
        }

        /// <summary>Nach der Abrechnung beginnt ein neuer Tag – nach dem allerersten Tag ruft Papa an.</summary>
        public void Sleep()
        {
            if (Day == 1) PlayStoryOnce("ersterAbend", TwoMoons.Cutscenes.FirstEvening, StartNextDay);
            else StartNextDay();
        }

        void StartNextDay()
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

            TodayEvent = DailyEvents.Roll();
            InfluencerMinute = TodayEvent == DayEvent.Influencer ? UnityEngine.Random.Range(10 * 60, 16 * 60) : -1;
            if (TodayEvent != DayEvent.None)
            {
                ShowDialog(new[] { $"Heute: {DailyEvents.Title(TodayEvent)}", DailyEvents.Description(TodayEvent) });
                SoundManager.Play(Sfx.Quest);
            }

            int dirt = Skills.Has(Skill.Tidy) ? UnityEngine.Random.Range(0, 2) : UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < dirt; i++) WorldFactory.SpawnDirtSomewhere();

            Tournament.OnNewDay();

            // Automatisch speichern, jeden Morgen nach dem Schlafen.
            SaveGame();
        }

        public void NotifyDirtCleaned() => DirtCleaned?.Invoke();

        public void NotifyShelfPlaced() => ShelfPlaced?.Invoke();

        public void NotifyDecoPlaced() => DecoPlaced?.Invoke();

        public void NotifyTournamentHeld() => TournamentHeld?.Invoke();

        public void NotifyPrinted() => Printed?.Invoke();

        public void NotifyCityTournament() => CityTournamentHeld?.Invoke();

        /// <summary>Ein Kunde hat am Arcade-Automaten gespielt: 1 € pro Runde.</summary>
        public void RegisterArcade(int rounds)
        {
            if (IsDemo) return;
            int cents = rounds * 100;
            MoneyCents += cents;
            Today.Income += cents;
            ShowToast($"Arcade: {rounds} Runden, +{Money.Format(cents)}", 2f);
            SoundManager.Play(Sfx.Register);
            AddXp(rounds);
        }

        /// <summary>Ankauf: Karten vom Verkäufer übernehmen und bezahlen.</summary>
        public void BuyCards(List<CardDef> cards, int price)
        {
            MoneyCents -= price;
            Today.Expenses += price;
            foreach (var card in cards) Collection.Add(card.Id);
            ShowToast($"{cards.Count} Karten angekauft für {Money.Format(price)}.", 4f);
            SoundManager.Play(Sfx.Order);
            CardsBought?.Invoke();
        }

        /// <summary>Vom Mitarbeiter angekaufte Karten, die noch auf ein freies Vitrinen-Fach warten (liegen im Sammelbuch).</summary>
        public readonly List<int> StaffCards = new List<int>();
        float staffShowcaseTimer;

        /// <summary>Wird ein Vitrinen-Fach frei, legt der Mitarbeiter die nächste seiner angekauften Karten hinein.</summary>
        void RefillShowcasesFromStaffCards()
        {
            if (Staff == null || StaffCards.Count == 0 || IsDemo) return;
            staffShowcaseTimer -= Time.deltaTime;
            if (staffShowcaseTimer > 0f) return;
            staffShowcaseTimer = 3f;

            var showcase = Showcases.Find(s => System.Array.IndexOf(s.Cards, -1) >= 0);
            if (showcase == null) return;
            // Karten, die du inzwischen selbst verwendet hast, fallen aus der Liste.
            while (StaffCards.Count > 0)
            {
                int id = StaffCards[0];
                StaffCards.RemoveAt(0);
                var card = CardDatabase.Get(id);
                if (card == null || !Collection.Remove(id)) continue;
                showcase.Put(System.Array.IndexOf(showcase.Cards, -1), card, card.Value);
                ShowToast($"{Employee.Name} legt {card.Name} in die Vitrine.", 3f);
                return;
            }
        }

        /// <summary>
        /// Der Mitarbeiter an der Kasse kauft Karten an: erst ein Gegenangebot (−15 %), sonst der verlangte Preis.
        /// Die Karten kommen gleich in freie Vitrinen-Fächer (zum Marktwert); der Rest kommt nach und nach hinein,
        /// sobald wieder ein Fach frei ist.
        /// </summary>
        public void EmployeeBuysCards(Customer seller)
        {
            var cards = new List<CardDef>(seller.OfferCards);
            bool haggled = seller.TryCounterOffer();
            int price = seller.AskingPrice;
            if (MoneyCents < price)
            {
                ShowToast($"{Employee.Name}: „Dafür reicht das Geld in der Kasse nicht.“ Ankauf abgelehnt.", 4f);
                seller.ResolveOffer(false);
                return;
            }
            seller.ResolveOffer(true);

            int placed = 0;
            foreach (var card in cards)
            {
                var showcase = Showcases.Find(s => System.Array.IndexOf(s.Cards, -1) >= 0);
                if (showcase == null)
                {
                    StaffCards.Add(card.Id);
                    continue;
                }
                if (!Collection.Remove(card.Id)) continue;
                showcase.Put(System.Array.IndexOf(showcase.Cards, -1), card, card.Value);
                placed++;
            }
            ShowToast($"{Employee.Name} hat {cards.Count} Karten für {Money.Format(price)} angekauft" +
                      (haggled ? " (−15 % gehandelt)" : "") +
                      (placed > 0 ? $" und {placed} in die Vitrine gelegt." : "."), 5f);
        }

        public void Hire(Employee.Role role)
        {
            if (Staff != null)
            {
                Staff.Job = role;
                ShowToast($"{Employee.Name} kümmert sich jetzt um: {(role == Employee.Role.Register ? "die Kasse" : "die Regale")}");
                return;
            }
            if (StoreLevel < 3) return;
            Staff = WorldFactory.SpawnEmployee(ShopGrid.StaffCells[0], role);
            ShowToast($"{Employee.Name} arbeitet jetzt bei TwoMoons! Lohn: {Money.Format(Employee.DailyWage)} pro Tag.", 5f);
            SoundManager.Play(Sfx.Quest);
            EmployeeHired?.Invoke();
        }

        public void Fire()
        {
            if (Staff == null) return;
            Destroy(Staff.gameObject);
            Staff = null;
            ShowToast($"{Employee.Name} arbeitet nicht mehr bei dir.");
        }

        /// <summary>Startgeld eines Turnierspielers an der Kasse.</summary>
        public void RegisterEntryFee()
        {
            MoneyCents += TournamentManager.EntryFee;
            Today.Income += TournamentManager.EntryFee;
            ShowToast($"Turnier-Anmeldung: +{Money.Format(TournamentManager.EntryFee)}", 2f);
            SoundManager.Play(Sfx.Register);
            AddXp(TournamentManager.EntryFee / 100);
        }

        public void NotifyShelfStocked(int packs) => ShelfStocked?.Invoke(packs);
    }
}
