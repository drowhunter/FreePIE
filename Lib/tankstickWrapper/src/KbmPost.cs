using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JonesCorp
{
    public class KbmPost
    {
        public enum MouseButtons { Left = 0x0002, Right = 0x0008, Middle = 0x0020 };

        protected const int MOUSE_VWHEEL = 0x0800;
        protected const int MOUSE_HWHEEL = 0x1000;
        protected const int WHEEL_DELTA = 120;

        protected const int MOUSE_MOVE = 1;

        protected const int VK_STANDARD = 0;
        protected const int VK_EXTENDED = 1;

        protected const int VK_KEYDOWN = 0;
        protected const int VK_KEYUP = 2;


        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 MapVirtualKeyW(UInt32 uCode, UInt32 uMapType);

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, Keys wParam, IntPtr lParam);

        //[DllImport("User32.dll")]
        //public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);



        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static bool RunningAsService()
        {
            bool retval = false;

            var name = System.Reflection.Assembly.GetEntryAssembly().GetName();

            retval = name.Name == "XInputArcadeService";

            return retval;
        }


        public static void Key(Keys Key, bool bExtended, bool bDown)
        {
            //if(RunningAsService())
                //PostMessage(GetForegroundWindow(), (bDown ? WM_KEYDOWN : WM_KEYUP), Key, IntPtr.Zero);
            //else
                keybd_event((byte)Key, (byte)MapVirtualKeyW((UInt32)Key, 0), (bDown ? VK_KEYDOWN : VK_KEYUP) | (bExtended ? VK_EXTENDED : VK_STANDARD), IntPtr.Zero);


        }

        public static void Move(int dx, int dy)
        {
            mouse_event(MOUSE_MOVE, dx, dy, 0, IntPtr.Zero);
        }

        public static void Button(MouseButtons Button, bool bDown)
        {
            mouse_event(bDown ? (int)Button : (int)Button << 1, 0, 0, 0, IntPtr.Zero);
        }

        public static void Wheel(bool bVertical, int Clicks)
        {
            mouse_event(bVertical ? MOUSE_VWHEEL : MOUSE_HWHEEL, 0, 0, Clicks * WHEEL_DELTA, IntPtr.Zero);
        }
    }
}
