using System;
using System.Collections.Generic;
using System.Text;

namespace UPnPNet.Services.SwitchPower
{
    public class SwitchPowerEvent : UPnPLastChangeEvent
    {
        public SwitchPowerEvent(IList<UPnPLastChangeValue> values) : base(values)
        {

        }
    }
}
