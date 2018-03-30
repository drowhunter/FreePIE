using System;

namespace JonesCorp.Lookup
{
    // <summary>
    // ==============================================================================
    // ACTUAL LAYOUT
    // -----------------------------------------------------------------------------
    // 
    //								        (1)   (2)
    //			  (U)	(LCTrl) (LAlt) (Space)          (R)     (A)(S)(Q)
    //			(L) (R)	(LShift)  (Z)    (X)         (D)  (G)   (W)(E)([)
    //	(LMB)	  (D)	(C)(5)                          (F)     (])(6)      (RMB)
    // (3)                                                                 (4)
    // 
    // </summary>            
    [Flags]
    internal enum TankStickButtonEnum : uint
    {
        NONE = 0,

        // 
        //								(START)   
        //			  (U)	(1) (2) (3)    
        //			(L) (R)	(4) (5) (6)      
        //	    	  (D)	(7) (8)                    
        // (COIN)                                        
        // 

        #region  Player 1

        /// <summary>
        ///     LCtrl
        /// </summary>
        P1_1 = 1 << 0,

        /// <summary>
        ///     LAlt
        /// </summary>
        P1_2 = 1 << 1,

        /// <summary>
        ///     Space
        /// </summary>
        P1_3 = 1 << 2,

        /// <summary>
        ///     LShift
        /// </summary>
        P1_4 = 1 << 3,

        /// <summary>
        ///     Z
        /// </summary>
        P1_5 = 1 << 4,

        /// <summary>
        ///     X
        /// </summary>
        P1_6 = 1 << 5,


        /// <summary>
        ///     C
        /// </summary>
        P1_7 = 1 << 6,

        /// <summary>
        ///     D5
        /// </summary>
        P1_8 = 1 << 7,

        /// <summary>
        ///     D3
        /// </summary>
        P1_9 = 1 << 8,

        /// <summary>
        ///     D1
        /// </summary>
        P1_10 = 1 << 9,

        /// <summary>
        ///     Up
        /// </summary>
        P1_Up = 1 << 10,

        /// <summary>
        ///     Down
        /// </summary>
        P1_Down = 1 << 11,

        /// <summary>
        ///     Left
        /// </summary>
        P1_Left = 1 << 12,

        /// <summary>
        ///     Right
        /// </summary>
        P1_Right = 1 << 13,

        #endregion

        // 
        //	(START)   
        //			  (U)	(1) (2) (3)    
        //			(L) (R)	(4) (5) (6)      
        //		       (D)	(7) (8)                    
        //                              (COIN)                                        
        // 

        #region Player 2

        /// <summary>
        ///     [
        /// </summary>
        P2_1 = 1 << 14,

        /// <summary>
        ///     S
        /// </summary>
        P2_2 = 1 << 15,

        /// <summary>
        ///     Q
        /// </summary>
        P2_3 = 1 << 16,

        /// <summary>
        ///     W
        /// </summary>
        P2_4 = 1 << 17,

        /// <summary>
        ///     E
        /// </summary>
        P2_5 = 1 << 18,

        /// <summary>
        ///     [
        /// </summary>
        P2_6 = 1 << 19,

        /// <summary>
        ///     ]
        /// </summary>
        P2_7 = 1 << 20,

        /// <summary>
        ///     6
        /// </summary>
        P2_8 = 1 << 21,

        /// <summary>
        ///     4
        /// </summary>
        P2_9 = 1 << 22,

        /// <summary>
        ///     2
        /// </summary>
        P2_10 = 1 << 23,

        /// <summary>
        ///     R
        /// </summary>
        P2_Up = 1 << 24,

        /// <summary>
        ///     F
        /// </summary>
        P2_Down = 1 << 23,

        /// <summary>
        ///     D
        /// </summary>
        P2_Left = 1 << 24,

        /// <summary>
        ///     G
        /// </summary>
        P2_Right = 1 << 25

        #endregion

        ///// <summary>
        ///// LMB / RMB
        ///// </summary>
        //LS = 1 << 14
    }
}