﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class WiimoteButtonState : Subscribable
    {
        private IWiimoteData data;

        public WiimoteButtonState(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public bool button_down(WiimoteButtons b)
        {
            return data.IsButtonPressed(b);
        }
    }
}
