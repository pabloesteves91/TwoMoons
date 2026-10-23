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
        public readonly List<OrderOption> PendingDeliveries = new List<OrderOption>();

        public bool ComputerOpen;
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
            DialogLines != null || ComputerOpen || BuildMode;

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

            // Dialoge und das Bestellmenü halten das Spiel an.
            Time.timeScale = DialogLines != null || ComputerOpen ? 0f : 1f;

            if (DialogLines != null)
            {
                if (GameInput.ConfirmPressed && Time.frameCount != dialogOpenedFrame) AdvanceDialog();
                return;
            }

            if (ComputerOpen && GameInput.CancelPressed) ComputerOpen = false;

            if (Phase == GamePhase.Open)
            {
                DayTime += Time.deltaTime;
                if (DayTime >= DayLengthSeconds)
                {
                    Phase = GamePhase.Closing;
                    ShowToast("18:00 – Ladenschluss! Die letzten Kunden werden noch bedient.");
                }
            }
            else if (Phase == GamePhase.Closing && Customer.Active.Count == 0)
            {
                Phase = GamePhase.Summary;
                ComputerOpen = false;
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
            ShopOpened?.Invoke();
        }

        public bool Order(OrderOption option)
        {
            if (MoneyCents < option.Cost)
            {
                ShowToast("Dafür reicht das Geld nicht.");
                return false;
            }
            MoneyCents -= option.Cost;
            Today.Expenses += option.Cost;
            PendingDeliveries.Add(option);
            ShowToast($"Bestellt: {option.Name}. Lieferung morgen früh.");
            return true;
        }

        public void RegisterSale(int cents, int packs)
        {
            MoneyCents += cents;
            Today.Income += cents;
            Today.PacksSold += packs;
            Today.CustomersServed++;
            ShowToast($"+{Money.Format(cents)}", 2f);
            CustomerServed?.Invoke();
        }

        public void CustomerLeftHappy() => ChangeReputation(2);

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
            if (ReputationStage > before) ShowToast($"Dein Laden ist jetzt „{ReputationName}“!", 5f);
        }

        /// <summary>Nach der Abrechnung: neuer Tag, Lieferungen kommen an.</summary>
        public void Sleep()
        {
            Day++;
            Today = new DayStats();
            DayTime = 0f;
            Phase = GamePhase.Preparation;

            foreach (var order in PendingDeliveries)
            {
                if (order.Kind == OrderKind.Shelf)
                {
                    ShelvesToPlace++;
                }
                else
                {
                    var cell = WorldFactory.FreeDeliveryCell();
                    WorldFactory.SpawnBox(cell, Catalog.Get(order.Product), order.Amount);
                }
            }
            if (PendingDeliveries.Count > 0) ShowToast("Die Lieferung ist da!");
            if (ShelvesToPlace > 0) ShowToast($"Regale zum Aufstellen: {ShelvesToPlace} – drücke [B].");
            PendingDeliveries.Clear();

            int dirt = UnityEngine.Random.Range(0, 3);
            for (int i = 0; i < dirt; i++) WorldFactory.SpawnDirtSomewhere();
        }

        public void NotifyDirtCleaned() => DirtCleaned?.Invoke();

        public void NotifyShelfPlaced() => ShelfPlaced?.Invoke();

        public void NotifyShelfStocked(int packs) => ShelfStocked?.Invoke(packs);
    }
}
