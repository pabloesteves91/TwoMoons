namespace TwoMoons
{
    /// <summary>Geldbeträge werden als Cent (int) gespeichert, damit keine Rundungsfehler entstehen.</summary>
    public static class Money
    {
        public static string Format(int cents)
        {
            string sign = cents < 0 ? "-" : "";
            int abs = System.Math.Abs(cents);
            // Tausender mit Punkt, z. B. 150.000,00 €
            string euros = (abs / 100).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
            return $"{sign}{euros},{abs % 100:00} €";
        }
    }
}
