using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Cronus
{
    
    [GlobalEnum,Flags]
    public enum InputPortState : byte
    {
        CONTROLLER_DISCONNECTED    = 0x00,
        CONTROLLER_PS3         =     0x10,
        CONTROLLER_XB360        =    0x20,
        CONTROLLER_WII     =         0x30,
        CONTROLLER_PS4    =          0x40,
        CONTROLLER_XB1     =         0x50,
        EXTENSION_NUNCHUK  =         0x01,
        EXTENSION_CLASSIC     =      0x02,
    }
    [GlobalEnum]
    public enum OutputPortState :byte
    {
         CONSOLE_DISCONNECTED      =     0,
         CONSOLE_PS3               =     1,
         CONSOLE_XB360             =     2,
         CONSOLE_PS4               =     3,
         CONSOLE_XB1                =    4,
    }

    public enum LedState : byte
    {
        LED_OFF,
        LED_ON,
        LED_FAST,
        LED_SLOW,
    }

    public enum TRACE : byte
    {
        TRACE_1 = 30,
        TRACE_2 = 31,
        TRACE_3 = 32,
        TRACE_4 = 33,
        TRACE_5 = 34,
        TRACE_6 = 35
    }

    [GlobalEnum]
    public enum WIICLASSIC
    {
        HOME = 0,
        MINUS = 1,
        PLUS = 2,
        RT = 3,
        ZR = 4,
        LT = 6,
        ZL = 7,
        RX = 9,
        RY = 10,
        LX = 11,
        LY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,
        X = 17,
        B = 18,
        A = 19,
        Y = 20
    }
    [GlobalEnum]
    public enum WII
    {
        HOME = 0,
        MINUS = 1,
        PLUS = 2,
        ONE = 5,
        C = 6,
        Z = 7,
        TWO = 8,
        NX = 11,
        NY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,

        B = 18,
        A = 19,

        ACCX = 21,
        ACCY = 22,
        ACCZ = 23,

        ACCNX = 25,
        ACCNY = 26,
        ACCNZ = 27,
        IRX = 28,
        IRY = 29
    }
    [GlobalEnum]
    public enum PS3
    {
        PS = 0,
        SELECT = 1,
        START = 2,
        R1 = 3,
        R2 = 4,
        R3 = 5,
        L1 = 6,
        L2 = 7,
        L3 = 8,
        RX = 9,
        RY = 10,
        LX = 11,
        LY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,
        TRIANGLE = 17,
        CIRCLE = 18,
        CROSS = 19,
        SQUARE = 20,
        ACCX = 21,
        ACCY = 22,
        ACCZ = 23,
        GYRO = 24
    }
    [GlobalEnum]
    public enum PS4
    {
        PS = 0,
        SHARE = 1,
        OPTIONS = 2,
        R1 = 3,
        R2 = 4,
        R3 = 5,
        L1 = 6,
        L2 = 7,
        L3 = 8,
        RX = 9,
        RY = 10,
        LX = 11,
        LY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,
        TRIANGLE = 17,
        CIRCLE = 18,
        CROSS = 19,
        SQUARE = 20,
        ACCX = 21,
        ACCY = 22,
        ACCZ = 23,
        GYROX = 24,
        GYROY = 25,
        GYROZ = 26,
        TOUCH = 27,
        TOUCHX = 28,
        TOUCHY = 29
    }
    [GlobalEnum]
    public enum XB360
    {
        XBOX = 0,
        BACK = 1,
        START = 2,
        RB = 3,
        RT = 4,
        RS = 5,
        LB = 6,
        LT = 7,
        LS = 8,
        RX = 9,
        RY = 10,
        LX = 11,
        LY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,
        Y = 17,
        B = 18,
        A = 19,
        X = 20
    }
    [GlobalEnum]
    public enum XB1
    {
        XBOX = 0,
        VIEW = 1,
        MENU = 2,
        RB = 3,
        RT = 4,
        RS = 5,
        LB = 6,
        LT = 7,
        LS = 8,
        RX = 9,
        RY = 10,
        LX = 11,
        LY = 12,
        UP = 13,
        DOWN = 14,
        LEFT = 15,
        RIGHT = 16,
        Y = 17,
        B = 18,
        A = 19,
        X = 20
    }
}
