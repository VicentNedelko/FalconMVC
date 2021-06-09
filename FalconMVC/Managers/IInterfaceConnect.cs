using Knx.Falcon.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IInterfaceConnect
    {
        public string InterfaceName { get; set; }
        public string Ip { get; set; }
        public Bus bus { get; set; }
        public bool CheckConnection(string interfaceIp);
    }
}
