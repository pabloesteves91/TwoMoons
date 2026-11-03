using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Sehr seltener Überfall ab Level 3: Ein Gauner kommt an die Kasse und fordert Geld oder Ware – das entscheidet er.
    /// Man kann nachgeben, einmal verhandeln oder (mit Alarmanlage) den Alarmknopf drücken.
    /// Höchstens alle 10 Tage, dann 5 % pro Tag (mit Alarmanlage die Hälfte).
    /// </summary>
    public class Robbery
    {
        const int MinDaysBetween = 10;
        const float Chance = 0.05f;
        const float NegotiateSuccess = 0.6f;
        const float AlarmSuccess = 0.7f;

        public Customer Robber;
        public bool WantsGoods;
        public int Cents;
        public Product Product;
        public int Count;
        public bool Negotiated;
        public string Message;

        static GameManager Game => GameManager.Instance;

        public static bool HasAlarm => Game.Decos.Exists(d => d.Type == Furniture.Alarm);

        /// <summary>Morgens würfeln, ob heute ein Überfall passiert. Gibt die Uhrzeit zurück (-1 = keiner).</summary>
        public static int RollForToday(int lastRobberyDay)
        {
            if (Game.StoreLevel < 3 || Game.Day - lastRobberyDay < MinDaysBetween) return -1;
            if (Game.Tournament.IsTournamentDay || (Game.Guide != null && Game.Guide.Active)) return -1;
            float chance = Chance * (HasAlarm ? 0.5f : 1f);
            return Random.value < chance ? Random.Range(11 * 60, 16 * 60) : -1;
        }

        /// <summary>Der Gauner entscheidet, was er will: Geld oder die Ware, von der am meisten da ist.</summary>
        public static Robbery Create(Customer robber)
        {
            var robbery = new Robbery { Robber = robber };
            var stock = new Dictionary<Product, int>();
            foreach (var shelf in Game.Shelves)
                foreach (var pair in shelf.Contents)
                    stock[pair.Key] = (stock.TryGetValue(pair.Key, out int n) ? n : 0) + pair.Value;
            Product most = null;
            foreach (var pair in stock)
                if (most == null || pair.Value * pair.Key.MarketValue > stock[most] * most.MarketValue) most = pair.Key;

            bool canTakeMoney = Game.MoneyCents >= 2000;
            bool canTakeGoods = most != null && stock[most] >= 3;
            robbery.WantsGoods = canTakeGoods && (!canTakeMoney || Random.value < 0.5f);
            if (robbery.WantsGoods)
            {
                robbery.Product = most;
                robbery.Count = Mathf.Clamp(stock[most] / 2, 3, 12);
            }
            else
            {
                robbery.Cents = Mathf.Clamp(Mathf.RoundToInt(Game.MoneyCents * 0.25f / 100f) * 100, 1000, 30000);
                robbery.Cents = Mathf.Min(robbery.Cents, Game.MoneyCents);
            }
            return robbery;
        }

        public string Demand => WantsGoods ? $"{Count}× {Product.Name}" : Money.Format(Cents);

        /// <summary>Einmal verhandeln: meist halbiert er, sonst wird er sauer und will mehr.</summary>
        public void Negotiate()
        {
            if (Negotiated) return;
            Negotiated = true;
            if (Random.value < NegotiateSuccess)
            {
                if (WantsGoods) Count = Mathf.Max(1, Count / 2);
                else Cents = Mathf.Max(500, Cents / 2 / 100 * 100);
                Message = "„Na gut … die Hälfte. Aber schnell!“";
                Robber.Say("Na gut, die Hälfte!", 3f);
            }
            else
            {
                if (WantsGoods) Count = Mathf.Min(Count + Mathf.Max(1, Count / 5), AvailableGoods());
                else Cents = Mathf.Min(Game.MoneyCents, Mathf.RoundToInt(Cents * 1.2f / 100f) * 100);
                Message = "„Willst du mich veräppeln?! Jetzt will ich mehr!“";
                Robber.Say("Jetzt will ich mehr!", 3f);
                Game.ChangeReputation(-5);
            }
        }

        int AvailableGoods()
        {
            int total = 0;
            foreach (var shelf in Game.Shelves) total += shelf.CountOf(Product);
            return total;
        }

        /// <summary>Nachgeben: Er nimmt die Forderung (bzw. den Anteil) und haut ab.</summary>
        public void GiveIn(float share = 1f)
        {
            int cents = 0, count = 0;
            if (WantsGoods)
            {
                count = Mathf.Max(1, Mathf.RoundToInt(Count * share));
                int left = count;
                foreach (var shelf in Game.Shelves)
                {
                    if (left <= 0) break;
                    left -= shelf.Take(Product, left);
                }
                count -= Mathf.Max(0, left);
            }
            else
            {
                cents = Mathf.Min(Game.MoneyCents, Mathf.RoundToInt(Cents * share));
                Game.LoseMoney(cents);
            }
            Finish(cents, count);
        }

        /// <summary>Alarmknopf: meist flieht er ohne Beute, sonst schnappt er sich vorher die Hälfte.</summary>
        public void PressAlarm()
        {
            SoundManager.Play(Sfx.Error);
            if (Random.value < AlarmSuccess)
            {
                Robber.Say("Alarm?! Nichts wie weg!", 3f);
                Game.ChangeReputation(10);
                Game.ShowToast("Der Alarm hat den Gauner verjagt! Ruf +10", 5f);
                Finish(0, 0);
            }
            else
            {
                Robber.Say("Das nehm ich trotzdem mit!", 3f);
                Game.ShowToast("Der Gauner hat sich vor der Flucht noch etwas geschnappt.", 5f);
                GiveIn(0.5f);
            }
        }

        void Finish(int cents, int count)
        {
            Game.EndRobbery(this, cents, count, false);
        }
    }
}
