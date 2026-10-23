using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Alles, womit die Spielerfigur über [E] interagieren kann.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();

        /// <summary>Text für den Hinweis unten am Bildschirm. Null = gerade nicht ansprechbar.</summary>
        public abstract string GetPrompt(PlayerController player);

        /// <summary>False = der Hinweis ist nur eine Info, [E] tut nichts.</summary>
        public virtual bool CanInteract(PlayerController player) => true;

        public abstract void Interact(PlayerController player);

        protected virtual void OnEnable() => All.Add(this);

        protected virtual void OnDisable() => All.Remove(this);
    }
}
