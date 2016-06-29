﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UPnPNet.Discovery
{
    // ReSharper disable once InconsistentNaming
    public class UPnPDiscovery
    {
        public int LocalPort { get; set; } = 2500;
        public IPAddress LocalIpAddress { get; set; } = IPAddress.Any;
        public int Port { get; set; } = 1900;
        public int TimeToLive { get; set; } = 2;
        public string Host { get; set; } = "239.255.255.250";
        public int WaitTimeInSeconds { get; set; } = 1;
        public Encoding Encoder { get; set; } = new ASCIIEncoding();
        public IDiscoverySearchTarget SearchTarget { get; set; } = new DiscoverySearchTargetAll();

        private IPEndPoint LocalEndPoint => new IPEndPoint(LocalIpAddress, LocalPort);
        private IPEndPoint MulticastEndPoint => new IPEndPoint(IPAddress.Parse(Host), Port);

        public async Task<IList<UPnPDevice>> SearchAsync()
        {
            IList<UPnPDevice> output = new List<UPnPDevice>();

            //Prepare receive socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            socket.Bind(LocalEndPoint);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastEndPoint.Address, LocalIpAddress));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, TimeToLive);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            string request = $"M-SEARCH * HTTP/1.1\r\nHOST: {Host}\r\nMAN:\"ssdp:discover\"\r\nST: {SearchTarget.Target}\r\nMX: {WaitTimeInSeconds}\r\n\r\n";

            socket.SendTo(Encoder.GetBytes(request), SocketFlags.None, MulticastEndPoint);

            byte[] buffer = new byte[10000];

            Stopwatch watch = Stopwatch.StartNew();

            Task task = Task.Factory.StartNew(() =>
            {
                while (watch.ElapsedMilliseconds < WaitTimeInSeconds * 2000)
                {
                    if (socket.Available <= 0)
                        continue;

                    int received = socket.Receive(buffer);

                    IDictionary<string, string> response = ParseResponse(Encoder.GetString(buffer, 0, received));

                    if (!response.ContainsKey("LOCATION"))
                        return;

                    UPnPDevice device = output.FirstOrDefault(x => x.Location == response["LOCATION"]);
                    Console.WriteLine(response["LOCATION"]);

                    if (device == null)
                    {
                        device = new UPnPDevice()
                        {
                            Location = response["LOCATION"],
                            Server = response["SERVER"],
                            UniqueServiceName = response["USN"]
                        };


                        output.Add(device);
                    }

                    device.Targets.Add(response["ST"]);

                    System.Threading.Thread.Sleep(10);
                }
            });

            await task;
            socket.Close();

            return output;   
        }

        private IDictionary<string, string> ParseResponse(string input)
        {
            string[] lines = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> output = new Dictionary<string, string>();

            if (lines[0] != "HTTP/1.1 200 OK")
                return null;

            foreach (string line in lines.Where(x => x != lines.First()))
            {
                int colonIndex = line.IndexOf(":", StringComparison.Ordinal);

                if(colonIndex < 0)
                    continue;

                string key = line.Substring(0, colonIndex);
                string value = line.Substring(key.Length + 1).Trim();

                if (value.Length <= 0)
                    continue;
                
                output.Add(key, value);
            }

            return output;
        }
    }
}