using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.Cronus
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        public sbyte value;            // Current value - Range: [-100 ~ 100] %
        public sbyte prev_value;       // Previous value - Range: [-100 ~ 100] %
        public uint press_tv;       // Time marker for the button press event

        public int delta { get { return value - prev_value; } }

        public override string ToString()
        {
            return string.Format("value: {0} prev: {1} time:{2}", value, prev_value, press_tv);
        }

        public bool wasReleased
        {
            get
            {
                return (this.prev_value != 0 && (this.value != this.prev_value));
            }
        }

        public bool wasPressed
        {
            get
            {
                return (this.prev_value == 0 && (this.value != this.prev_value));
            }
        }

        public bool isHeld
        {
            get
            {
                return this.value != 0;
            }
        }

        
    }
}
