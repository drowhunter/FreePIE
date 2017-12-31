using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Cronus;
using FreePIE.Core.Plugins.Globals;

namespace FreePIE.Core.Plugins
{
    internal struct Constants
    {
        public const int GCAPI_INPUT_TOTAL = 30;
        public const int GCAPI_OUTPUT_TOTAL = 36;
    }


    [GlobalType(Type = typeof(CronusGlobal))]
    public class CronusPlugin: Plugin, IDisposable
    {
        
        private CronusGlobal _global;
        public override object CreateGlobal()
        {
            _global = new CronusGlobal(this);
            return _global;
        }

        public override string FriendlyName { get { return "cronus"; } }

        private GcdAPI _wrapper;

        private Stopwatch syncTimer;

        int pollInterval = Convert.ToInt32(1000 / 60);

        private GCAPI_REPORT _report;

        public override Action Start()
        {
            _wrapper = new GcdAPI("gcdapi.dll");
            _wrapper.Load();
            syncTimer = new Stopwatch();

            if (!_wrapper.IsConnected())
            {
                _wrapper.Connect(5);
            }


            if (_wrapper.IsConnected() && ReadReport())
            {
                syncTimer.Start();
            }

            return null;
        }

        
        public override void DoBeforeNextExecute()
        {
            if (!syncTimer.IsRunning || syncTimer.ElapsedMilliseconds < pollInterval)
                return;

            syncTimer.Stop();

            //retrieve user input
            Input[] inputs = _global.Outgoing();

            if ((OutputPortState) _report.console != OutputPortState.CONSOLE_DISCONNECTED)
            {
                _wrapper.Write(inputs);
            }



            if (ReadReport()) //read physical controller -  if not plugged in then this should be false
            {
                //global.update(report);
                _global.Incoming(_report);
            }
            
            syncTimer.Restart();
        }

        /// <summary>
        /// Read a report from the device
        /// </summary>
        /// <returns></returns>
        private bool ReadReport()
        {
            _report = new GCAPI_REPORT();

            var report = new GCAPI_REPORT();
            bool success = _wrapper.Read(ref report);

            if(success)
                _report = report;
            return success;

        }

        public override void Stop()
        {
            syncTimer.Reset();
            _wrapper.Dispose();
        }


        public uint GetFirmwareVersion()
        {
            return _wrapper.GetFWVer();
        }

        

        public void Dispose()
        {
            ((IDisposable) _wrapper).Dispose();
        }
    }

    [Global(Name = "cronus")]
    public class CronusGlobal
    {
        private readonly CronusPlugin _plugin;
        public PS3SubController ps3 { get; private set; }
        //public SubController<PS3> ps3 { get; private set; }
        public SubController<PS4> ps4 { get; private set; }

        public SubController<XB360> x360 { get; private set; }
        public SubController<XB1> xBone { get; private set; }
        public SubController<WII> wiimote { get; private set; } 
        
        public SubController<WIICLASSIC> wiiclassic { get; private set; } 


        public CronusGlobal(CronusPlugin plugin)
        {
            _plugin = plugin;
            
            //ps3 = new SubController<PS3>(this);
            ps3 = new PS3SubController(this);
            ps4 = new SubController<PS4>(this);
            x360 = new SubController<XB360>(this);
            xBone = new SubController<XB1>(this);
            wiimote = new SubController<WII>(this);


        }

        Input[] _input = new Input[Constants.GCAPI_INPUT_TOTAL];
        internal Input[] input
        {
            get { return _input; }
        }
        

        public OutputPortState Console { get; private set; }

        private InputPortState _controller;
        public InputPortState Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public byte[] Led { get; private set; }

        public byte[] Rumble { get; private set; }

        public int BatteryLevel { get; private set; }

        
        /// <summary>
        /// send input to the console
        /// </summary>
        /// <returns></returns>
        internal Input[] Outgoing()
        {
            return input;
        }

        /// <summary>
        /// receive data from input controller
        /// </summary>
        /// <param name="report"></param>
        internal void Incoming(GCAPI_REPORT report)
        {
            Console = (OutputPortState)report.console;
            Rumble = report.rumble;
            Led = report.led;
            BatteryLevel = report.battery_level * 10;

            if (Controller != (InputPortState)report.controller)
            {
                //if report has changed and it wasnt just disconnected update our input buffer
                if ((InputPortState)report.controller != InputPortState.CONTROLLER_DISCONNECTED)
                    _input = report.inputs;
                else //otherwise just this once reset the input
                    _input = new Input[Constants.GCAPI_INPUT_TOTAL];
            }


            Controller = (InputPortState)report.controller;
            
            

        }

        public uint GetFirmwareVersion()
        {
            return _plugin.GetFirmwareVersion();
        }

        ///// <summary>
        ///// input from authenticated controller
        ///// </summary>
        ///// <param name="args"></param>
        //public override void update(params object[] args)
        //{

        //    //throw new NotImplementedException();
        //    //var report = (GCAPI_REPORT)args[0];
        //    //Receive(report);
        // }





    }

    public class SubController<T> where T:struct
    {
        protected CronusGlobal g;

        public SubController(CronusGlobal g)
        {
            this.g = g;
        }

        protected SByte boolToSbyte(bool value)
        {
            return Convert.ToSByte(value ? 100 : 0);
        }

        protected bool SbyteToBool(SByte value)
        {
            return value > 0;
        }

        protected bool getBool(int index)
        {
            return g.input[index].value > 0;
        }

        protected void setBool(int index, bool value)
        {
            g.input[index].value = Convert.ToSByte(value ? 100 : 0);
        }

        protected double getDouble(int index)
        {
            return g.input[index].value / 100.0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">enumeration index</param>
        /// <param name="value">value from -1 to 1</param>
        protected void setDouble(int index, double value)
        {
            g.input[index].value = Convert.ToSByte(value * 100);
        }

        public bool IsDown(T button)
        {
            var retval = getBool((int) Enum.Parse(typeof (T), button.ToString()));
            //var retval = g.input[(int)Enum.Parse(typeof(T),button.ToString())].isHeld;
            return retval;
        }

        public void setDown(T button, bool val)
        {
            setBool((int)Enum.Parse(typeof(T),button.ToString()),val);
            //g.input[(int)Enum.Parse(typeof(T), button.ToString())].value = Convert.ToSByte(val ? 100 : 0);
        }

        public double getValue(T button)
        {
            var retval = getDouble((int) Enum.Parse(typeof (T), button.ToString()));
            //var retval = g.input[(int)Enum.Parse(typeof(T), button.ToString())].value / 100.0;
            return retval;
        }

        public void setValue(T button, double val)
        {
            if (Math.Abs(val) <= 1)
                //g.input[(int) Enum.Parse(typeof (T), button.ToString())].value = Convert.ToSByte(val *100.0);
                setDouble((int)Enum.Parse(typeof(T),button.ToString()), val);
        }
    }


    public abstract class SubController
    {
        protected CronusGlobal g;

        public SubController(CronusGlobal g)
        {
            this.g = g;
        }

        protected SByte boolToSbyte(bool value)
        {
            return Convert.ToSByte(value ? 100 : 0);
        }

        protected bool SbyteToBool(SByte value)
        {
            return value > 0;
        }

        protected bool getBool(int index)
        {
            return g.input[index].value > 0;
        }

        protected void setBool(int index, bool value)
        {
            g.input[index].value = Convert.ToSByte(value ? 100 : 0);
        }

        protected double getDouble(int index)
        {
            return g.input[index].value / 100.0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">enumeration index</param>
        /// <param name="value">value from -1 to 1</param>
        protected void setDouble(int index, double value)
        {
            g.input[index].value = Convert.ToSByte(value * 100);
        }
    }

    public class PS3SubController : SubController
    {

        public bool PS
        {
            get { return getBool((int)PS3.PS); }
            set { setBool((int)PS3.PS, value); }
        }

        public bool Select
        {
            get { return getBool((int)PS3.SELECT); }
            set { setBool((int)PS3.SELECT, value); }
        }
        public bool Start
        {
            get { return getBool((int)PS3.START); }
            set { setBool((int)PS3.START, value); }
        }


        public bool R1
        {
            get { return getBool((int)PS3.R1); }
            set { setBool((int)PS3.R1, value); }
        }
        public bool R3
        {
            get { return getBool((int)PS3.R3); }
            set { setBool((int)PS3.R3, value); }
        }
        public bool L1
        {
            get { return getBool((int)PS3.L1); }
            set { setBool((int)PS3.L1, value); }
        }
        public bool L3
        {
            get { return getBool((int)PS3.L3); }
            set { setBool((int)PS3.L3, value); }
        }
        public bool Triangle
        {
            get { return getBool((int)PS3.TRIANGLE); }
            set { setBool((int)PS3.TRIANGLE, value); }
        }
        public bool Circle
        {
            get { return getBool((int)PS3.CIRCLE); }
            set { setBool((int)PS3.CIRCLE, value); }
        }
        public bool Cross
        {
            get { return getBool((int)PS3.CROSS); }
            set { setBool((int)PS3.CROSS, value); }
        }
        public bool Square
        {
            get { return getBool((int)PS3.SQUARE); }
            set { setBool((int)PS3.SQUARE, value); }
        }
        public bool Up
        {
            get { return getBool((int)PS3.UP); }
            set { setBool((int)PS3.UP, value); }
        }
        public bool Down
        {
            get { return getBool((int)PS3.DOWN); }
            set { setBool((int)PS3.DOWN, value); }
        }
        public bool Left
        {
            get { return getBool((int)PS3.SQUARE); }
            set { setBool((int)PS3.SQUARE, value); }
        }
        public bool Right
        {
            get { return getBool((int)PS3.SQUARE); }
            set { setBool((int)PS3.SQUARE, value); }
        }


        public double LX
        {
            get { return getDouble((int)PS3.LX); }
            set { setDouble((int)PS3.LX, value); }
        }
        public double LY
        {
            get { return getDouble((int)PS3.LY); }
            set { setDouble((int)PS3.LY, value); }
        }
        public double RX
        {
            get { return getDouble((int)PS3.RX); }
            set { setDouble((int)PS3.RX, value); }
        }
        public double RY
        {
            get { return getDouble((int)PS3.RY); }
            set { setDouble((int)PS3.RY, value); }
        }



        public double L2
        {
            get { return getDouble((int)PS3.L2); }
            set { setDouble((int)PS3.L2, value); }
        }
        public double R2
        {
            get { return getDouble((int)PS3.R2); }
            set { setDouble((int)PS3.R2, value); }
        }

        public double Accx
        {
            get { return getDouble((int)PS3.ACCX); }
            set { setDouble((int)PS3.ACCX, value); }
        }
        public double Accy
        {
            get { return getDouble((int)PS3.ACCY); }
            set { setDouble((int)PS3.ACCY, value); }
        }
        public double AccZ
        {
            get { return getDouble((int)PS3.ACCZ); }
            set { setDouble((int)PS3.ACCZ, value); }
        }
        public double Gyro
        {
            get { return getDouble((int)PS3.GYRO); }
            set { setDouble((int)PS3.GYRO, value); }
        }

        public PS3SubController(CronusGlobal g) : base(g)
        {
            this.g = g;
        }


    }
}
