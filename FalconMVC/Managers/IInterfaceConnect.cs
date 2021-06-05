using Knx.Falcon.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IInterfaceConnect
    {
        public Bus bus { get; set; }
        public bool CheckConnection(string interfaceIP);
    }
}
