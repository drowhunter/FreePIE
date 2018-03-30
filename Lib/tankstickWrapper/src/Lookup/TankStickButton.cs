using System;

namespace JonesCorp.Lookup
{
    /// <summary>
    ///     ==============================================================================
    ///     BUTTON LAYOUT
    ///     -------------------------------------------------------------------------------
    ///     (START-10)
    ///     (U-11)	    (X-1) (Y-2) (LT-3)
    ///     (L-13) (R-14)	(A-4) (B-5) (RT-6)
    ///     (D-12)	    (LB-7)(RB-8)
    ///     (BACK-9)
    /// </summary>
    [Flags]
    public enum TankStickButton : uint
    {
        NONE = 0,

        /// <summary>
        ///     LCtrl / [ A
        /// </summary>
        Button1 = 1 << 0,

        /// <summary>
        ///     LAlt / S
        /// </summary>
        Button2 = 1 << 1,

        /// <summary>
        ///     Space / Q
        /// </summary>
        Button3 = 1 << 2,

        /// <summary>
        ///     LShift / W
        /// </summary>
        Button4 = 1 << 3,

        /// <summary>
        ///     Z / E
        /// </summary>
        Button5 = 1 << 4,

        /// <summary>
        ///     X / [
        /// </summary>
        Button6 = 1 << 5,


        /// <summary>
        ///     C / ]
        /// </summary>
        Button7 = 1 << 6,

        /// <summary>
        ///     5 / 6
        /// </summary>
        Button8 = 1 << 7,

        /// <summary>
        ///     3 / 4
        /// </summary>
        Button9 = 1 << 8,

        /// <summary>
        ///     1 / 2
        /// </summary>
        Button10 = 1 << 9,

        /// <summary>
        ///     Up / R
        /// </summary>
        Up = 1 << 10,

        /// <summary>
        ///     Down / F
        /// </summary>
        Down = 1 << 11,

        /// <summary>
        ///     Left / D
        /// </summary>
        Left = 1 << 12,

        /// <summary>
        ///     Right / G
        /// </summary>
        Right = 1 << 13
    }
}