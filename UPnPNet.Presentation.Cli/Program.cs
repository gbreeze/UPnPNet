using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UPnPNet.Discovery;
using UPnPNet.Discovery.SearchTargets;
using UPnPNet.Models;
using UPnPNet.Services.AvTransport;
using UPnPNet.Services.SwitchPower;

namespace UPnPNet.Presentation.Cli
{
    public static class CollectionExtensions
    {
        public static void Foreach<T>(this ICollection<T> collection, Action<T> command)
        {
            foreach (T var in collection)
            {
                command(var);
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            UPnPDiscovery discovery = new UPnPDiscovery
            {
                //SearchTarget = DiscoverySearchTargetFactory.ServiceTypeSearch("AVTransport", "1")
                SearchTarget = DiscoverySearchTargetFactory.AllSearch(),
                WaitTimeInSeconds = 2
            };
            IList<UPnPDevice> devices = discovery.Search().Result;

            Console.WriteLine("Devices Found: " + devices.Count);

            devices.Foreach((i) => PrintDevice(i));


            var NetworkLight = devices.SelectMany(s => s.Services).FirstOrDefault(s => s.Type == "urn:schemas-upnp-org:service:SwitchPower:1");
            if (NetworkLight != null)
            {
                // do something
                var control = new SwitchPowerServiceControl(NetworkLight);
                Console.WriteLine("Type 'A' to set on the lights. 'Q' to exit.");

                while (true)
                {
                    ConsoleKeyInfo info = Console.ReadKey();

                    switch (info.Key)
                    {
                        case ConsoleKey.Q:
                            return;
                        case ConsoleKey.A:
                            control.SendAction("SetTarget", new Dictionary<string, string>() { { "newTargetValue", "True" } }).Wait();
                            break;
                        case ConsoleKey.S:

                            break;
                    }
                }
            }

            //UPnPServer server = new UPnPServer();

            //IList<UPnPDevice> sonosDevices =
            //	devices.Where(x => x.Properties["friendlyName"].ToLower().Contains("sonos")).ToList();

            //IList<UPnPService> avServices = sonosDevices
            //	.SelectMany(x => x.SubDevices)
            //	.SelectMany(x => x.Services)
            //	.Where(x => x.Type == "urn:schemas-upnp-org:service:AVTransport:1").ToList();

            //IList<AvTransportServiceControl> speakers = avServices.Select(x => new AvTransportServiceControl(x)).ToList();


            //server.Start(24458);

            //speakers.Foreach(x =>
            //{
            //	server.SubscribeToControl(x);
            //	x.OnLastChangeEvent += (sender, args) => { Console.WriteLine("SOMETHING: " + args.TransportState); };
            //});


            //while (true)
            //{
            //	ConsoleKeyInfo info = Console.ReadKey();

            //	switch (info.Key)
            //	{
            //		case ConsoleKey.Q:
            //			return;
            //		case ConsoleKey.A:
            //			speakers.Foreach(
            //				x => x.SendAction("Play", new Dictionary<string, string>() {{"InstanceID", "0"}, {"Speed", "1"}}).Wait());
            //			break;
            //		case ConsoleKey.S:
            //			speakers.Foreach(x => x.SendAction("Pause", new Dictionary<string, string>() {{"InstanceID", "0"}}).Wait());
            //			break;
            //	}
            //}
            Console.ReadLine();
        }

        public static void PrintDevice(UPnPDevice device, int indentation = 0)
        {
            string identation = new string('\t', indentation);

            Console.WriteLine(identation + "==" + device.Properties["friendlyName"] + "==");

            Console.WriteLine(identation + "Properties:");
            foreach (KeyValuePair<string, string> keyValuePair in device.Properties)
            {
                Console.WriteLine(identation + "\t - " + keyValuePair.Key + ": " + keyValuePair.Value);
            }

            Console.WriteLine(identation + "Services:");
            foreach (UPnPService service in device.Services)
            {
                Console.WriteLine(identation + "\t - " + service.Type);
            }

            Console.WriteLine(identation + "Actions:");
            foreach (string target in device.Targets)
            {
                Console.WriteLine(identation + "\t - " + target);
            }

            Console.WriteLine(identation + "SubDevices:");
            foreach (UPnPDevice subDevice in device.SubDevices)
            {
                PrintDevice(subDevice, indentation + 1);
            }
        }
    }
}