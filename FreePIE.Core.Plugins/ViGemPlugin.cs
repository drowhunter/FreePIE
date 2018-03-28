﻿using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(DS4Global), IsIndexed = true)]
    public class DualShock4Plugin : ViGemPlugin
    {
        private List<DS4Global> _globals;
        public override object CreateGlobal()
        {
            _globals = new List<DS4Global>();
            return new GlobalIndexer<DS4Global>(CreateGlobal);
        }

        private DS4Global CreateGlobal(int index)
        {
            var global = new DS4Global(index, this);
            _globals.Add(global);

            return global;
        }
    }

    [GlobalType(Type = typeof(XboxGlobal), IsIndexed = true)]
    public abstract class ViGemPlugin : Plugin
    {
        /// <summary>
        /// Indicates what went wrong
        /// </summary>
        public enum ErrorState
        {
            OK = 0,
            OPEN_FAILED,
            CLOSE_FAILED,
            PLUG_FAILED,
            UNPLUG_FAILED,
            
        }

        private List<XboxGlobal> _globals;
        public ViGEmClient Client { get; private set; }

        private List<int> _connectedControllers = new List<int>();
        /// <summary>
        /// global flips this if an error occured plugging in
        /// </summary>
        private ErrorState _errorOccured;

        private bool IsConnected
        {
            get
            {
                if (Client != null)
                    return true;

                return false;
            }
        }

        public void PlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (!_connectedControllers.Contains(index))
            {
                _connectedControllers.Add(index);
            }
        }

        public void UnPlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (_connectedControllers.Contains(index))
            {
                _connectedControllers.Remove(index);
            }
        }

        public override Action Start()
        {
            try
            {
                Client = new ViGEmClient();
            }
            catch (Exception x)
            {
                //TODO Log exception
                _errorOccured = ErrorState.OPEN_FAILED;
            }

            return base.Start();
        }

        public override void Stop()
        {
            _globals.ForEach(d =>
            {
                d.Dispose();
                UnPlugController(d.index);
            });

            _connectedControllers.Clear();
        }

        public override object CreateGlobal()
        {
            _globals = new List<XboxGlobal>();
            return new GlobalIndexer<XboxGlobal>(CreateGlobal);
        }

        private XboxGlobal CreateGlobal(int index)
        {
            var global = new XboxGlobal(index, this);
            _globals.Add(global);

            return global;
        }

        public override void DoBeforeNextExecute()
        {
            _globals.ForEach(d => d.Update());
        }

        public override string FriendlyName { get { return "ViGEm Virtual Bus"; } }

    }

    [Global(Name = "xbox")]
    public class XboxGlobal : IDisposable
    {
        public readonly int index = -1;

        private ViGemPlugin _plugin;

        private Xbox360Controller _controller;
        private Xbox360Report _report = new Xbox360Report();

        //public x360ButtonCollection button { get; }

        public XboxGlobal(int index, ViGemPlugin plugin)
        {
            _plugin = plugin;
            _controller = new Xbox360Controller(plugin.Client);
            _controller.FeedbackReceived += _controller_FeedbackReceived;
            _controller.Connect();

            this.index = index;

            _plugin.PlugController(index);

        }

        public event RumbleEvent onRumble;
        public delegate void RumbleEvent(byte largeMotor, byte smallMotor, byte led);

        private void _controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            onRumble?.Invoke(e.LargeMotor, e.SmallMotor, e.LedNumber);            
        }

        internal void Update()
        {
            if (_report != null)
            {
                _controller.SendReport(_report);
                //_prevReport = _report;
            }
            _report = new Xbox360Report();

        }
        
        private Xbox360Buttons[] enumToArray(Xbox360Buttons buttons)
        {
            var a = Enum.GetValues(typeof(Xbox360Buttons)).Cast<Xbox360Buttons>();
            var pressed = a.Where(b => (b & buttons) == b).ToArray();
            return pressed;
        }
        private Xbox360Buttons buttons
        {
            get { return (Xbox360Buttons)_report?.Buttons; }           
        }

        public bool a
        {
            get { return buttons.HasFlag(Xbox360Buttons.A); }
            set { _report.SetButtonState(Xbox360Buttons.A, value);         }
        }

        public bool b
        {
            get { return buttons.HasFlag(Xbox360Buttons.B); }
            set {
                _report.SetButtonState(Xbox360Buttons.B, value);
            }
        }

        public bool x
        {
            get { return  buttons.HasFlag(Xbox360Buttons.X);  }
            set {
                _report.SetButtonState(Xbox360Buttons.X, value);
            }
        }

        public bool y
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Y); }
            set {
                _report.SetButtonState(Xbox360Buttons.Y,value);
            }
        }

        public bool leftShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.LeftShoulder); }
            set {
                _report.SetButtonState(Xbox360Buttons.LeftShoulder,value);
            }
        }

        public bool rightShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.RightShoulder); }
            set {
                _report.SetButtonState(Xbox360Buttons.RightShoulder, value);
            }
        }

        public bool start
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Start); }
            set {
                _report.SetButtonState(Xbox360Buttons.Start, value);
            }
        }

        public bool back
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Back); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Back, value);
            }
        }

        public bool up
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Up); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Up, value);
            }
        }

        public bool down
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Down); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Down, value);
            }
        }

        public bool left
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Left);  }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Left, value);
            }
        }

        public bool right
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Right); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Right, value);
            }
        }

        public bool leftThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.LeftThumb); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.LeftThumb, value);
            }
        }

        public bool rightThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.RightThumb); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.RightThumb, value);
            }
        }

        /// <summary>
        /// Acceptable values range 0 - 1
        /// </summary>
        public double leftTrigger
        {
            get { return  _report.LeftTrigger/255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (short)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(Xbox360Axes.LeftTrigger,v);
                }
            }
        }

        
        public double rightTrigger
        {
            get { return _report.RightTrigger/255.0; }
            set
            {
                if(isBetween(value,0,1))
                {
                    var v = (short)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(Xbox360Axes.RightTrigger, v);
                }                
            }
        }

        public double leftStickX
        {
            get {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbX / 32768.0;
                else
                    return _report.LeftThumbX / 32767.0; 
            }
            set
            {
                _report.SetAxis(Xbox360Axes.LeftThumbX , (short)ensureMapRange(value, -1, 1, -32768, 32767)); 
            }
        }

        public double leftStickY
        {
            get
            {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbY / 32768.0;
                else
                    return _report.LeftThumbY / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.LeftThumbY, (short)ensureMapRange(value, -1, 1, -32768, 32767));
            }
        }

        public double rightStickX
        {
            get
            {
                if (_report.RightThumbX < 0)
                    return _report.RightThumbX / 32768.0;
                else
                    return _report.RightThumbX / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.RightThumbX, (short)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        public double rightStickY
        {
            get
            {
                if (_report.RightThumbY < 0)
                    return _report.RightThumbY / 32768.0;
                else
                    return _report.RightThumbY / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.RightThumbY, (short)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        private bool isBetween(double val, double min, double max, bool isInclusive = true)
        {
            if (isInclusive)
            {
                return (val >= min) && (val <= max);
            }
            else
            {
                return (val > min) && (val < max);
            }
        }

        private double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        private double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin) / (xMax - xMin)) * (yMax - yMin) + yMin, yMax), yMin);
        }

        public void Disconnect()
        {
            if (_controller != null)
            {
                _controller.Disconnect();
            }
        }
        public void Dispose()
        {
            if(_controller != null)
            {
                Disconnect();
                _controller.Dispose();
                _controller = null;
            }
        }
    }


    [Global(Name = "dualshock")]
    public class DS4Global : IDisposable
    {
        public readonly int index = -1;

        private ViGemPlugin _plugin;

        private DualShock4Controller _controller;
        private DualShock4Report _report = new DualShock4Report();

        //public x360ButtonCollection button { get; }

        public DS4Global(int index, ViGemPlugin plugin)
        {
            _plugin = plugin;
            _controller = new DualShock4Controller(plugin.Client);
            _controller.FeedbackReceived += _controller_FeedbackReceived;
            _controller.Connect();

            this.index = index;

            _plugin.PlugController(index);

        }

        public event RumbleEvent onRumble;
        public delegate void RumbleEvent(byte largeMotor, byte smallMotor, LightbarColor led);

        private void _controller_FeedbackReceived(object sender, DualShock4FeedbackReceivedEventArgs e)
        {
            onRumble?.Invoke(e.LargeMotor, e.SmallMotor, e.LightbarColor);
        }

        internal void Update()
        {
            if (_report != null)
            {
                DualShock4DPadValues d = conv(_DpadFlags);
                _report.SetDPad(d);
                _controller.SendReport(_report);
                //_prevReport = _report;
            }
            _report = new DualShock4Report();

        }

        private DualShock4Buttons[] enumToArray(DualShock4Buttons buttons)
        {
            var a = Enum.GetValues(typeof(DualShock4Buttons)).Cast<DualShock4Buttons>();
            var pressed = a.Where(b => (b & buttons) == b).ToArray();
            return pressed;
        }
        private DualShock4Buttons buttons
        {
            get { return (DualShock4Buttons)_report?.Buttons; }
        }

        public bool a
        {
            get { return buttons.HasFlag(DualShock4Buttons.Cross); }
            set { _report.SetButtonState(DualShock4Buttons.Cross, value); }
        }

        public bool b
        {
            get { return buttons.HasFlag(DualShock4Buttons.Circle); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.Circle, value);
            }
        }

        public bool x
        {
            get { return buttons.HasFlag(DualShock4Buttons.Square); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.Square, value);
            }
        }

        public bool y
        {
            get { return buttons.HasFlag(DualShock4Buttons.Triangle); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.Triangle, value);
            }
        }

        public bool leftShoulder
        {
            get { return buttons.HasFlag(DualShock4Buttons.ShoulderLeft); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.ShoulderLeft, value);
            }
        }

        public bool rightShoulder
        {
            get { return buttons.HasFlag(DualShock4Buttons.ShoulderRight); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.ShoulderRight, value);
            }
        }

        public bool options
        {
            get { return buttons.HasFlag(DualShock4Buttons.Options); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.Options, value);
            }
        }

        public bool share
        {
            get { return buttons.HasFlag(DualShock4Buttons.Share); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.Share, value);
            }
        }
        

        public bool up
        {
            get { return _DpadFlags.HasFlag(dpadFlags.Up); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Up;
                else
                    _DpadFlags &= ~dpadFlags.Up;
                
            }
        }

        public bool down
        {
            get { return buttons.HasFlag(DualShock4DPadValues.South); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Down;
                else
                    _DpadFlags &= ~dpadFlags.Down;
            }
        }

        public bool left
        {
            get { return buttons.HasFlag(DualShock4DPadValues.West); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Left;
                else
                    _DpadFlags &= ~dpadFlags.Left;
            }
        }

        public bool right
        {
            get { return buttons.HasFlag(DualShock4DPadValues.East); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Right;
                else
                    _DpadFlags &= ~dpadFlags.Right;
            }
        }

        public bool leftThumb
        {
            get { return buttons.HasFlag(DualShock4Buttons.ThumbLeft); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.ThumbLeft, value);
            }
        }

        public bool rightThumb
        {
            get { return buttons.HasFlag(DualShock4Buttons.ThumbRight); }
            set
            {
                _report.SetButtonState(DualShock4Buttons.ThumbRight, value);
            }
        }

        /// <summary>
        /// Acceptable values range 0 - 1
        /// </summary>
        public double leftTrigger
        {
            get { return _report.LeftTrigger / 255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (byte)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(DualShock4Axes.LeftTrigger, v);
                }
            }
        }


        public double rightTrigger
        {
            get { return _report.RightTrigger / 255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (byte)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(DualShock4Axes.RightTrigger, v);
                }
            }
        }

        public double leftStickX
        {
            get
            {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbX / 32768.0;
                else
                    return _report.LeftThumbX / 32767.0;
            }
            set
            {
                _report.SetAxis(DualShock4Axes.LeftThumbX,(byte) ensureMapRange(value, -1, 1, -256, 257)); //(short)ensureMapRange(value, -1, 1, -32768, 32767));
            }
        }

        public double leftStickY
        {
            get
            {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbY / 32768.0;
                else
                    return _report.LeftThumbY / 32767.0;
            }
            set
            {
                //_report.SetAxis(DualShock4Axes.LeftThumbY, (short)ensureMapRange(value, -1, 1, -32768, 32767));
                _report.SetAxis(DualShock4Axes.RightThumbY, (byte)ensureMapRange(value, -1, 1, -256, 257));
            }
        }

        public double rightStickX
        {
            get
            {
                if (_report.RightThumbX < 0)
                    return _report.RightThumbX / 32768.0;
                else
                    return _report.RightThumbX / 32767.0;
            }
            set
            {
                _report.SetAxis(DualShock4Axes.RightThumbX, (byte)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        public double rightStickY
        {
            get
            {
                if (_report.RightThumbY < 0)
                    return _report.RightThumbY / 32768.0;
                else
                    return _report.RightThumbY / 32767.0;
            }
            set
            {
                _report.SetAxis(DualShock4Axes.RightThumbY, (byte)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        private bool isBetween(double val, double min, double max, bool isInclusive = true)
        {
            if (isInclusive)
            {
                return (val >= min) && (val <= max);
            }
            else
            {
                return (val > min) && (val < max);
            }
        }

        private double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        private double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin) / (xMax - xMin)) * (yMax - yMin) + yMin, yMax), yMin);
        }

        [Flags]
        private enum dpadFlags
        {
            Up = 1 << 0, Right = 1 << 1, Down = 1 << 2, Left = 1 << 3
        }

        dpadFlags _DpadFlags = 0;

        private static DualShock4DPadValues conv(dpadFlags flags)
        {

            DualShock4DPadValues retval = 0;

            var g = new List<KeyValuePair<dpadFlags, DualShock4DPadValues>>()
            {
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up,DualShock4DPadValues.North),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up | dpadFlags.Right,DualShock4DPadValues.Northeast ),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Right, DualShock4DPadValues.East ),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Right | dpadFlags.Down,DualShock4DPadValues.Southeast),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Down, DualShock4DPadValues.South),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Down | dpadFlags.Left, DualShock4DPadValues.Southwest),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Left, DualShock4DPadValues.West),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up | dpadFlags.Left, DualShock4DPadValues.Northwest)
            };
            //Console.WriteLine(g.Aggregate((a,c) => string.Format("{0},{1}",a,c)));
            if (flags != 0)
                retval = g.Single(gg => gg.Key == flags).Value;


            return retval;

        }

        public void Disconnect()
        {
            if (_controller != null)
            {
                _controller.Disconnect();
            }
        }
        public void Dispose()
        {
            if (_controller != null)
            {
                Disconnect();
                _controller.Dispose();
                _controller = null;
            }
        }
    }
}
