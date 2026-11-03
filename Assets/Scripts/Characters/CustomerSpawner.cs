using UnityEngine;

namespace TwoMoons
{
    /// <summary>Schickt während der Öffnungszeit Kunden in den Laden. Je besser der Ruf, desto mehr.</summary>
    public class CustomerSpawner : MonoBehaviour
    {
        const int MaxCustomers = 5;
        /// <summary>Anteil der Kunden, die etwas klauen wollen.</summary>
        const float ThiefChance = 0.07f;

        float timer = 3f;

        void Update()
        {
            var game = GameManager.Instance;
            if (game.Phase != GamePhase.Open && game.Phase != GamePhase.MainMenu) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            bool open = game.Phase == GamePhase.Open;
            bool influencer = open && game.InfluencerMinute >= 0 && game.ClockMinutes >= game.InfluencerMinute;
            // Sehr selten: ein Überfall (Uhrzeit wird morgens ausgewürfelt).
            if (open && game.RobberyMinute >= 0 && game.ClockMinutes >= game.RobberyMinute && game.CurrentRobbery == null)
            {
                game.RobberArrived();
                WorldFactory.SpawnCustomer().IsRobber = true;
                SoundManager.Play(Sfx.Doorbell);
            }
            // Beim Ansturm passen mehr Kunden gleichzeitig in den Laden.
            int maxCustomers = open && game.SpawnIntervalFactor < 1f ? MaxCustomers + 3 : MaxCustomers;
            if (Customer.Active.Count < maxCustomers || influencer)
            {
                var customer = WorldFactory.SpawnCustomer();
                if (influencer)
                {
                    customer.IsInfluencer = true;
                    game.InfluencerArrived();
                }
                // Ab Level 4 will ab und zu jemand Karten verkaufen.
                else if (open && game.StoreLevel >= 4 && Random.value < 0.12f) customer.IsSeller = true;
                // Ab Tag 2 versucht ab und zu jemand, Packs mitgehen zu lassen.
                else if (open && game.Day >= 2 && !(game.Guide != null && game.Guide.Active) && Random.value < ThiefChance * (game.Upgrades.Has(Upgrade.Camera) ? 0.5f : 1f)) customer.IsThief = true;
                if (game.Phase != GamePhase.MainMenu) SoundManager.Play(Sfx.Doorbell);
            }
            float reputationFactor = Mathf.Lerp(1f, 0.5f, game.ReputationStage / 4f);
            timer = Random.Range(8f, 16f) * reputationFactor * (open ? game.SpawnIntervalFactor : 1f);
        }
    }
}
