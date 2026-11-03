using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Was Polizei und Spurensicherung nach einem Überfall liegen lassen: Absperrband, Beweis-Markierungen,
    /// umgeworfenes Zeug. Am nächsten Morgen muss alles weg, bevor der Laden öffnet.
    /// </summary>
    public class RobberyMess : Interactable
    {
        public enum Kind { Tape, Marker, Clutter }

        public static readonly List<RobberyMess> Existing = new List<RobberyMess>();

        public Kind Type;
        public Vector2Int Cell;

        void Awake() => Existing.Add(this);

        void OnDestroy() => Existing.Remove(this);

        public static string SpriteFor(Kind kind) =>
            kind == Kind.Tape ? "policeTape" : kind == Kind.Marker ? "evidenceMarker" : "wrappers";

        public override string GetPrompt(PlayerController player)
        {
            switch (Type)
            {
                case Kind.Tape: return "Absperrband abnehmen";
                case Kind.Marker: return "Beweis-Markierung einsammeln";
                default: return "Durcheinander aufräumen";
            }
        }

        public override void Interact(PlayerController player)
        {
            SoundManager.Play(Sfx.Clean);
            Destroy(gameObject);
        }
    }
}
