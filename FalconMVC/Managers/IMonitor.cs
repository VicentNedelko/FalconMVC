using Knx.Falcon.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IMonitor
    {
        public string GetInterfaceInfo();
        public void Start();
        public void Stop();
    }
}
