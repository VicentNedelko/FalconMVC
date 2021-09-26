using FalconMVC.ViewModels;
using Knx.Falcon.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IMonitor
    {
        public InterfaceVM GetInterfaceData();
        public void Start();
        public void Stop();
        public void StartNotificator();
        public void StopNotificator();
    }
}
