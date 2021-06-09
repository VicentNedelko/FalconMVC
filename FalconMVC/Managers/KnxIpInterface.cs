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
        public Bus bus { get; set; } // use Discovery method as setter
        public string InterfaceName { set; get; }
        public string Ip { set; get; }

        public KnxIpInterface()
        {
            int i = 0;
            DiscoveryClient discoveryClient = new DiscoveryClient(adapterType: AdapterTypes.All);
            DiscoveryResult[] results = discoveryClient.Discover();
            while(i < results.Count() - 1)
            {
                if(results[i].MediumType == Knx.Bus.Common.MediumTypes.Tp && results[i].FriendlyName == "Gira X1")
                {
                    var ip = results[i].IpAddress;
                    Ip = ip.ToString();
                    InterfaceName = results[i].FriendlyName;
                    bus = new Bus(new KnxIpTunnelingConnectorParameters(Ip, 0x0e57, false));
                    break;
                }
                i++;
            }
        }

        public void GetNewInterface(string interfaceIp)
        {
            if(bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
            {
                bus.Disconnect();
                bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x057, false));
            }
            bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x057, false));
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
