using UnityEngine;

namespace TwoMoons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        const float Speed = 3.5f;
        const float InteractRange = 1.3f;

        Rigidbody2D body;
        SpriteRenderer spriteRenderer;

        public DeliveryBox Carried { get; private set; }

        /// <summary>Das Objekt, mit dem [E] gerade interagieren würde.</summary>
        public Interactable Focus { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetFigure(int figure) => spriteRenderer.sprite = Art.Get($"player{figure + 1}");

        void Update()
        {
            var game = GameManager.Instance;
            if (game.InputLocked)
            {
                Focus = null;
                return;
            }

            Focus = FindFocus();

            // Die Taste, die gerade einen Dialog geschlossen hat, soll nicht sofort noch etwas auslösen.
            if (Time.frameCount == game.DialogClosedFrame) return;

            if (GameInput.InteractPressed && Focus != null && Focus.CanInteract(this)) Focus.Interact(this);
            if (GameInput.DropPressed && Carried != null) Drop();
        }

        void FixedUpdate()
        {
            Vector2 move = GameManager.Instance.InputLocked ? Vector2.zero : GameInput.Move;
            float speed = GameManager.Instance.Skills.Has(Skill.QuickHands) ? Speed * 1.4f : Speed;
            body.linearVelocity = move * speed;
            if (Mathf.Abs(move.x) > 0.01f) spriteRenderer.flipX = move.x < 0;
        }

        Interactable FindFocus()
        {
            Interactable best = null;
            float bestScore = float.MaxValue;
            Vector2 position = transform.position;
            foreach (var candidate in Interactable.All)
            {
                float distance = Vector2.Distance(position, candidate.transform.position);
                if (distance > InteractRange || candidate.GetPrompt(this) == null) continue;
                // Was man wirklich benutzen kann, hat Vorrang vor reinen Infos.
                float score = distance + (candidate.CanInteract(this) ? 0f : InteractRange);
                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }
            return best;
        }

        public void PickUp(DeliveryBox box)
        {
            Carried = box;
            box.enabled = false;
            box.transform.SetParent(transform);
            box.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            box.GetComponent<YSort>().bias = 60;
            SoundManager.Play(Sfx.Pickup);
        }

        void Drop()
        {
            var box = Carried;
            Carried = null;
            box.transform.SetParent(WorldFactory.Root);
            box.transform.position = ShopGrid.CellToWorld(ShopGrid.WorldToCell(transform.position));
            box.GetComponent<YSort>().bias = 0;
            box.enabled = true;
            SoundManager.Play(Sfx.Drop);
        }

        public void DestroyCarried()
        {
            if (Carried == null) return;
            Destroy(Carried.gameObject);
            Carried = null;
        }
    }
}
