using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Nach einem Überfall bis Ladenschluss: Ein Streifenwagen kommt mit Sirene angefahren, draußen blinkt es blau-rot.
    /// Die Polizisten verhaften den Gauner am Eingang, befragen die Kunden, die dabei waren, und bleiben dann an der Tür
    /// stehen (und schnappen Ladendiebe). Die Spurensicherung arbeitet an der Kasse, davor Absperrband und Markierungen.
    /// </summary>
    public class PoliceScene : MonoBehaviour
    {
        static PoliceScene current;

        public static bool Active => current != null;

        /// <param name="robber">Der flüchtende Gauner (null nach dem Laden eines Spielstands).</param>
        /// <param name="witnesses">Kunden, die beim Überfall im Laden geblieben sind.</param>
        /// <param name="escapes">Die Polizei kommt zu spät: Der Gauner ist mit der Beute weg.</param>
        public static void Create(Customer robber = null, List<Customer> witnesses = null, bool escapes = false)
        {
            if (current != null) return;
            current = new GameObject("Polizei").AddComponent<PoliceScene>();
            current.transform.SetParent(WorldFactory.Root);
            current.escapes = escapes;
            current.Build(robber, witnesses);
        }

        bool escapes;
        /// <summary>Wie lange der Streifenwagen noch braucht, bis er auftaucht.</summary>
        float delay;

        public static void Remove()
        {
            if (current != null) Destroy(current.gameObject);
            current = null;
        }

        static readonly string[] WitnessLines =
        {
            "Er hatte eine Sturmhaube auf!", "Ganz in Schwarz war der!", "Ich hab mich hinterm Regal versteckt …",
            "Er wollte zuerst an die Kasse!", "So groß ungefähr …", "Ich hab nur seine Schuhe gesehen.",
        };

        const float ChaseSpeed = 3.6f;
        const float WalkSpeed = 2.2f;
        const float CarSpeed = 7f;
        const float QuestionSeconds = 4f;

        enum Mode { Post, Chase, Question }

        /// <summary>Ein Polizist: steht an seinem Posten, verhaftet, befragt Zeugen und jagt Ladendiebe.</summary>
        class Officer
        {
            public SpriteRenderer Body;
            public Vector2Int Post;
            public Mode Mode;
            public Customer Target;
            public List<Vector2Int> Path;
            public int Index;
            public float Repath;
            public float Wait;
        }

        readonly List<SpriteRenderer> people = new List<SpriteRenderer>();
        readonly List<Officer> officers = new List<Officer>();
        readonly Queue<Customer> witnesses = new Queue<Customer>();
        Customer robber;
        bool robberHandled;

        SpriteRenderer car;
        Vector3 carParked;
        bool arrived;
        float sirenTimer;
        readonly List<SpriteRenderer> lights = new List<SpriteRenderer>();
        float turnTimer;

        void Build(Customer fleeing, List<Customer> present)
        {
            var grid = GameManager.Instance.Grid;
            var queue = ShopGrid.QueueCells[0];
            robber = fleeing;
            if (present != null) foreach (var witness in present) witnesses.Enqueue(witness);

            // Streifenwagen auf der Straße, rechts neben der Tür. Beim Überfall fährt er von rechts heran.
            carParked = ShopGrid.CellToWorld(new Vector2Int(ShopGrid.DoorCell.x + 2, 0));
            bool animate = fleeing != null;
            var start = animate ? ShopGrid.CellToWorld(new Vector2Int(ShopGrid.Width + 2, 0)) : carParked;
            car = WorldFactory.CreateSprite("Streifenwagen", "policeCar", start, 0, transform);
            car.transform.localScale = new Vector3(2f, 1f, 1f);
            car.gameObject.AddComponent<YSort>();

            // Blau-rotes Blaulicht-Leuchten draußen auf der Straße (und ein Schimmer an der Ladenfront).
            for (int row = 0; row < 2; row++)
            {
                var glow = WorldFactory.CreateSprite("Blaulicht", "white",
                    new Vector3(ShopGrid.CellToWorld(new Vector2Int(0, row)).x + (ShopGrid.Width - 1) / 2f, ShopGrid.CellToWorld(new Vector2Int(0, row)).y, 0f),
                    WorldFactory.FloorDecalOrder + 5, transform);
                glow.transform.localScale = new Vector3(ShopGrid.Width, 1f, 1f);
                lights.Add(glow);
            }

            // Spurensicherung, Absperrband und Markierungen an der Kasse
            Person("Spurensicherung", "forensic", Free(grid, queue + Vector2Int.left), false);
            for (int x = 1; x <= queue.x + 2; x++)
                Prop("Absperrband", "policeTape", new Vector2Int(x, ShopGrid.QueueCells[ShopGrid.QueueCells.Length - 1].y - 1));
            foreach (var offset in new[] { new Vector2Int(-1, 1), new Vector2Int(1, 0), new Vector2Int(1, -2) })
            {
                var cell = queue + offset;
                if (grid.IsWalkable(cell)) Prop("Beweis", "evidenceMarker", cell, WorldFactory.FloorDecalOrder + 1);
            }

            if (!animate) Arrive(false);
            else if (escapes) delay = 6f; // zu spät: erst wenn der Gauner längst weg ist
            else SoundManager.Play(Sfx.Siren);
        }

        /// <summary>Der Wagen steht: Polizisten steigen aus (beim Laden stehen sie gleich auf ihrem Posten).</summary>
        void Arrive(bool fromCar)
        {
            arrived = true;
            // Nach dem Laden eines Spielstands ist die Verhaftung längst erledigt.
            if (!fromCar) robberHandled = true;
            else if (escapes)
            {
                robberHandled = true;
                GameManager.Instance.RobberEscaped();
            }
            var grid = GameManager.Instance.Grid;
            var door = ShopGrid.DoorCell;
            var spawn = fromCar ? ShopGrid.SpawnCell : (Vector2Int?)null;
            AddOfficer("Polizist", Free(grid, door + new Vector2Int(-1, 1)), false, spawn);
            AddOfficer("Polizistin", Free(grid, door + new Vector2Int(1, 1)), true, spawn);
            if (robberHandled) return;
            if (robber != null && !robber.IsArrested)
            {
                officers[0].Mode = Mode.Chase;
                officers[0].Target = robber;
            }
            else ArrestRobber();
        }

        static Vector2Int Free(ShopGrid grid, Vector2Int cell)
        {
            if (grid.IsWalkable(cell)) return cell;
            foreach (var neighbour in grid.WalkableNeighbours(cell)) return neighbour;
            return cell;
        }

        SpriteRenderer Person(string name, string sprite, Vector2Int cell, bool flip, Vector2Int? from = null)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(from ?? cell), 0, transform);
            renderer.flipX = flip;
            renderer.gameObject.AddComponent<YSort>();
            people.Add(renderer);
            return renderer;
        }

        void AddOfficer(string name, Vector2Int post, bool flip, Vector2Int? from) =>
            officers.Add(new Officer { Body = Person(name, "police", post, flip, from), Post = post });

        void Prop(string name, string sprite, Vector2Int cell, int order = 0)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(cell), order, transform);
            if (order == 0) renderer.gameObject.AddComponent<YSort>();
        }

        void Update()
        {
            FlashLights();
            if (!arrived)
            {
                DriveIn();
                return;
            }
            AssignJobs();
            MoveOfficers();
            LookAround();
        }

        void FlashLights()
        {
            bool blue = Mathf.Repeat(Time.time * 2.5f, 1f) < 0.5f;
            var color = blue ? new Color(0.25f, 0.45f, 1f, 0.32f) : new Color(1f, 0.2f, 0.2f, 0.32f);
            for (int i = 0; i < lights.Count; i++)
                lights[i].color = i == 0 ? color : new Color(color.r, color.g, color.b, color.a * 0.4f);
        }

        void DriveIn()
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                if (delay <= 0f) SoundManager.Play(Sfx.Siren);
                return;
            }
            car.transform.position = Vector3.MoveTowards(car.transform.position, carParked, CarSpeed * Time.deltaTime);
            sirenTimer -= Time.deltaTime;
            if (sirenTimer <= 0f)
            {
                sirenTimer = 1.4f;
                SoundManager.Play(Sfx.Siren);
            }
            if ((car.transform.position - carParked).sqrMagnitude < 0.0001f) Arrive(true);
        }

        /// <summary>Gauner verhaften, dann Zeugen befragen; schleichende Ladendiebe werden geschnappt.</summary>
        void AssignJobs()
        {
            // Hat der Gauner den Laden schon verlassen, wird er draußen am Wagen verhaftet.
            if (robber == null && !robberHandled && !escapes) ArrestRobber();

            foreach (var customer in Customer.Active)
            {
                if (!customer.IsSneaking || officers.Exists(o => o.Target == customer)) continue;
                var free = Nearest(customer.transform.position, o => o.Mode != Mode.Chase);
                if (free == null) break;
                if (free.Mode == Mode.Question && free.Target != null) witnesses.Enqueue(free.Target);
                free.Mode = Mode.Chase;
                free.Target = customer;
                free.Repath = 0f;
                GameManager.Instance.ShowToast("Die Polizei hat den Ladendieb bemerkt!", 3f);
            }

            while (witnesses.Count > 0 && (witnesses.Peek() == null || !Customer.Active.Contains(witnesses.Peek()))) witnesses.Dequeue();
            if (witnesses.Count > 0 && robberHandled)
            {
                var idle = Nearest(witnesses.Peek().transform.position, o => o.Mode == Mode.Post);
                if (idle != null)
                {
                    idle.Mode = Mode.Question;
                    idle.Target = witnesses.Dequeue();
                    idle.Wait = 0f;
                    idle.Repath = 0f;
                }
            }
        }

        Officer Nearest(Vector3 position, System.Predicate<Officer> allowed)
        {
            Officer nearest = null;
            foreach (var officer in officers)
                if (allowed(officer) && (nearest == null ||
                    Vector3.Distance(officer.Body.transform.position, position) < Vector3.Distance(nearest.Body.transform.position, position)))
                    nearest = officer;
            return nearest;
        }

        void ArrestRobber()
        {
            robberHandled = true;
            if (robber != null) robber.Arrest();
            GameManager.Instance.RobberArrested();
        }

        void MoveOfficers()
        {
            var grid = GameManager.Instance.Grid;
            foreach (var officer in officers)
            {
                var body = officer.Body.transform;
                bool hasTarget = officer.Target != null && Customer.Active.Contains(officer.Target);

                switch (officer.Mode)
                {
                    case Mode.Chase:
                        bool isRobber = officer.Target == robber;
                        if (!hasTarget || (!isRobber && !officer.Target.IsSneaking))
                        {
                            if (isRobber && !robberHandled) ArrestRobber();
                            Back(officer);
                            break;
                        }
                        if (Vector3.Distance(body.position, officer.Target.transform.position) < 0.7f)
                        {
                            if (isRobber) ArrestRobber();
                            else officer.Target.Caught();
                            Back(officer);
                        }
                        break;

                    case Mode.Question:
                        if (!hasTarget)
                        {
                            Back(officer);
                            break;
                        }
                        if (Vector3.Distance(body.position, officer.Target.transform.position) < 1.1f)
                        {
                            if (officer.Wait <= 0f)
                            {
                                officer.Wait = QuestionSeconds;
                                officer.Body.flipX = officer.Target.transform.position.x < body.position.x;
                                officer.Target.HoldForQuestioning(QuestionSeconds, WitnessLines[Random.Range(0, WitnessLines.Length)]);
                            }
                            officer.Wait -= Time.deltaTime;
                            if (officer.Wait <= 0f) Back(officer);
                            continue;
                        }
                        break;
                }

                var goal = officer.Mode == Mode.Post ? officer.Post : ShopGrid.WorldToCell(officer.Target.transform.position);
                Walk(officer, grid, goal, officer.Mode == Mode.Chase ? ChaseSpeed : WalkSpeed);
            }
        }

        static void Back(Officer officer)
        {
            officer.Mode = Mode.Post;
            officer.Target = null;
            officer.Repath = 0f;
        }

        static void Walk(Officer officer, ShopGrid grid, Vector2Int goal, float speed)
        {
            var body = officer.Body.transform;
            officer.Repath -= Time.deltaTime;
            if (officer.Repath <= 0f)
            {
                officer.Repath = 0.4f;
                officer.Path = grid.FindPath(ShopGrid.WorldToCell(body.position), goal);
                officer.Index = 0;
            }
            Vector3 target = officer.Path != null && officer.Index < officer.Path.Count
                ? ShopGrid.CellToWorld(officer.Path[officer.Index])
                : ShopGrid.CellToWorld(goal);
            Vector3 step = target - body.position;
            if (step.sqrMagnitude < 0.0001f)
            {
                if (officer.Path != null && officer.Index < officer.Path.Count) officer.Index++;
                return;
            }
            if (Mathf.Abs(step.x) > 0.01f) officer.Body.flipX = step.x < 0;
            body.position = Vector3.MoveTowards(body.position, target, speed * Time.deltaTime);
        }

        /// <summary>Ab und zu schaut jemand in die andere Richtung (nur wer gerade still steht).</summary>
        void LookAround()
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer > 0f || people.Count == 0) return;
            turnTimer = Random.Range(2f, 5f);
            var person = people[Random.Range(0, people.Count)];
            if (person != null && !officers.Exists(o => o.Body == person && (o.Mode != Mode.Post || ShopGrid.WorldToCell(person.transform.position) != o.Post)))
                person.flipX = !person.flipX;
        }
    }
}
