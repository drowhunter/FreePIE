using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JonesCorp
{
    public partial class TankStickWrapper
    {
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x0005;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x0000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x0001;
        private static readonly List<IntPtr> Handles = new List<IntPtr>();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter,
            int Flags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [StructLayout(LayoutKind.Sequential)]
        private class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Guid dbcc_classguid;
            internal int dbcc_devicetype;
            internal Int16 dbcc_name;
            internal int dbcc_reserved;
            internal int dbcc_size;
        }

        #region Register for Device Notify

        /// <summary>
        ///     Register a window or service handle for device notification
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="handle"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool RegisterForDeviceNotification(IntPtr hwnd, ref IntPtr handle, bool window = true)
        {
            var classGuid = new Guid(XARCADE_INTERFACE_ONE_GUID);

            var devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
            Debug.WriteLine("Register notification {0} handle {1}", window ? "window" : "service", hwnd);
            try
            {
                var devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                var Size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size = Size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = classGuid;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                handle = RegisterDeviceNotification(hwnd, devBroadcastDeviceInterfaceBuffer,
                    window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);
                Handles.Add(handle);
                return handle != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        /// <summary>
        ///     Unregister a notification handle
        /// </summary>
        /// <param name="Handle"></param>
        /// <returns></returns>
        public static bool UnregisterNotify(IntPtr handle)
        {
            Debug.WriteLine("Unregister notification handle {0}", handle);
            try
            {
                return UnregisterDeviceNotification(handle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        /// <summary>
        ///     Unregister all registered notification handles
        /// </summary>
        /// <returns></returns>
        public static bool UnregisterNotifyAll()
        {
            var l = new IntPtr[Handles.Count];
            Handles.CopyTo(l, 0);

            foreach (var handle in l)
            {
                if (UnregisterNotify(handle))
                    Handles.Remove(handle);
            }

            return Handles.Count == 0;
        }

        #endregion
    }
}