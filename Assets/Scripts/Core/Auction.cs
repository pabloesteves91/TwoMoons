using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Ein Los bei der Sammler-Auktion: eine (meist bewertete) Karte mit Mindestgebot.</summary>
    [Serializable]
    public class AuctionLot
    {
        public int card;
        public int grade;
        /// <summary>Aktuelles Höchstgebot (Cent).</summary>
        public int price;
        /// <summary>So viel bietet der stärkste andere Sammler höchstens (geheim).</summary>
        public int rivalMax;
        /// <summary>0 = läuft, 1 = du hast gewonnen, 2 = an jemand anderen verkauft.</summary>
        public int state;
        /// <summary>Wer hält gerade das Höchstgebot? true = du.</summary>
        public bool yours;

        public CardDef Card => CardDatabase.Get(card);
        public int Value => Grading.Value(Card, grade);
        public int Step => Mathf.Max(100, Mathf.RoundToInt(Value * 0.05f / 100f) * 100);
    }

    /// <summary>
    /// Sammler-Auktion ab Level 5, jeden 7. Tag am Computer (Tab „Ware“): drei Lose mit seltenen, oft bewerteten Karten.
    /// Man bietet in Schritten, andere Sammler bieten mit, bis ihr geheimes Limit erreicht ist. Wer zuletzt bietet, gewinnt.
    /// Das Limit kann unter oder über dem Wert liegen – mal ein Schnäppchen, mal zu teuer.
    /// </summary>
    public static class Auction
    {
        public const int MinLevel = 5;
        public const int EveryDays = 7;

        static GameManager Game => GameManager.Instance;

        public static bool IsAuctionDay(int day) => Game.StoreLevel >= MinLevel && day % EveryDays == 0;

        public static int NextAuctionDay(int day) => (day / EveryDays + 1) * EveryDays;

        public static List<AuctionLot> CreateLots()
        {
            var lots = new List<AuctionLot>();
            var pool = new List<CardDef>();
            foreach (var card in CardDatabase.All) if (card.Rarity != Rarity.Common) pool.Add(card);
            for (int i = 0; i < 3; i++)
            {
                var card = pool[UnityEngine.Random.Range(0, pool.Count)];
                pool.Remove(card);
                float roll = UnityEngine.Random.value;
                int grade = roll < 0.06f ? Grading.BlackLabel : roll < 0.4f ? 10 : roll < 0.75f ? 9 : 0;
                var lot = new AuctionLot { card = card.Id, grade = grade };
                lot.price = Mathf.Max(100, Mathf.RoundToInt(lot.Value * UnityEngine.Random.Range(0.4f, 0.6f) / 100f) * 100);
                lot.rivalMax = Mathf.RoundToInt(lot.Value * UnityEngine.Random.Range(0.65f, 1.25f));
                lots.Add(lot);
            }
            return lots;
        }

        /// <summary>Du bietest einen Schritt mehr. Liegt das über dem Limit der anderen, bekommst du den Zuschlag.</summary>
        public static string Bid(AuctionLot lot)
        {
            if (lot.yours || lot.state != 0) return null;
            int bid = lot.price + lot.Step;
            if (Game.MoneyCents < bid) return "Dafür reicht dein Geld nicht.";
            lot.price = bid;
            lot.yours = true;
            if (lot.rivalMax >= bid + lot.Step)
            {
                // Ein anderer Sammler zieht nach.
                lot.price = bid + lot.Step;
                lot.yours = false;
                SoundManager.Play(Sfx.UiClick);
                return $"Ein anderer Sammler bietet {Money.Format(lot.price)}!";
            }
            // Zuschlag!
            lot.state = 1;
            Game.LoseMoney(bid);
            if (lot.grade > 0) Game.AddSlab(new Slab { card = lot.card, grade = lot.grade }, false);
            else Game.Collection.Add(lot.card);
            SoundManager.Play(Sfx.Fanfare);
            return $"Zuschlag! {lot.Card.Name} gehört dir für {Money.Format(bid)}.";
        }

        /// <summary>Aussteigen: Das Los geht an den anderen Sammler.</summary>
        public static void Pass(AuctionLot lot)
        {
            if (lot.state == 0) lot.state = 2;
        }
    }
}
