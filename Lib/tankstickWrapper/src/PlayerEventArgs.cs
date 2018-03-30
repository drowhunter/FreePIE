using System;
using JonesCorp.Lookup;

namespace JonesCorp
{
    public sealed class PlayerEventArgs : EventArgs
    {
        /// <summary>
        ///     the buttons whose has changed since last read
        /// </summary>
        private readonly TankStickButton _differentButtons;

        /// <summary>
        ///     The are currently held
        /// </summary>
        public readonly TankStickButton Buttons;

        public readonly int Index;

        public PlayerEventArgs(int index, TankStickButton buttons, TankStickButton differentButtons)
        {
            Index = index;
            Buttons = buttons;
            _differentButtons = differentButtons;
        }

        /// <summary>
        ///     Get the buttons that were just pressed
        /// </summary>
        /// <returns></returns>
        public TankStickButton JustPressed()
        {
            return Buttons & _differentButtons;
        }

        /// <summary>
        ///     Get the buttons that were just released
        /// </summary>
        /// <returns></returns>
        public TankStickButton JustReleased()
        {
            return ~Buttons & _differentButtons;
        }
    }
}