using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Turnierspieler: setzt sich an einen Spieltisch, spielt, jubelt bei der Siegerehrung und geht.</summary>
    public class TournamentPlayer : MonoBehaviour
    {
        enum State { ToSeat, Waiting, Playing, Cheering, Leaving }

        const float Speed = 2.2f;

        public string Name;
        public Vector2Int Seat;

        State state = State.ToSeat;
        List<Vector2Int> path;
        int pathIndex;
        SpriteRenderer body;
        SpriteRenderer icon;
        float iconTimer;
        bool happy;

        static GameManager Game => GameManager.Instance;
        bool Arrived => path == null || pathIndex >= path.Count;

        void Start()
        {
            body = GetComponent<SpriteRenderer>();
            icon = WorldFactory.CreateSprite("Symbol", "cardIcon", transform.position + new Vector3(0f, 0.85f, 0f), 50, transform);
            icon.transform.localScale = Vector3.one * 0.5f;
            icon.enabled = false;
            GetComponent<YSort>().Collect();
            // Nach dem Laden kann ein Spieler schon bei der Siegerehrung sein – dann nicht erst zum Platz.
            if (state == State.ToSeat) WalkTo(Seat);
        }

        void WalkTo(Vector2Int cell)
        {
            path = Game.Grid.FindPath(ShopGrid.WorldToCell(transform.position), cell);
            pathIndex = 0;
        }

        public void StartPlaying() => state = State.Playing;

        public void GoCheer(Vector2Int spot)
        {
            state = State.Cheering;
            WalkTo(spot);
        }

        public void Leave(bool wasHappy)
        {
            happy = wasHappy;
            state = State.Leaving;
            icon.enabled = true;
            icon.sprite = Art.Get(wasHappy ? "moodHappy" : "moodAngry");
            WalkTo(ShopGrid.SpawnCell);
            if (path == null) Destroy(gameObject);
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

            switch (state)
            {
                case State.ToSeat:
                    // Am Platz zum Tisch drehen: die beiden Spieler sitzen sich gegenüber.
                    var table = TournamentManager.Tables().Find(t => System.Array.IndexOf(GameTable.SeatCells(t.Cell), Seat) >= 0);
                    if (table != null) body.flipX = table.Cell.x < Seat.x;
                    // Wer sich spät angemeldet hat, steigt direkt ins laufende Turnier ein.
                    state = Game.Tournament.Current == TournamentManager.State.Running ? State.Playing : State.Waiting;
                    break;

                case State.Playing:
                    // Karten ausspielen, ab und zu ein Jubel oder Ärger
                    iconTimer -= Time.deltaTime;
                    if (iconTimer > 0f) break;
                    iconTimer = Random.Range(0.8f, 2.2f);
                    float roll = Random.value;
                    icon.enabled = roll < 0.75f;
                    icon.sprite = Art.Get(roll < 0.55f ? "cardIcon" : roll < 0.65f ? "moodHappy" : "moodNeutral");
                    break;

                case State.Cheering:
                    // Hüpfen vor Freude
                    icon.enabled = true;
                    icon.sprite = Art.Get("moodHappy");
                    body.transform.position = ShopGrid.CellToWorld(ShopGrid.WorldToCell(transform.position)) +
                                              new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 6f + Name.Length)) * 0.15f, 0f);
                    break;

                case State.Leaving:
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
