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
        private readonly Bus _bus;
        public KnxIpInterface()
        {
            int i = 0;
            DiscoveryClient discoveryClient = new DiscoveryClient(adapterType: AdapterTypes.All);
            DiscoveryResult[] results = discoveryClient.Discover();
            while(i < results.Count() - 1)
            {
                if(results[i].MediumType == Knx.Bus.Common.MediumTypes.Tp)
                {
                    var ip = results[i].IpAddress;
                    _bus = new Bus(new KnxIpTunnelingConnectorParameters(ip.ToString(), 0x0e57, false));
                    break;
                }
                i++;
            }
        }
        public bool CheckConnection(string interfaceIP)
        {
            using (_bus)
            {
                _bus.Connect();
                if (_bus.CheckCommunication() == Knx.Bus.Common.CheckCommunicationResult.Ok)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
