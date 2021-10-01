using Knx.Bus.Common.Configuration;
using Knx.Bus.Common.KnxIp;
using Knx.Falcon.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public class KnxIpInterface : IInterfaceConnect
    {
        public Bus bus { get; set; }
        public string InterfaceName { set; get; }
        public string Ip { set; get; }
        public DiscoveryResult[] Interfaces { get; set; }

        public KnxIpInterface()
        {
            DiscoveryClient discoveryClient = new(adapterType: AdapterTypes.All);
            Interfaces = discoveryClient.Discover();
        }

        public void GetNewInterface(string interfaceIp)
        {
            if(bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
            {
                bus.Disconnect();
                bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x057, false));
            }
            bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x057, false));
            InterfaceName = bus.OpenParameters.Name;
            Ip = "Unknown";
        }


        public bool CheckConnection(string interfaceIp)
        {
            bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x0e57, false));
            using (bus)
            {
                bus.Connect();
                if (bus.CheckCommunication() == Knx.Bus.Common.CheckCommunicationResult.Ok)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
