using System;
using System.Runtime.InteropServices;

//using int8_t = System.SByte;
//using uint8_t = System.Byte;
//using int16_t = System.Int16;
//using uint16_t = System.UInt16;
//using uint32_t = System.UInt32;


namespace FreePIE.Core.Plugins.Cronus
{
    public class GcdAPIBase : ExternalLib,IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate byte GCDAPI_Load();


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate void GCDAPI_Unload();
        


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate byte GCAPI_IsConnected();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate ushort GCAPI_GetFWVer();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate IntPtr GCAPI_Read([In, Out] ref GCAPI_REPORT report);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate byte GCAPI_Write(sbyte[] output);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate uint GCAPI_GetTimeVal();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate uint GCAPI_CalcPressTime(uint btn);

        

        
        /// <summary>
        /// Mandatory! Call after load the gcdapi.dll library
        /// </summary>
        protected GCDAPI_Load gcdapi_Load;

        /// <summary>
        /// Mandatory! Call before unload the gcdapi.dll library
        /// </summary>
        protected GCDAPI_Unload gcdapi_Unload;

        /// <summary>
        /// Check if the GPP/CronusMax device is connected
        /// </summary>
        protected GCAPI_IsConnected gcapi_IsConnected;
        /// <summary>
        /// Get GPP/CronusMax firmware version
        /// </summary>
        protected GCAPI_GetFWVer gcapi_GetFWVer;

        /// <summary>
        /// Read GCDAPI_REPORT (states of GPP/CronusMax and controller)
        /// </summary>
        protected GCAPI_Read gcapi_Read;
        /// <summary>
        /// Write output[GCDAPI_OUTPUT_TOTAL] (send it to console)
        /// </summary>
        protected GCAPI_Write gcapi_Write;
        /// <summary>
        /// Get current time value
        /// </summary>
        protected GCAPI_GetTimeVal gcapi_GetTimeVal;
        /// <summary>
        /// Calculates the pressing time of a entry.
        /// </summary>
        protected GCAPI_CalcPressTime gcapi_CalcPressTime;
        
        

        public GcdAPIBase(string pathToDll):base(pathToDll)
        {
           
        }

        protected override void Init()
        {
            
            gcdapi_Load = GetProc<GCDAPI_Load>("gcdapi_Load");
            gcdapi_Unload = GetProc<GCDAPI_Unload>("gcdapi_Unload");

            gcapi_IsConnected = GetProc<GCAPI_IsConnected>("gcapi_IsConnected");
            gcapi_GetFWVer = GetProc<GCAPI_GetFWVer>("gcapi_GetFWVer");
            gcapi_Read = GetProc<GCAPI_Read>("gcapi_Read");
            gcapi_Write = GetProc<GCAPI_Write>("gcapi_Write");
            gcapi_GetTimeVal = GetProc<GCAPI_GetTimeVal>("gcapi_GetTimeVal");
            gcapi_CalcPressTime = GetProc<GCAPI_CalcPressTime>("gcapi_CalcPressTime");
            
            base.Init();
        }

        public override void Load()
        {
            if (this.gcdapi_Load() == 0)
                throw new Exception("gcdapi_Load failed");
            base.Load();
        }


        public override void Dispose()
        {
            if(gcdapi_Load != null)
                this.gcdapi_Unload();
            base.Dispose();
        }
        
    }
}
