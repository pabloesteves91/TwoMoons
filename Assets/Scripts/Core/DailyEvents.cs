namespace TwoMoons
{
    /// <summary>Neue Ereignisse immer hinten anhängen – die Nummern stehen in Spielständen.</summary>
    public enum DayEvent { None, Rain, Rush, Influencer, Sale, Collectors }

    /// <summary>Zufalls-Ereignis des Tages (ab Tag 2): bringt Abwechslung in den Ladenalltag.</summary>
    public static class DailyEvents
    {
        public static string Title(DayEvent e)
        {
            switch (e)
            {
                case DayEvent.Rain: return "Regentag";
                case DayEvent.Rush: return "Schulschluss-Ansturm";
                case DayEvent.Influencer: return "Influencer-Besuch";
                case DayEvent.Sale: return "Großhändler-Aktion";
                case DayEvent.Collectors: return "Sammlertag";
                default: return null;
            }
        }

        public static string Description(DayEvent e)
        {
            switch (e)
            {
                case DayEvent.Rain: return "Es regnet in Strömen. Es kommen weniger Kunden – aber wer kommt, bleibt gern länger und ist geduldiger.";
                case DayEvent.Rush: return "Zwischen 13 und 15 Uhr stürmen die Schulkinder den Laden! Volle Regale und eine schnelle Kasse lohnen sich.";
                case DayEvent.Influencer: return "Eine bekannte Kartenspiel-Influencerin kommt heute vorbei. Bedien sie gut – das spricht sich herum!";
                case DayEvent.Sale: return "Der Großhändler hat Aktion: Alle Ware ist heute 20 % günstiger. Gute Gelegenheit zum Vorrat-Kaufen!";
                case DayEvent.Collectors: return "In der Stadt ist Sammlerbörse: Kunden kaufen heute ein Pack mehr, und mehr Sammler schauen in die Vitrine.";
                default: return null;
            }
        }

        /// <summary>Kurz für die Kopfzeile.</summary>
        public static string Short(DayEvent e)
        {
            switch (e)
            {
                case DayEvent.Rain: return "Regentag: weniger Kunden, aber geduldiger";
                case DayEvent.Rush: return "Ansturm 13–15 Uhr";
                case DayEvent.Influencer: return "Influencer kommt heute vorbei";
                case DayEvent.Sale: return "Großhändler: Ware −20 %";
                case DayEvent.Collectors: return "Sammlertag: +1 Pack pro Kunde";
                default: return null;
            }
        }

        /// <summary>Etwa jeder zweite Tag hat ein Ereignis.</summary>
        public static DayEvent Roll()
        {
            if (UnityEngine.Random.value < 0.45f) return DayEvent.None;
            return (DayEvent)UnityEngine.Random.Range(1, 6);
        }
    }
}
