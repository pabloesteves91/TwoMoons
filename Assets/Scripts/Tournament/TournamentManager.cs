using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Turniere ab Store-Level 3: jeden 2. Tag von 15 bis 18 Uhr (vorerst Monstera).
    /// Morgens kommen Gratis-Packs vom Hersteller für den Prizepool, ab 13 Uhr melden sich Spieler
    /// an der Kasse an (15 € Startgeld), um 15 Uhr wird an den Spieltischen gespielt,
    /// um 18 Uhr versammeln sich alle am Turnier-Counter zur Siegerehrung.
    /// Ab Level 6 werden daraus Großturniere (mehr Spieler, mehr XP), in Kapitel 7 kommt das Stadtturnier als Finale.
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        public enum State { None, Registration, Running, Ceremony }

        public const int EntryFee = 1500;
        public const int StartMinute = 15 * 60;
        const int RegistrationMinute = 13 * 60;
        const int Rounds = 3;
        /// <summary>So lange warten die Spieler nach Turnierende auf die Siegerehrung (Sekunden).</summary>
        const float CeremonyPatience = 60f;

        static readonly string[] PlayerNames =
        {
            "Lena", "Tim", "Mo", "Jule", "Ben", "Aylin", "Noah", "Mia", "Elias", "Sophie",
            "Finn", "Lea", "Jonas", "Emma", "Luca", "Zoe",
        };

        public State Current { get; private set; }
        /// <summary>Spieltag des nächsten Turniers (0 = noch keins geplant).</summary>
        public int NextDay { get; private set; }
        public int Held { get; private set; }

        /// <summary>Prizepool in Packs: 1., 2., 3. Platz, Teilnahmepreise (für alle zusammen).</summary>
        public readonly int[] Prizes = new int[4];
        /// <summary>Zusätzliche Karte aus der Sammlung für den 1. Platz (-1 = keine).</summary>
        public int BonusCard = -1;

        public readonly List<TournamentPlayer> Players = new List<TournamentPlayer>();
        int expectedPlayers;
        int spawnedEntrants;
        float nextSpawn;
        float ceremonyTimer;

        static GameManager Game => GameManager.Instance;

        public int PrizePackTotal => Prizes[0] + Prizes[1] + Prizes[2] + Prizes[3];
        public bool IsTournamentDay => NextDay > 0 && Game.Day == NextDay;
        public bool HasPrizepool => PrizePackTotal > 0 || BonusCard >= 0;
        public int Round => Mathf.Clamp(1 + Mathf.FloorToInt(RunningFraction * Rounds), 1, Rounds);

        float RunningFraction =>
            Mathf.Clamp01((Game.ClockMinutes - StartMinute) / (float)(GameManager.CloseMinute - StartMinute));

        public bool Busy => Current != State.None;

        /// <summary>Wie viele Plätze die Spieltische bieten (2 pro Tisch). Es zählen nur Tische, an denen beide Seiten frei sind.</summary>
        public int Seats => PlayableTables().Count * 2;

        /// <summary>Spieltische, an denen links und rechts Platz ist – nur dort wird gespielt.</summary>
        static List<GameTable> PlayableTables() => Tables().FindAll(t => t.Seats().Count == 2);

        public static List<GameTable> Tables()
        {
            var tables = new List<GameTable>();
            foreach (var deco in Game.Decos)
                if (deco.Type == Furniture.GameTable) tables.Add(deco.GetComponent<GameTable>());
            return tables;
        }

        public static PrizeCounter Counter()
        {
            foreach (var deco in Game.Decos)
                if (deco.Type == Furniture.PrizeCounter) return deco.GetComponent<PrizeCounter>();
            return null;
        }

        public bool Ready => Game.StoreLevel >= 3 && Tables().Count > 0 && Counter() != null;

        /// <summary>Spieltische, die man für ein Großturnier braucht.</summary>
        public const int BigTournamentTables = 4;

        /// <summary>Großturnier ab Level 6 mit mindestens 4 Spieltischen: mehr Spieler und mehr Belohnung.</summary>
        public bool IsBig => Game.StoreLevel >= 6 && Tables().Count >= BigTournamentTables;

        /// <summary>Das nächste Turnier ist das Stadtturnier (letzte Etappe von Kapitel 7).</summary>
        public bool IsCityTournament
        {
            get
            {
                var chapters = Game.ChapterProgress;
                return chapters.Chapter == 7 && !chapters.Completed && chapters.Quests.Exists(q => q.Key == CityTournamentKey);
            }
        }

        public const string CityTournamentKey = "cityTournament";

        /// <summary>Papa schaut beim Stadtturnier aus der ersten Reihe zu.</summary>
        GameObject papa;

        string Kind => IsCityTournament ? "Stadtturnier" : IsBig ? "Großturnier" : "Turnier";

        /// <summary>Wie viele Spieler voraussichtlich kommen: Ruf, Prizepool und Bonus-Karte locken.</summary>
        public int ExpectedPlayers
        {
            get
            {
                if (IsCityTournament) return Seats;
                int players = 2 + Game.ReputationStage + PrizePackTotal / 4 + (IsBig ? 3 : 0) +
                              (Game.Skills.Has(Skill.Organizer) ? 2 : 0);
                var bonus = CardDatabase.Get(BonusCard);
                if (bonus != null) players += 1 + (int)bonus.Rarity;
                players = Mathf.Clamp(players, 0, Seats);
                // Immer paarweise, damit jeder Tisch voll ist.
                return players - players % 2;
            }
        }

        public string GameName => Catalog.GameName(TcgGame.Monstera);

        /// <summary>Zeile für die Kopfzeile, oder null wenn heute nichts ist.</summary>
        public string StatusLine
        {
            get
            {
                if (NextDay <= 0) return null;
                switch (Current)
                {
                    case State.Registration: return $"{Kind} heute 15–18 Uhr ({GameName}) · Anmeldungen: {Players.Count + PendingEntrants}";
                    case State.Running: return $"{Kind} läuft: Runde {Round}/{Rounds} · {Players.Count} Spieler";
                    case State.Ceremony: return "Siegerehrung am Turnier-Counter – [E] dort drücken!";
                }
                if (IsTournamentDay)
                    return HasPrizepool
                        ? $"{Kind} heute 15–18 Uhr ({GameName}) · Prizepool: {PrizePackTotal} Packs"
                        : $"{Kind} heute 15 Uhr – Prizepool am Turnier-Counter vorbereiten!";
                string bigHint = Game.StoreLevel >= 6 && !IsBig && !IsCityTournament
                    ? $" · Für ein Großturnier: {Tables().Count}/{BigTournamentTables} Spieltische"
                    : "";
                return $"Nächstes {Kind}: Tag {NextDay}, 15 Uhr ({GameName}){bigHint}";
            }
        }

        int PendingEntrants
        {
            get
            {
                int count = 0;
                foreach (var customer in Customer.Active) if (customer.IsTournamentEntrant) count++;
                return count;
            }
        }

        public void LoadFrom(SavedTournament saved)
        {
            if (saved == null) return;
            NextDay = saved.nextDay;
            Held = saved.held;
            BonusCard = saved.bonusCard;
            for (int i = 0; i < Prizes.Length && i < saved.prizes.Length; i++) Prizes[i] = saved.prizes[i];

            // Ein laufendes Turnier geht nach dem Laden genau dort weiter, wo es war.
            Current = (State)saved.state;
            if (Current == State.None || saved.players == null) return;
            expectedPlayers = saved.expectedPlayers;
            spawnedEntrants = Mathf.Min(saved.spawnedEntrants, saved.players.Count);
            ceremonyTimer = saved.ceremonyTimer > 0f ? saved.ceremonyTimer : CeremonyPatience;
            foreach (var savedPlayer in saved.players)
            {
                var seat = new Vector2Int(savedPlayer.x, savedPlayer.y);
                var player = WorldFactory.SpawnTournamentPlayer(ShopGrid.CellToWorld(seat), Art.Get(savedPlayer.sprite), savedPlayer.name);
                player.Seat = seat;
                Players.Add(player);
            }
            if (Current == State.Ceremony)
            {
                var counter = Counter();
                var spots = counter != null ? new List<Vector2Int>(Game.Grid.WalkableNeighbours(counter.Cell)) : new List<Vector2Int>();
                for (int i = 0; i < Players.Count; i++) Players[i].GoCheer(spots.Count > 0 ? spots[i % spots.Count] : Players[i].Seat);
            }
            if (Current == State.Registration && IsCityTournament) SpawnPapa();
        }

        public SavedTournament Save()
        {
            var saved = new SavedTournament
            {
                nextDay = NextDay,
                held = Held,
                bonusCard = BonusCard,
                prizes = (int[])Prizes.Clone(),
                state = (int)Current,
                expectedPlayers = expectedPlayers,
                spawnedEntrants = spawnedEntrants,
                ceremonyTimer = ceremonyTimer,
            };
            foreach (var player in Players)
            {
                var sprite = player.GetComponent<SpriteRenderer>().sprite;
                saved.players.Add(new SavedTournamentPlayer
                {
                    name = player.Name, sprite = sprite != null ? sprite.name : "customer1", x = player.Seat.x, y = player.Seat.y,
                });
            }
            return saved;
        }

        /// <summary>Jeden Morgen: Turnier planen und am Turniertag die Hersteller-Packs liefern.</summary>
        public void OnNewDay()
        {
            if (!Ready) return;
            if (NextDay <= 0 || NextDay < Game.Day) NextDay = Game.Day + 1;
            if (!IsTournamentDay)
            {
                Game.ShowToast($"Morgen ist {Kind}! ({GameName}, 15–18 Uhr)", 5f);
                return;
            }

            int packs = (IsCityTournament ? 40 : 8 + 4 * (Game.StoreLevel - 3)) + (Game.Skills.Has(Skill.Organizer) ? 4 : 0);
            WorldFactory.SpawnBox(WorldFactory.FreeDeliveryCell(), Catalog.TournamentPack, packs);
            Game.ShowToast($"{Kind}! Der Hersteller schickt {packs} Turnier-Packs – bring sie zum Turnier-Counter.", 7f);
        }

        /// <summary>Hersteller-Packs in den Prizepool legen und automatisch aufteilen.</summary>
        public void AddPrizePacks(int packs)
        {
            int total = PrizePackTotal + packs;
            Prizes[0] = Mathf.RoundToInt(total * 0.4f);
            Prizes[1] = Mathf.RoundToInt(total * 0.25f);
            Prizes[2] = Mathf.RoundToInt(total * 0.15f);
            Prizes[3] = total - Prizes[0] - Prizes[1] - Prizes[2];
        }

        /// <summary>Ein Pack von einem Platz zu einem anderen verschieben (+/- im Prizepool-Fenster).</summary>
        public void MovePack(int from, int to)
        {
            if (Prizes[from] <= 0) return;
            Prizes[from]--;
            Prizes[to]++;
        }

        public void SetBonusCard(int cardId)
        {
            if (BonusCard >= 0) Game.Collection.Add(BonusCard);
            BonusCard = -1;
            if (cardId >= 0 && Game.Collection.Remove(cardId)) BonusCard = cardId;
        }

        void Update()
        {
            if (Game.Phase == GamePhase.MainMenu) return;

            switch (Current)
            {
                case State.None:
                    if (IsTournamentDay && Game.Phase == GamePhase.Open && Game.ClockMinutes >= RegistrationMinute && Ready)
                        BeginRegistration();
                    break;

                case State.Registration:
                    SpawnEntrants();
                    if (Game.ClockMinutes >= StartMinute) StartTournament();
                    break;

                case State.Running:
                    if (Game.Phase != GamePhase.Open) EndTournament();
                    break;

                case State.Ceremony:
                    ceremonyTimer -= Time.deltaTime;
                    if (ceremonyTimer <= 0f) CeremonyMissed();
                    break;
            }
        }

        void BeginRegistration()
        {
            Current = State.Registration;
            expectedPlayers = ExpectedPlayers;
            spawnedEntrants = 0;
            nextSpawn = 0f;
            Game.ShowToast($"Die Anmeldung fürs {Kind} läuft! Erwartet: etwa {expectedPlayers} Spieler.", 5f);
            if (IsCityTournament) SpawnPapa();
        }

        /// <summary>Papa nimmt neben dem Turnier-Counter Platz, in der ersten Reihe.</summary>
        void SpawnPapa()
        {
            if (papa != null) return;
            var counter = Counter();
            var spot = ShopGrid.PlayerStartCell;
            if (counter != null)
                foreach (var cell in Game.Grid.WalkableNeighbours(counter.Cell)) { spot = cell; break; }
            papa = WorldFactory.CreateSprite("Papa", "papa", ShopGrid.CellToWorld(spot), 0).gameObject;
            papa.AddComponent<YSort>();
            Game.ShowToast("Papa ist da! Er sitzt in der ersten Reihe und winkt dir zu.", 6f);
        }

        void SpawnEntrants()
        {
            if (spawnedEntrants >= expectedPlayers) return;
            nextSpawn -= Time.deltaTime;
            if (nextSpawn > 0f) return;
            nextSpawn = Random.Range(4f, 9f);
            spawnedEntrants++;
            WorldFactory.SpawnCustomer().IsTournamentEntrant = true;
            SoundManager.Play(Sfx.Doorbell);
        }

        /// <summary>Ein Spieler hat an der Kasse bezahlt und setzt sich an einen Spieltisch.</summary>
        public void Register(Customer entrant)
        {
            if (!CanRegister) return;
            var seat = FreeSeat();
            var player = WorldFactory.SpawnTournamentPlayer(entrant.transform.position, entrant.Sprite, PlayerNames[Players.Count % PlayerNames.Length]);
            player.Seat = seat.Value;
            Players.Add(player);
        }

        /// <summary>Anmelden geht nur vor dem Start und solange ein Platz frei ist (danach bliebe ein Tisch halb leer).</summary>
        public bool CanRegister => Current == State.Registration && FreeSeat() != null;

        /// <summary>Plätze werden Tisch für Tisch vergeben, damit sich immer zwei gegenübersitzen.</summary>
        Vector2Int? FreeSeat()
        {
            foreach (var table in PlayableTables())
                foreach (var seat in table.Seats())
                    if (!Players.Exists(p => p.Seat == seat)) return seat;
            return null;
        }

        void StartTournament()
        {
            if (Players.Count < 2)
            {
                Game.ShowToast("Zu wenige Anmeldungen – das Turnier fällt heute aus.", 6f);
                if (papa != null) Destroy(papa);
                foreach (var player in Players) player.Leave(false);
                Players.Clear();
                Finish();
                return;
            }
            Current = State.Running;
            SoundManager.Play(Sfx.Quest);
            foreach (var player in Players) player.StartPlaying();
            // Erst danach: der Einspringer muss noch vom Eingang zu seinem Platz laufen
            // (StartPlaying vor dem Losgehen würde ihn draußen stehen lassen).
            if (Players.Count % 2 == 1) AddStandIn();
            Game.ShowToast($"Das {Kind} beginnt! {Players.Count} Spieler, 3 Runden.", 5f);
        }

        /// <summary>Bei ungerader Anmeldezahl springt spontan noch jemand ein, damit jeder Tisch voll ist.</summary>
        void AddStandIn()
        {
            var seat = FreeSeat();
            if (seat == null) return;
            string name = PlayerNames[Players.Count % PlayerNames.Length];
            var player = WorldFactory.SpawnTournamentPlayer(ShopGrid.CellToWorld(ShopGrid.SpawnCell),
                Art.Get($"customer{Random.Range(1, 4)}"), name);
            player.Seat = seat.Value;
            Players.Add(player);
            Game.RegisterEntryFee();
            Game.ShowToast($"Ungerade Spielerzahl – {name} springt spontan ein, damit jeder einen Gegner hat!", 5f);
        }

        void EndTournament()
        {
            Current = State.Ceremony;
            ceremonyTimer = CeremonyPatience;
            var counter = Counter();
            var spots = counter != null ? new List<Vector2Int>(Game.Grid.WalkableNeighbours(counter.Cell)) : new List<Vector2Int>();
            for (int i = 0; i < Players.Count; i++)
            {
                // Rund um den Counter verteilen; wer keinen Platz hat, wartet in der Nähe.
                var target = spots.Count > 0 ? spots[i % spots.Count] : Players[i].Seat;
                Players[i].GoCheer(target);
            }
            // Zufällige Platzierungen
            for (int i = Players.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (Players[i], Players[j]) = (Players[j], Players[i]);
            }
            SoundManager.Play(Sfx.Fanfare);
            Game.ShowToast("Das Turnier ist vorbei! Alle warten am Turnier-Counter auf die Siegerehrung.", 6f);
        }

        /// <summary>Die Siegerehrung wurde am Counter durchgeführt.</summary>
        public void CeremonyDone()
        {
            bool hadPrizes = HasPrizepool;
            int players = Players.Count;
            foreach (var player in Players) player.Leave(hadPrizes);
            Players.Clear();

            bool city = IsCityTournament;
            if (hadPrizes)
            {
                int xp = (IsBig ? 100 : 60) + (players >= Seats ? 20 : 0);
                Game.ChangeReputation(5 + 2 * players);
                Game.AddXp(xp);
                Game.ShowToast($"Tolles {Kind}! Ruf +{5 + 2 * players}, +{xp} XP", 6f);
            }
            else
            {
                Game.ChangeReputation(-5);
                Game.ShowToast("Ohne Preise waren die Spieler enttäuscht. Ruf −5", 6f);
            }

            for (int i = 0; i < Prizes.Length; i++) Prizes[i] = 0;
            BonusCard = -1;
            Held++;
            Game.NotifyTournamentHeld();

            if (papa != null) Destroy(papa);
            if (Held == 1)
                Game.PlayStoryOnce("erstesTurnier", Cutscenes.FirstTournament);
            if (city && hadPrizes) Game.NotifyCityTournament();
            Finish();
        }

        void CeremonyMissed()
        {
            if (papa != null) Destroy(papa);
            foreach (var player in Players) player.Leave(false);
            Players.Clear();
            Game.ChangeReputation(-5);
            Game.ShowToast("Niemand hat die Preise verteilt – die Spieler sind enttäuscht gegangen. Ruf −5", 6f);
            Finish();
        }

        void Finish()
        {
            Current = State.None;
            NextDay = Game.Day + 2;
        }
    }
}
