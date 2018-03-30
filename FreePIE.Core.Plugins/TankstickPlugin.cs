using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using JonesCorp;



namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(TankStickGlobal), IsIndexed = true)]
    public class TankStickPlugin : Plugin, IDisposable
    {
        private ErrorCode _lastError;
        private List<TankStickGlobal> _globals;

        private TankStickWrapper _driver;
        private CancellationTokenSource _cancelToken;

        internal enum ErrorCode
        {
            OK = 0,
            OPEN_FAILED,
            START_FAILED
        }

        
        //public bool XarcadeIsRunning { get { return _driver.IsRunning; } }
        
        public override object CreateGlobal()
        {
            _globals = new List<TankStickGlobal>();

            return new  GlobalIndexer<TankStickGlobal>(CreateGlobal);
        }

        public TankStickGlobal CreateGlobal(int index)
        {
            var global = new TankStickGlobal(index);
            _globals.Add(global);

            return global;
        }

        public override string FriendlyName { get { return "X-Arcade Tankstick"; } }
        

        public override Action Start()
        {
            if (_driver == null)
            {
                _driver = new TankStickWrapper();
                _driver.PlayerData += Update;
            }

           

            if (_driver.Open())
            {
                _cancelToken = new CancellationTokenSource();

                if (!_driver.Start(_cancelToken))
                {
                    _lastError = ErrorCode.START_FAILED;
                }
            }
            else 
                _lastError = ErrorCode.OPEN_FAILED;
#if DEBUG
            Debug.Assert(_lastError == ErrorCode.OK, "Error Finding Tankstick");
#endif
            _lastError = ErrorCode.OPEN_FAILED;

            
            return base.Start();
        }

        private void Update(object sender, PlayerEventArgs e)
        {
            if (e.Index  < _globals.Count && e.Index >= 0)
                _globals[e.Index].Update((XArcadeButton) e.Buttons);
        }
        
        public override void Stop()
        {
            if (_driver.Stop())
                _driver.Close();

        }

        public void Dispose()
        {
            Console.WriteLine("dispose tankstick plugin");
        }
    }

    [Global(Name = "tankstick")]
    public class TankStickGlobal
    {
        internal readonly int index;
        
        private XArcadeButton _buttons;

        public TankStickGlobal(int index)
        {
            this.index = index;
        }
        internal void Update(XArcadeButton buttons)
        {
            _buttons = buttons;
        }


        public bool up { get { return _buttons.HasFlag(XArcadeButton.Up); } }
        public bool down { get { return _buttons.HasFlag(XArcadeButton.Down); } }
        public bool left { get { return _buttons.HasFlag(XArcadeButton.Left); } }
        public bool right { get { return _buttons.HasFlag(XArcadeButton.Right); } }


        public bool b1 { get { return _buttons.HasFlag(XArcadeButton.Button1); } }
        public bool b2 { get { return _buttons.HasFlag(XArcadeButton.Button2); } }
        public bool b3 { get { return _buttons.HasFlag(XArcadeButton.Button3); } }

        public bool b4 { get { return _buttons.HasFlag(XArcadeButton.Button4); } }
        public bool b5 { get { return _buttons.HasFlag(XArcadeButton.Button5); } }
        public bool b6 { get { return _buttons.HasFlag(XArcadeButton.Button6); } }

        public bool b7 { get { return _buttons.HasFlag(XArcadeButton.Button7); } }
        public bool b8 { get { return _buttons.HasFlag(XArcadeButton.Button8); } }

        
        public bool coin { get { return _buttons.HasFlag(XArcadeButton.Button9); } }
        public bool start { get { return _buttons.HasFlag(XArcadeButton.Button10); } }

        

        
    }
    
    /// <summary>
    /// ==============================================================================
    /// BUTTON LAYOUT
    /// -------------------------------------------------------------------------------
    ///								                    (START-10)
    ///			        (U-11)	    (X-1) (Y-2) (LT-3)          
    ///			    (L-13) (R-14)	(A-4) (B-5) (RT-6)
    ///	        	    (D-12)	    (LB-7)(RB-8)    
    /// (BACK-9)        
    /// </summary>     
    [GlobalEnum]
    public enum XArcadeButton : uint
    {

        NONE = 0,

        /// <summary>
        /// LCtrl / [ A
        /// </summary>
        Button1 = 1 << 0,

        /// <summary>
        /// LAlt / S
        /// </summary>
        Button2 = 1 << 1,

        /// <summary>
        /// Space / Q
        /// </summary>
        Button3 = 1 << 2,

        /// <summary>
        /// LShift / W
        /// </summary>
        Button4 = 1 << 3,

        /// <summary>
        /// Z / E
        /// </summary>
        Button5 = 1 << 4,

        /// <summary>
        /// X / [
        /// </summary>
        Button6 = 1 << 5,


        /// <summary>
        /// C / ]
        /// </summary>
        Button7 = 1 << 6,

        /// <summary>
        /// 5 / 6
        /// </summary>
        Button8 = 1 << 7,
        /// <summary>
        /// 3 / 4
        /// </summary>
        Button9 = 1 << 8,

        /// <summary>
        /// 1 / 2
        /// </summary>
        Button10 = 1 << 9,
        /// <summary>
        /// Up / R
        /// </summary>
        Up = 1 << 10,

        /// <summary>
        /// Down / F
        /// </summary>
        Down = 1 << 11,

        /// <summary>
        /// Left / D
        /// </summary>
        Left = 1 << 12,

        /// <summary>
        /// Right / G
        /// </summary>
        Right = 1 << 13,


    }
}
