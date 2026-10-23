using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoMoons
{
    /// <summary>Tastatur, Maus und Controller an einer Stelle.</summary>
    public static class GameInput
    {
        public static Vector2 Move
        {
            get
            {
                Vector2 move = Vector2.zero;
                var k = Keyboard.current;
                if (k != null)
                {
                    if (k.aKey.isPressed || k.leftArrowKey.isPressed) move.x -= 1;
                    if (k.dKey.isPressed || k.rightArrowKey.isPressed) move.x += 1;
                    if (k.sKey.isPressed || k.downArrowKey.isPressed) move.y -= 1;
                    if (k.wKey.isPressed || k.upArrowKey.isPressed) move.y += 1;
                }
                var g = Gamepad.current;
                if (g != null)
                {
                    Vector2 stick = g.leftStick.ReadValue();
                    if (stick.sqrMagnitude > 0.04f) move = stick;
                }
                return Vector2.ClampMagnitude(move, 1f);
            }
        }

        public static bool InteractPressed =>
            (Keyboard.current?.eKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);

        public static bool DropPressed =>
            (Keyboard.current?.qKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.buttonWest.wasPressedThisFrame ?? false);

        public static bool BuildPressed =>
            (Keyboard.current?.bKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.buttonNorth.wasPressedThisFrame ?? false);

        public static bool CancelPressed =>
            (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false);

        public static bool QuestTogglePressed =>
            (Keyboard.current?.tabKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.selectButton.wasPressedThisFrame ?? false);

        public static bool FullscreenTogglePressed => Keyboard.current?.f11Key.wasPressedThisFrame ?? false;

        static int cancelConsumedFrame = -1;

        /// <summary>
        /// Esc/Zurück nur einmal pro Frame auswerten, damit z. B. das Schließen des Bau-Modus
        /// nicht gleichzeitig das Einstellungsmenü öffnet.
        /// </summary>
        public static bool ConsumeCancel()
        {
            if (!CancelPressed || cancelConsumedFrame == Time.frameCount) return false;
            cancelConsumedFrame = Time.frameCount;
            return true;
        }

        /// <summary>Weiter in Dialogen: Leertaste, Enter, E, Mausklick oder A-Taste.</summary>
        public static bool ConfirmPressed =>
            (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) ||
            (Keyboard.current?.enterKey.wasPressedThisFrame ?? false) ||
            (Keyboard.current?.eKey.wasPressedThisFrame ?? false) ||
            (Mouse.current?.leftButton.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);

        public static Vector2 MousePosition => Mouse.current?.position.ReadValue() ?? Vector2.zero;

        public static bool ClickPressed => Mouse.current?.leftButton.wasPressedThisFrame ?? false;

        public static bool RightClickPressed => Mouse.current?.rightButton.wasPressedThisFrame ?? false;
    }
}
