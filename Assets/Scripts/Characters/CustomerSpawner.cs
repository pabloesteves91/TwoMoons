using UnityEngine;

namespace TwoMoons
{
    /// <summary>Schickt während der Öffnungszeit Kunden in den Laden. Je besser der Ruf, desto mehr.</summary>
    public class CustomerSpawner : MonoBehaviour
    {
        const int MaxCustomers = 5;

        float timer = 3f;

        void Update()
        {
            var game = GameManager.Instance;
            if (game.Phase != GamePhase.Open && game.Phase != GamePhase.MainMenu) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            if (Customer.Active.Count < MaxCustomers)
            {
                WorldFactory.SpawnCustomer();
                if (game.Phase != GamePhase.MainMenu) SoundManager.Play(Sfx.Doorbell);
            }
            float reputationFactor = Mathf.Lerp(1f, 0.5f, game.ReputationStage / 4f);
            timer = Random.Range(8f, 16f) * reputationFactor;
        }
    }
}
