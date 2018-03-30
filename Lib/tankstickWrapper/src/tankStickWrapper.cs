using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JonesCorp.Lookup;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace JonesCorp
{
    public partial class TankStickWrapper : ItankStickWrapper
    {
        public const string XARCADE_INTERFACE_ONE_GUID = "{4F37DBAD-4AF1-42E4-9457-44C8D0D63E17}";
        public const string XARCADE_INTERFACE_TWO_GUID = "{3D97CA50-8044-4CE5-8CB6-6DCC299C7A1D}";
        public const int XARCADE_VENDOR_ID = 0xAA55;
        public const int XARCADE_PRODUCT_ID = 0x0101;

        /// <summary>
        ///     holds the endpoint readers of the xarcade devices
        /// </summary>
        private readonly List<UsbEndpointReader> _endpointReaders = new List<UsbEndpointReader>();

        /// <summary>
        ///     holds the found xarcade devices
        /// </summary>
        private readonly List<UsbDevice> _myUsbDevices = new List<UsbDevice>();

        //private XArcadeButton _cachedSnapshot = XArcadeButton.NONE;
        /// <summary>
        ///     The combined raw buffer from the xarcade unit
        ///     |===========================================|
        ///     Bytes - | 0 - 1                 | 2 - 14            |
        ///     |--------------------------------------------
        ///     | Modifiers Flags (byte)|Rollover key codes |
        ///     |===========================================|
        /// </summary>
        private readonly byte[] _unifiedRolloverBuffer = new byte[14];

        private readonly object keyLock = new object();

        /// <summary>
        ///     holds the previous bitmap of pressed keys
        /// </summary>
        //private XArcadeButtonSingle[] _cachedSnapshot = { XArcadeButtonSingle.NONE, XArcadeButtonSingle.NONE } ;
        private readonly TankStickButton[] _cachedSnapshots = new TankStickButton[2];

        /// <summary>
        ///     the allows the cancellation of the task
        /// </summary>
        private CancellationTokenSource _cancelInputWorkerTokenSource;

        /// <summary>
        ///     Occurs when data arrives from the joystick
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerData;


        /// <summary>
        ///     signifies that the library is connected and running
        /// </summary>
        public bool IsRunning
        {
            get { return XState == XState.Running; }
        }

        /// <summary>
        ///     the driver is stopped
        /// </summary>
        public bool IsStopped
        {
            get { return XState == XState.Stopped; }
        }

        /// <summary>
        ///     the driver is opened but not running
        /// </summary>
        public bool IsOpened
        {
            get { return XState == XState.Opened; }
        }

        /// <summary>
        ///     The current state that the library is in
        /// </summary>
        public XState XState { get; private set; }


        /// <summary>
        ///     Open all handles and claim any interfaces
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            Console.WriteLine("Xarcade device Opening...");
            try
            {
                if (XState == XState.Stopped && _myUsbDevices.Count == 0)
                {
                    Console.WriteLine("Searching for xArcade devices ...");
                    var devices = (from d in UsbDevice.AllDevices
                        where d.Vid == XARCADE_VENDOR_ID && d.Pid == XARCADE_PRODUCT_ID
                        select d).ToList();

                    if (devices.Count > 0)
                        Console.WriteLine("Found ({0}) xArcade devices.", devices.Count);
                    else
                        Console.WriteLine("Found ({0}) xArcade devices.", devices.Count);

                    for (var index = 0; index < devices.Count; index++)
                    {
                        var foundDevice = devices[index];
                        UsbDevice d;
                        Console.WriteLine("Opening device number ({0})...", index + 1);
                        if (foundDevice.Open(out d) && d != null)
                        {
                            Console.WriteLine("Opened device number ({0})", index + 1);
                            _myUsbDevices.Add(d);
                        }
                        else
                        {
                            var a = foundDevice.IsAlive;
                            Console.WriteLine("but could not open xarcade device ({0}) {1}.Is it Alive? {2}",
                                index, foundDevice.Name, a);
                            if (a)
                            {
                                
                            }
                            break;
                        }
                    }

                    //LogDebug("Devices Not Found.");
                    if (devices.Count > 0 && _myUsbDevices.Count == devices.Count)
                    {
                        foreach (var dev in _myUsbDevices)
                        {
                            // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                            // it exposes an IUsbDevice interface. If not (WinUSB) the 
                            // 'wholeUsbDevice' variable will be null indicating this is 
                            // an interface of a device; it does not require or support 
                            // configuration and interface selection.
                            var wholeUsbDevice = dev as IUsbDevice;
                            if (!ReferenceEquals(wholeUsbDevice, null))
                            {
                                // This is a "whole" USB device. Before it can be used, 
                                // the desired configuration and interface must be selected.

                                // Select config #1
                                if (wholeUsbDevice.SetConfiguration(1))
                                {
                                    int id = wholeUsbDevice.Configs[0].InterfaceInfoList[0].Descriptor.InterfaceID;

                                    // Claim interface #0.
                                    if (wholeUsbDevice.ClaimInterface(id))
                                        XState = XState.Opened;
                                }
                            }
                            else
                                XState = XState.Opened;


                            if (XState != XState.Opened)
                                break;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                throw;
                Console.WriteLine("Error {0}\r\n\r\n{1}", x.Message, x.StackTrace);
            }

            if (XState == XState.Opened)
                Console.WriteLine("Xarcade device Opened");
            else
                Console.WriteLine("Open Xarcade Failed");

            return IsOpened;
        }

        /// <summary>
        ///     Start Workers and receieving of data
        /// </summary>
        /// <returns></returns>
        public bool Start(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Xarcade device Starting...");
            _cancelInputWorkerTokenSource = tokenSource;

            if (XState == XState.Opened)
            {
                try
                {
                    // Find the start the two Opened xarcade interfaces subscribe to them
                    _myUsbDevices.ForEach(dev =>
                    {
                        if (dev == null)
                            throw new Exception("one of the usb devices was null during Start");

                        var epid = (ReadEndpointID)
                            dev.Configs[0].InterfaceInfoList[0].EndpointInfoList[0].Descriptor.EndpointID;

                        var ur = dev.OpenEndpointReader(epid);

                        if (ur != null)
                        {
                            if ((ReadEndpointID) dev.ActiveEndpoints[0].EpNum == ReadEndpointID.Ep01)
                                ur.DataReceived += Endpoint1_DataReceived;

                            else if ((ReadEndpointID) dev.ActiveEndpoints[0].EpNum == ReadEndpointID.Ep02)
                                ur.DataReceived += Endpoint2_DataReceieved;

                            int maxbytes = ur.EndpointInfo.Descriptor.MaxPacketSize;
                            ur.ReadBufferSize = maxbytes;
                            ur.DataReceivedEnabled = true;

                            _endpointReaders.Add(ur);
                        }
                    });

                    XState = XState.Running;

                    BeginPoll(tokenSource.Token);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Error {0}\r\n\r\n{1}", x.Message, x.StackTrace);
                }
            }
            if (!IsRunning)
                Console.WriteLine("Xarcade device not started.");
            else
                Console.WriteLine("Xarcade device Started.");
            return IsRunning;
        }


        /// <summary>
        ///     Stop Workers and receieving of data
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (IsRunning)
            {
                Console.WriteLine("Xarcade device Stopping .. ");
                try
                {
                    _cancelInputWorkerTokenSource?.Cancel();

                    _endpointReaders.ForEach(ur =>
                    {
                        ur.DataReceivedEnabled = false;
                        ur.DataReceived -= Endpoint1_DataReceived;
                        ur.DataReceived -= Endpoint2_DataReceieved;
                    });

                    _endpointReaders.Clear();

                    XState = XState.Opened;
                }
                catch (Exception)
                {
                    //throw;
                }
            }
            else
            {
                Console.WriteLine("Xarcade device wasn't running .  Skipped Stop .. ");
                return true;
            }

            if (IsOpened)
                Console.WriteLine("Xarcade device Stopped .. ");
            else
                Console.WriteLine("Failed to Stop Xarcade device  .. ");

            return IsOpened;
        }

        /// <summary>
        ///     Close all open handles and release interfaces and resources
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (IsRunning)
                throw new Exception("Cannot close xarcade while it is running. You must called Stop first");
            if (IsStopped)
            {
                Console.WriteLine("Xarcade device was already stopped skipping .. ");
            }
            else
            {
                Console.WriteLine("Xarcade device Closing .. ");

                try
                {
                    _myUsbDevices.Where(d => d != null).ToList().ForEach(dev =>
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        var wholeUsbDevice = dev as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        if (dev.IsOpen && dev.Close())
                            dev = null;
                    });


                    var opendevices = _myUsbDevices.Where(dev => dev != null && dev.IsOpen).ToList();

                    if (opendevices.Count > 0)
                    {
                        Console.WriteLine("Attempt to close arcade devices while some of them were still open!");
                        //return false;
                    }

                    try
                    {
                        Console.WriteLine(" Free xarcade usb resources .. ");
                        UsbDevice.Exit();
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(" Error calling UsbDevice.Exit ...");
                        throw;
                    }
                    finally
                    {
                        _myUsbDevices.RemoveAll(dev => dev != null);
                    }

                    XState = XState.Stopped;
                }
                catch (Exception x)
                {
                    Console.WriteLine("Error Closing {0}", x);
                }
            }

            if (IsStopped)
                Console.WriteLine("Xarcade device Closed .. ");
            else
                Console.WriteLine("Failed to Close Xarcade device  .. ");

            return IsStopped;
        }


        /// <summary>
        ///     Stops all workers and data points and releases all usb resources
        ///     in other words it calls Stop() then Close()
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Disposing XArcadeDriver");
            Stop();
            Close();
            UnregisterNotifyAll();
        }

        public bool StartPoll()
        {
            Console.WriteLine("Xarcade device Starting...");

            _cancelInputWorkerTokenSource = new CancellationTokenSource();

            if (XState == XState.Opened)
            {
                try
                {
                    // Find the start the two Opened xarcade interfaces subscribe to them
                    _myUsbDevices.ForEach(dev =>
                    {
                        if (dev == null)
                            throw new Exception("one of the usb devices was null during Start");

                        var epid = (ReadEndpointID)
                            dev.Configs[0].InterfaceInfoList[0].EndpointInfoList[0].Descriptor.EndpointID;

                        var ur = dev.OpenEndpointReader(epid);

                        if (ur != null)
                        {
                            if ((ReadEndpointID) dev.ActiveEndpoints[0].EpNum == ReadEndpointID.Ep01)
                                ur.DataReceived += Endpoint1_DataReceived;

                            else if ((ReadEndpointID) dev.ActiveEndpoints[0].EpNum == ReadEndpointID.Ep02)
                                ur.DataReceived += Endpoint2_DataReceieved;

                            int maxbytes = ur.EndpointInfo.Descriptor.MaxPacketSize;
                            ur.ReadBufferSize = maxbytes;
                            ur.DataReceivedEnabled = true;

                            _endpointReaders.Add(ur);
                        }
                    });

                    XState = XState.Running;

                    //InputWorker_DoWork(_cancelInputWorkerTokenSource.Token);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Error {0}\r\n\r\n{1}", x.Message, x.StackTrace);
                }
            }
            if (!IsRunning)
                Console.WriteLine("Xarcade device not started.");
            else
                Console.WriteLine("Xarcade device Started.");
            return IsRunning;
        }

        #region Private Functions

        /// <summary>
        ///     Get the bitmask of held keys from the xarcade
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private TankStickButtonEnum GetRollovers(byte[] buffer)
        {
            var retval = TankStickButtonEnum.NONE;

            var pressedKeys = (from bite in buffer.Skip(2)
                where bite != 0
                select (HidUsageKeycode) bite).ToList();

            var m = (ModifierKeys) ((buffer[0] << 0) | (buffer[1] << 8));

            if (m.HasFlag(ModifierKeys.LEFTCTRL)) pressedKeys.Add(HidUsageKeycode.LCTRL);
            if (m.HasFlag(ModifierKeys.LEFTALT)) pressedKeys.Add(HidUsageKeycode.LALT);
            if (m.HasFlag(ModifierKeys.LEFTSHIFT)) pressedKeys.Add(HidUsageKeycode.LSHIFT);

            lock (keyLock)
            {
                //retval = pressedKeys.Select(pk => Array.IndexOf(KeyMap, pk)).Where(index => index > -1).Aggregate(retval, (current, index) => current | (XArcadeButton) (1 << index));
                pressedKeys.ForEach(k =>
                {
                    var position = Array.IndexOf(HIDButtonPosition, k);
                    if (position > -1)
                        retval |= (TankStickButtonEnum) (1 << position);
                });
            }

            return retval;
        }

        /// <summary>
        ///     The keyboard keys physically mapped to button position
        ///     This is important because it tells us what position incoming keyboard keys are int the
        ///     <see cref="TankStickButtonEnum" /> enumeration
        /// </summary>
        private readonly HidUsageKeycode[] HIDButtonPosition =
        {
            //player 1
            HidUsageKeycode.LCTRL, HidUsageKeycode.LALT, HidUsageKeycode.SPACE,
            HidUsageKeycode.LSHIFT, HidUsageKeycode.Z, HidUsageKeycode.X,
            HidUsageKeycode.C, HidUsageKeycode.D5,
            HidUsageKeycode.D3, HidUsageKeycode.D1,
            HidUsageKeycode.UP, HidUsageKeycode.DOWN, HidUsageKeycode.LEFT, HidUsageKeycode.RIGHT,

            // player 2
            HidUsageKeycode.A, HidUsageKeycode.S, HidUsageKeycode.Q,
            HidUsageKeycode.W, HidUsageKeycode.E, HidUsageKeycode.LEFTBRACKET,
            HidUsageKeycode.RIGHTBRACKET, HidUsageKeycode.D6,
            HidUsageKeycode.D4, HidUsageKeycode.D2,
            HidUsageKeycode.R, HidUsageKeycode.F, HidUsageKeycode.D, HidUsageKeycode.G
        };

        /// <summary>
        ///     Convert a two player enum to a single player enum
        /// </summary>
        /// <param name="buttonEnum">an XArcadeButton Flag Enum</param>
        /// <param name="padId">0 based index of player</param>
        /// <returns></returns>
        private TankStickButton ToSingle(TankStickButtonEnum buttonEnum, int padId = 0)
        {
            var ps = (short) (((int) buttonEnum >> (padId*14)) & 0x3FFF);
            return (TankStickButton) ps;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///     Quickly copy the data into the unified buffer when keypresses come in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Endpoint1_DataReceived(object sender, EndpointDataEventArgs e)
        {
            lock (_unifiedRolloverBuffer)
                Array.Copy(e.Buffer, 0, _unifiedRolloverBuffer, 0, 8);
        }

        /// <summary>
        ///     Quickly copy the data into the unified buffer when keypresses come in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Endpoint2_DataReceieved(object sender, EndpointDataEventArgs e)
        {
            lock (_unifiedRolloverBuffer)
                Array.Copy(e.Buffer, 2, _unifiedRolloverBuffer, 8, 6);
        }

        #endregion
    }
}