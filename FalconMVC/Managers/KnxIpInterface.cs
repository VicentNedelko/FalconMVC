using Knx.Bus.Common;
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
            IAsyncResult asyncResult = discoveryClient.BeginDiscover();
            Interfaces = discoveryClient.EndDiscover(asyncResult);
        }

        public void GetNewInterface(string interfaceIp)
        {
            if (bus is not null && bus.State == BusConnectionStatus.Connected)
            {
                bus.Disconnect();
                bus.Dispose();
            }

            bus = new(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x0e57, false));
            InterfaceName = (Interfaces.FirstOrDefault(i => i.IpAddress.ToString() == interfaceIp)).FriendlyName;
            Ip = interfaceIp;
        }


        public bool CheckConnection(string interfaceIp)
        {
            bus = new Bus(new KnxIpTunnelingConnectorParameters(interfaceIp, 0x0e57, false));
            using (bus)
            {
                if (bus.State != BusConnectionStatus.Connected)
                {
                    try
                    {
                        bus.Connect();
                        if (bus.CheckCommunication() == CheckCommunicationResult.Ok)
                        {
                            bus.Disconnect();
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
