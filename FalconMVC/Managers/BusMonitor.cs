using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public class BusMonitor : IMonitor
    {
        private readonly IInterfaceConnect _connection;
        public BusMonitor(IInterfaceConnect connection)
        {
            _connection = connection;
        }
        public void Start()
        {
            _connection.bus.Connect();
            while(_connection.bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
            {
                // create new Task() and start monitor
            }
        }

        public void Stop()
        {
            _connection.bus.Disconnect();
        }
    }
}
