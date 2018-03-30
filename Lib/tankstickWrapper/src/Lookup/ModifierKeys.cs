using System;

namespace JonesCorp.Lookup
{
    /// <summary>
    ///     Keyboard modifier keys
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        NONE = 0,

        /// <summary>
        ///     Button A
        /// </summary>
        LEFTCTRL = 1,

        /// <summary>
        ///     Button D
        /// </summary>
        LEFTSHIFT = 1 << 1,

        /// <summary>
        ///     Button B
        /// </summary>
        LEFTALT = 1 << 2,
        LEFTWIN = 1 << 3,
        RIGHTCTRL = 1 << 4,
        RIGHTSHIFT = 1 << 5,
        RIGHTALT = 1 << 6,
        RIGHTWIN = 1 << 7
    }
}