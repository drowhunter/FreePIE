using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
//using int8_t = System.SByte;
//using byte = System.Byte;
//using int16_t = System.Int16;
//using uint16_t = System.UInt16;
//using uint = System.UInt32;


namespace FreePIE.Core.Plugins.Cronus
{

    public class GcdAPI : GcdAPIBase, IDisposable
    {

        public bool Connected { get { return IsConnected(); } }


        public GcdAPI(string path): base(path)
        {
          
            
        }
        
        /// <summary>
        /// Check if the GPP/CronusMax device is connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (gcapi_IsConnected == null)
                return false;

            bool retval = gcapi_IsConnected() > 0;
            return retval;
        }

        public void Connect(int timeout)
        {
            bool connected = false;
            if (gcapi_IsConnected == null)
                return;

            while (timeout > 0 && !connected)
            {
                connected = IsConnected();

                
                if (!connected)
                {
                    //wait a second and try again
                    System.Threading.Thread.Sleep(1000);
                }
                
                timeout--;
            }

            if (timeout == 0)
                throw new Exception("time out while trying to connect to cronusmax");
        }

        

        

        /// <summary>
        /// Poll the cronus for reports
        /// </summary>
        public void Poll()
        {
            while (true)
            {
                GCAPI_REPORT report = new GCAPI_REPORT();
               
            }
        }

        
        /// <summary>
        /// Get GPP/CronusMax firmware version
        /// </summary>
        /// <returns></returns>
        public UInt16 GetFWVer()
        {
            UInt16 retval = gcapi_GetFWVer();
            return retval;
        }

        /// <summary>
        /// Read GCDAPI_REPORT (states of GPP/CronusMax and controller)
        /// </summary>
        /// <returns></returns>
        public bool Read(ref GCAPI_REPORT report)
        {
            IntPtr retval = gcapi_Read(ref report);

            return retval != IntPtr.Zero;
        }
        /// <summary>
        /// Write output[GCDAPI_OUTPUT_TOTAL] (send it to console)
        /// </summary>
        /// <returns>after writing report if rumble is required next read will populate the rumble array</returns>
        public void Write(Input[] inputs)
        {
            var bytes = inputs.Select(input => input.value).ToArray();

            byte retval = gcapi_Write(bytes);

        }
        /// <summary>
        /// Get current time value
        /// </summary>
        /// <returns></returns>
        public uint GetTimeVal()
        {
            uint retval = gcapi_GetTimeVal();
            return retval;
        }

        /// <summary>
        /// Calculates the pressing time of a entry.
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public uint CalcPressTime(uint btn)
        {
            uint retval = gcapi_CalcPressTime(btn);
            return retval;
        }

        
    }
}
