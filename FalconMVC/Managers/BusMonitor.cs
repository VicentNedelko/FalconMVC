using FalconMVC.Enums;
using FalconMVC.Models;
using Knx.Bus.Common.DatapointTypes;
using Knx.Bus.Common.GroupValues;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Knx.Falcon.Sdk;

namespace FalconMVC.Managers
{
    public class BusMonitor : IMonitor
    {
        public IInterfaceConnect _connection { set; get; }

        private readonly DbFalcon _dbFalcon;

        private readonly string path = @"C:\Users\user\Documents\GitHub\FalconMVC\Monitoring\monitor.txt";

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
            Task monitoringTask = new(() => Monitoring(gaList));
            monitoringTask.Start();
        }

        public void Stop()
        {
            _connection.bus.Disconnect();
        }

        public string GetInterfaceInfo()
        {
            if(_connection.bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
            {
                return _connection.bus.GetLocalConfiguration().ToString();
            }
            _connection.bus.Connect();
            return _connection.bus.GetLocalConfiguration().ToString();
        }

        public static DptType DPTConvert(string dptType)
        {
            return dptType switch
            {
                "Switch" => DptType.Switch,
                "Temperature" => DptType.Temperature,
                "Percent" => DptType.Percent,
                _ => DptType.Unknown,
            };
        }

        private void Monitoring(List<GA> gaMonitoringList)
        {
            using StreamWriter streamWriter = new(path, false, System.Text.Encoding.Default);
            while (_connection.bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
            {
                float convertedValue;
                GroupValue rawValue;
                foreach (var gaValue in gaMonitoringList)
                {
                    try
                    {
                        rawValue = _connection.bus.ReadValue(gaValue.GAddress);
                    }
                    catch (Knx.Bus.Common.Exceptions.NoResponseReceivedException)
                    {
                        rawValue = new(false);
                    }

                    switch (gaValue.GType)
                    {
                        case (DptType.Switch or DptType.Unknown):
                            streamWriter.WriteLine($"{gaValue.GAddress} - {rawValue} - {DateTime.Now.ToShortTimeString()}");
                            break;
                        case DptType.Temperature:
                            convertedValue = new Dpt9().ToTypedValue(rawValue);
                            streamWriter.WriteLine($"{gaValue.GAddress} - {convertedValue} °C - {DateTime.Now.ToShortTimeString()}");
                            break;
                        case DptType.Percent:
                            convertedValue = new Dpt5().ToTypedValue(rawValue);
                            streamWriter.WriteLine($"{gaValue.GAddress} - {convertedValue} % - {DateTime.Now.ToShortTimeString()}");
                            break;
                    }
                }
                Thread.Sleep(10000);
            }
            streamWriter.Close();
        }
    }
}
