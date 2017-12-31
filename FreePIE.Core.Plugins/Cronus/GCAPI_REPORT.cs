using System;
using System.Drawing;
using System.Runtime.InteropServices;
//using int16_t = System.Int16;
//using uint16_t = System.UInt16;

namespace FreePIE.Core.Plugins.Cronus
{//, Size = 252



    [StructLayout(LayoutKind.Sequential)]
    public struct GCAPI_REPORT
    {


        /// <summary>
        /// Receives values established by the #defines CONSOLE_*
        /// </summary>
        public byte console;

        /// <summary>
        /// Values from #defines CONTROLLER_* and EXTENSION_*
        /// </summary>
        public byte controller;

        /// <summary>
        /// Four LED - #defines LED_*
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] led;

        /// <summary>
        /// Two rumbles - Range: [0 ~ 100] %
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] rumble;

        /// <summary>
        /// Battery level - Range: [0 ~ 10] 0 = empty, 10 = full
        /// </summary>
        public byte battery_level;

        /// <summary>
        /// Input structure (for controller entries) 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.GCAPI_INPUT_TOTAL)]
        public Input[] inputs;

        #region Public Get Methods

        public Point GetPoint<T1, T2>(T1 inputx, T2 inputy)
            where T1 : struct, IComparable, IFormattable, IConvertible
            where T2 : struct, IComparable, IFormattable, IConvertible
        {
            Point p = new Point { X = GetValue(inputx), Y = GetValue(inputy) };
            return p;
        }

        public Point GetPreviousPoint<T1, T2>(T1 inputx, T2 inputy)
            where T1 : struct, IComparable, IFormattable, IConvertible
            where T2 : struct, IComparable, IFormattable, IConvertible
        {
            Point p = new Point { X = GetPrevious(inputx), Y = GetPrevious(inputy) };
            return p;
        }

        public Point GetDeltaPoint<T1, T2>(T1 inputx, T2 inputy)
            where T1 : struct, IComparable, IFormattable, IConvertible
            where T2 : struct, IComparable, IFormattable, IConvertible
        {
            Point p = new Point { X = GetDelta(inputx), Y = GetDelta(inputy) };
            return p;
        }

        /// <summary>
        /// Get the value of the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public int GetValue<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp;
            if (!typeof(int).IsAssignableFrom(typeof(T)))
                inp = checkEnum(input);
            else
                inp = Convert.ToInt32(input);

            return inputs[inp].value;
        }
        /// <summary>
        /// Get the previous value of the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public int GetPrevious<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp;

            if (!typeof(int).IsAssignableFrom(typeof(T)))
                inp = checkEnum(input);
            else
                inp = Convert.ToInt32(input);

            return inputs[inp].prev_value;
        }

        /// <summary>
        /// Get is the input is currently held
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool isHeld<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp = checkEnum(input);
            return inputs[inp].isHeld;
        }

        public bool wasPressed<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp = checkEnum(input);
            return inputs[inp].wasPressed;
        }

        public bool wasReleased<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp = checkEnum(input);
            return inputs[inp].wasReleased;

        }

        
        /// <summary>
        /// Get the change in value of the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public int GetDelta<T>(T input)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            int inp = checkEnum(input);
            return inputs[inp].delta;
        }

        
        #endregion

        #region Private Methods

        /// <summary>
        /// Check that an object is an enum or throw an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        private static int checkEnum<T>(T t)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            Type typ = typeof(T);
            if (!typ.IsEnum ||
                !(typ == typeof(PS4) ||
                  //typ == typeof(PS3) ||
                  typ == typeof(XB1) ||
                  typ == typeof(XB360) ||
                  typ == typeof(WII) ||
                  typ == typeof(WIICLASSIC)
                    )
                )
            {
                if (!typeof(int).IsAssignableFrom(t.GetType()))
                    throw new ArgumentException("Type must be an enumeration or int");
            }
            return Convert.ToInt32(t);
        }

        #endregion

    }


}
