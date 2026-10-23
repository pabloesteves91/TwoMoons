namespace TwoMoons
{
    /// <summary>Geldbeträge werden als Cent (int) gespeichert, damit keine Rundungsfehler entstehen.</summary>
    public static class Money
    {
        public static string Format(int cents)
        {
            string sign = cents < 0 ? "-" : "";
            int abs = System.Math.Abs(cents);
            return $"{sign}{abs / 100},{abs % 100:00} €";
        }
    }
}
