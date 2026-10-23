using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    public enum GamePhase
    {
        CharacterSelect,
        /// <summary>Morgens, Laden noch zu: putzen, einräumen, bestellen.</summary>
        Preparation,
        Open,
        /// <summary>18 Uhr vorbei, die letzten Kunden werden noch bedient.</summary>
        Closing,
        Summary,
    }

    public class DayStats
    {
        public int Income;
        public int Expenses;
        public int PacksSold;
        public int CustomersServed;
        public int CustomersUnhappy;
    }

    public class Toast
    {
        public string Text;
        public float Until;
    }

    /// <summary>Spielstand und Tagesablauf.</summary>
    public class GameManager : MonoBehaviour
    {
        public const int StartMoney = 10000;
        public const float DayLengthSeconds = 600f;
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

        public GamePhase Phase { get; private set; } = GamePhase.CharacterSelect;
        public string PlayerName { get; private set; } = "";
        public int Day { get; private set; } = 1;
        public int MoneyCents { get; private set; } = StartMoney;
        public int Reputation { get; private set; }
        public int ShelvesToPlace;
        public float DayTime { get; private set; }
        public DayStats Today { get; private set; } = new DayStats();

        readonly Dictionary<ProductId, int> prices = new Dictionary<ProductId, int>();

        public bool ComputerOpen;
        public bool SettingsOpen;
        public bool BuildMode;

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
            Phase == GamePhase.CharacterSelect || Phase == GamePhase.Summary ||
            DialogLines != null || ComputerOpen || SettingsOpen || BuildMode;

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

            // Dialoge, Bestellmenü und Einstellungen halten das Spiel an.
            Time.timeScale = DialogLines != null || ComputerOpen || SettingsOpen ? 0f : 1f;

            if (DialogLines != null)
            {
                if (GameInput.ConfirmPressed && Time.frameCount != dialogOpenedFrame) AdvanceDialog();
                return;
            }

            if (ComputerOpen)
            {
                if (GameInput.ConsumeCancel()) ComputerOpen = false;
            }
            else if (SettingsOpen)
            {
                if (GameInput.ConsumeCancel()) SettingsOpen = false;
            }
            else if (!InputLocked && GameInput.ConsumeCancel())
            {
                SettingsOpen = true;
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
                SettingsOpen = false;
                BuildMode = false;
            }
        }

        public void StartGame(string playerName, int figure)
        {
            PlayerName = playerName.Trim();
            Player.SetFigure(figure);
            Phase = GamePhase.Preparation;
            ShowDialog(new[]
            {
                $"{PlayerName}: Da ist er also… mein eigener Laden. TwoMoons.",
                "Seit ich klein bin, träume ich davon, Sammelkarten zu verkaufen. Heute habe ich meinen Job gekündigt – jetzt gibt es kein Zurück mehr!",
                "Viel Geld habe ich nicht mehr. Aber es ist ein Anfang: ein Karton Monstera-Packs und ein altes Regal.",
                "Und vielleicht finde ich ja eines Tages die sagenumwobene Karte „Two Moons“, nach der mein Laden benannt ist …",
                "Erst mal aufräumen. Dann Regal aufstellen, Packs einräumen und das Schild an der Tür umdrehen!",
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
            if (MoneyCents < option.Cost)
            {
                ShowToast("Dafür reicht das Geld nicht.");
                SoundManager.Play(Sfx.Error);
                return false;
            }
            MoneyCents -= option.Cost;
            Today.Expenses += option.Cost;
            SoundManager.Play(Sfx.Order);

            if (option.Kind == OrderKind.Shelf)
            {
                ShelvesToPlace++;
                ShowToast($"Regal geliefert! Drücke [B], um es aufzustellen ({ShelvesToPlace} übrig).");
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
            MoneyCents += cents;
            Today.Income += cents;
            Today.PacksSold += packs;
            Today.CustomersServed++;
            ShowToast($"+{Money.Format(cents)}", 2f);
            SoundManager.Play(Sfx.Register);
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
        public void CustomerLeftHappy(bool bargain) => ChangeReputation(bargain ? 3 : 2);

        public void CustomerLeftUnhappy(string reason)
        {
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
            if (ShelvesToPlace > 0) ShowToast($"Regale zum Aufstellen: {ShelvesToPlace} – drücke [B].");

            int dirt = UnityEngine.Random.Range(0, 3);
            for (int i = 0; i < dirt; i++) WorldFactory.SpawnDirtSomewhere();
        }

        public void NotifyDirtCleaned() => DirtCleaned?.Invoke();

        public void NotifyShelfPlaced() => ShelfPlaced?.Invoke();

        public void NotifyShelfStocked(int packs) => ShelfStocked?.Invoke(packs);
    }
}
