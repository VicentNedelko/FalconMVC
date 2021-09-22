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
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using Knx.Bus.Common.Configuration;

namespace FalconMVC.Managers
{
    public class BusMonitor : IMonitor
    {
        private IInterfaceConnect _connection { set; get; }

        private readonly IWebHostEnvironment _env;


        public BusMonitor(IInterfaceConnect connection, IWebHostEnvironment env)
        {

            _connection = connection;
            _env = env;
        }
        public void Start()
        {
            List<GA> gaList;
            var jsonPath = Path.Combine(_env.WebRootPath, "gaList.json");
            using (StreamReader sr = new(jsonPath))
            {
                var json = sr.ReadToEnd();
                gaList = JsonSerializer.Deserialize<List<GA>>(json);
                sr.Close();
            }
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
                _connection.bus.Connect();
                while (_connection.bus.State == Knx.Bus.Common.BusConnectionStatus.Connected)
                {
                    using StreamWriter streamWriter = new(Path.Combine(_env.WebRootPath, "monitoring.txt"), true);
                    float convertedValue;
                    GroupValue rawValue;
                    foreach (var gaValue in gaMonitoringList)
                    {
                        try
                        {
                            rawValue = _connection.bus.ReadValue(gaValue.GAddress);
                        }
                        catch (Knx.Bus.Common.Exceptions.ConnectionException)
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
                    streamWriter.Flush();
                    streamWriter.Close();
                    Thread.Sleep(1000);
                }

        }
    }
}
