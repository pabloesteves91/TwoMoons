using UnityEngine;

namespace TwoMoons
{
    /// <summary>Namen aller Sounds. Jeder Name gehört zu einer WAV-Datei in Assets/Art/Audio.</summary>
    public static class Sfx
    {
        public const string Register = "register";
        public const string Doorbell = "doorbell";
        public const string Pickup = "pickup";
        public const string Drop = "drop";
        public const string Stock = "stock";
        public const string Clean = "clean";
        public const string Order = "order";
        public const string Build = "build";
        public const string Error = "error";
        public const string Quest = "quest";
        public const string Fanfare = "fanfare";
        public const string DayEnd = "dayEnd";
        public const string UiClick = "uiClick";
        public const string Dialog = "dialog";
        public const string CardFlip = "cardFlip";
        public const string RareCard = "rareCard";
        public const string ShinyCard = "shinyCard";
        public const string Music = "music";
    }

    /// <summary>
    /// Alle Sounds des Spiels. Eigene Sounds einfach als WAV mit demselben Namen in Assets/Art/Audio ablegen.
    /// Leere Felder bekommen einen erzeugten 8-Bit-Platzhalter.
    /// </summary>
    [CreateAssetMenu(menuName = "TwoMoons/Audio Library", fileName = "AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("Laden")]
        public AudioClip register;
        public AudioClip doorbell;
        public AudioClip pickup;
        public AudioClip drop;
        public AudioClip stock;
        public AudioClip clean;
        public AudioClip order;
        public AudioClip build;
        public AudioClip error;

        [Header("Fortschritt")]
        public AudioClip quest;
        public AudioClip fanfare;
        public AudioClip dayEnd;

        [Header("Karten")]
        public AudioClip cardFlip;
        public AudioClip rareCard;
        public AudioClip shinyCard;

        [Header("Menü")]
        public AudioClip uiClick;
        public AudioClip dialog;

        [Header("Musik")]
        public AudioClip music;

        public static readonly string[] Keys =
        {
            Sfx.Register, Sfx.Doorbell, Sfx.Pickup, Sfx.Drop, Sfx.Stock, Sfx.Clean, Sfx.Order, Sfx.Build, Sfx.Error,
            Sfx.Quest, Sfx.Fanfare, Sfx.DayEnd, Sfx.CardFlip, Sfx.RareCard, Sfx.ShinyCard,
            Sfx.UiClick, Sfx.Dialog, Sfx.Music,
        };
    }
}
