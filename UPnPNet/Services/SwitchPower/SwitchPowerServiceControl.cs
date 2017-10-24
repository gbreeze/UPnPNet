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

        public async Task SetLightState(int instanceId, int volume)
        {
            if (volume < 0)
                throw new ArgumentOutOfRangeException(nameof(volume), "Must be greater or equal to zero");

            await SendAction("SetVolume", new Dictionary<string, string>
            {
                {"InstanceID", instanceId.ToString()},
                {"DesiredVolume", volume.ToString() }
            });
        }

        public async Task<int> GetLightState(int instanceId)
        {
            IDictionary<string, string> result = await SendAction("SetVolume", new Dictionary<string, string>
            {
                {"InstanceID", instanceId.ToString()}
            });

            return int.Parse(result["CurrentVolume"]);
        }
    }
}
