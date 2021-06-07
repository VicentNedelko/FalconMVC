using FalconMVC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public class BusMonitor : IMonitor
    {
        private readonly IInterfaceConnect _connection;
        private readonly DbFalcon _dbFalcon;

        private string path = @"C:\Users\user\Documents\GitHub\FalconMVC\Monitoring\monitor.txt";
        public BusMonitor(IInterfaceConnect connection, DbFalcon dbFalcon)
        {
            _connection = connection;
            _dbFalcon = dbFalcon;
        }
        public void Start()
        {
            var gaList = _dbFalcon.GAs.AsNoTracking().ToList();
            var gaMonitoringList = new List<string>();
            foreach(var ga in gaList)
            {
                gaMonitoringList.Add(ga.GAddress);
            }
            _connection.bus.Connect();
            Task monitoringTask = new Task(() => Monitoring(gaMonitoringList));
            monitoringTask.Start();
        }

        public void Stop()
        {
            _connection.bus.Disconnect();
        }

        private void Monitoring(List<string> gaMonitoringList)
        {
            using (StreamWriter streamWriter = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                while (_connection.bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
                {
                    foreach(var gaValue in gaMonitoringList)
                    {
                        streamWriter.WriteLine(_connection.bus.ReadValue(gaValue).ToString());
                    }
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
