using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Nach einem Überfall bis Ladenschluss im Laden: zwei Polizisten am Eingang, die Spurensicherung an der Kasse,
    /// Absperrband und Beweis-Markierungen. Reine Deko – Kunden laufen ganz normal.
    /// </summary>
    public class PoliceScene : MonoBehaviour
    {
        static PoliceScene current;

        public static bool Active => current != null;

        public static void Create()
        {
            if (current != null) return;
            current = new GameObject("Polizei").AddComponent<PoliceScene>();
            current.transform.SetParent(WorldFactory.Root);
            current.Build();
        }

        public static void Remove()
        {
            if (current != null) Destroy(current.gameObject);
            current = null;
        }

        readonly List<SpriteRenderer> people = new List<SpriteRenderer>();
        float turnTimer;

        /// <summary>Ein Polizist: steht an seinem Posten und jagt Ladendiebe, solange er hier ist.</summary>
        class Officer
        {
            public SpriteRenderer Body;
            public Vector2Int Post;
            public Customer Target;
            public List<Vector2Int> Path;
            public int Index;
            public float Repath;
        }

        readonly List<Officer> officers = new List<Officer>();
        const float ChaseSpeed = 3.6f;

        void Build()
        {
            var grid = GameManager.Instance.Grid;
            var door = ShopGrid.DoorCell;
            var queue = ShopGrid.QueueCells[0];

            // Polizisten links und rechts hinter der Tür
            AddOfficer("Polizist", Free(grid, door + new Vector2Int(-1, 1)), false);
            AddOfficer("Polizistin", Free(grid, door + new Vector2Int(1, 1)), true);
            // Spurensicherung im weißen Anzug neben der Kasse
            Person("Spurensicherung", "forensic", Free(grid, queue + Vector2Int.left), false);

            // Absperrband vor dem Kassenbereich
            for (int x = 1; x <= queue.x + 2; x++)
                Prop("Absperrband", "policeTape", new Vector2Int(x, ShopGrid.QueueCells[ShopGrid.QueueCells.Length - 1].y - 1));

            // Gelbe Beweis-Markierungen am Boden
            foreach (var offset in new[] { new Vector2Int(-1, 1), new Vector2Int(1, 0), new Vector2Int(1, -2) })
            {
                var cell = queue + offset;
                if (grid.IsWalkable(cell)) Prop("Beweis", "evidenceMarker", cell, WorldFactory.FloorDecalOrder + 1);
            }
        }

        static Vector2Int Free(ShopGrid grid, Vector2Int cell)
        {
            if (grid.IsWalkable(cell)) return cell;
            foreach (var neighbour in grid.WalkableNeighbours(cell)) return neighbour;
            return cell;
        }

        SpriteRenderer Person(string name, string sprite, Vector2Int cell, bool flip)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(cell), 0, transform);
            renderer.flipX = flip;
            renderer.gameObject.AddComponent<YSort>();
            people.Add(renderer);
            return renderer;
        }

        void AddOfficer(string name, Vector2Int cell, bool flip) =>
            officers.Add(new Officer { Body = Person(name, "police", cell, flip), Post = cell });

        void Prop(string name, string sprite, Vector2Int cell, int order = 0)
        {
            var renderer = WorldFactory.CreateSprite(name, sprite, ShopGrid.CellToWorld(cell), order, transform);
            if (order == 0) renderer.gameObject.AddComponent<YSort>();
        }

        void Update()
        {
            ChaseThieves();
            LookAround();
        }

        /// <summary>Schleicht ein Ladendieb zur Tür, rennt der nächste freie Polizist hinterher und schnappt ihn.</summary>
        void ChaseThieves()
        {
            foreach (var customer in Customer.Active)
            {
                if (!customer.IsSneaking || officers.Exists(o => o.Target == customer)) continue;
                Officer nearest = null;
                foreach (var officer in officers)
                    if (officer.Target == null && (nearest == null ||
                        Vector3.Distance(officer.Body.transform.position, customer.transform.position) <
                        Vector3.Distance(nearest.Body.transform.position, customer.transform.position)))
                        nearest = officer;
                if (nearest == null) break;
                nearest.Target = customer;
                nearest.Repath = 0f;
                GameManager.Instance.ShowToast("Die Polizei hat den Ladendieb bemerkt!", 3f);
            }

            var grid = GameManager.Instance.Grid;
            foreach (var officer in officers)
            {
                var body = officer.Body.transform;
                bool chasing = officer.Target != null && officer.Target.IsSneaking;
                if (officer.Target != null && !chasing) officer.Target = null;

                if (chasing && Vector3.Distance(body.position, officer.Target.transform.position) < 0.7f)
                {
                    officer.Target.Caught();
                    officer.Target = null;
                    officer.Repath = 0f;
                    continue;
                }

                // Ziel: der Dieb, sonst zurück zum Posten
                var goal = chasing ? ShopGrid.WorldToCell(officer.Target.transform.position) : officer.Post;
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
                    continue;
                }
                if (Mathf.Abs(step.x) > 0.01f) officer.Body.flipX = step.x < 0;
                body.position = Vector3.MoveTowards(body.position, target, (chasing ? ChaseSpeed : 2f) * Time.deltaTime);
            }
        }

        /// <summary>Ab und zu schaut jemand in die andere Richtung.</summary>
        void LookAround()
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer > 0f || people.Count == 0) return;
            turnTimer = Random.Range(2f, 5f);
            var person = people[Random.Range(0, people.Count)];
            // Wer gerade rennt, dreht sich nicht einfach um.
            if (person != null && !officers.Exists(o => o.Body == person && (o.Target != null || ShopGrid.WorldToCell(person.transform.position) != o.Post)))
                person.flipX = !person.flipX;
        }
    }
}
