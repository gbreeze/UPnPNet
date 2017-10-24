using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPnPNet.Models;

namespace UPnPNet.Services.SwitchPower
{
    public class SwitchPowerServiceControl : UPnPLastChangeServiceControl<SwitchPowerEvent>
    {
        public SwitchPowerServiceControl(UPnPService service) : base(service, x => new SwitchPowerEvent(x))
        {
            if (service.Id != "urn:upnp-org:serviceId:SwitchPower.0001")
            {
                throw new ArgumentException("Service does not have correct id, is " + service.Id + ", should be urn:upnp-org:serviceId:SwitchPower.0001");
            }
        }

        public async Task SetLightState(bool state)
        {
            await SendAction("SetTarget", new Dictionary<string, object>
            {
                {"newTargetValue", state.ToString()}
            });
        }

        public async Task<bool> GetLightState()
        {
            IDictionary<string, string> result = await SendAction("GetStatus");

            return Convert.ToBoolean(int.Parse(result["ResultStatus"]));
        }
    }
}
