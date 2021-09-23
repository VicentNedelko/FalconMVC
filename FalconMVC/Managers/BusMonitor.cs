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
using Knx.Bus.Common;

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
            _connection.bus.Connect();
            _connection.bus.GroupValueReceived += Bus_GroupValueReceived;
        }

        public void Stop()
        {
            _connection.bus.GroupValueReceived -= Bus_GroupValueReceived;
            _connection.bus.Disconnect();
        }

        public string GetInterfaceInfo()
        {
            if(_connection.bus.State == BusConnectionStatus.Connected)
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

        private void Bus_GroupValueReceived(GroupValueEventArgs obj)
        {
            using StreamWriter streamWriter = new(Path.Combine(_env.WebRootPath, "monitoring.txt"), true);
            streamWriter.WriteLine($"{obj.Address} - {obj.Value} - {DateTime.Now.ToShortTimeString()}");
            streamWriter.Flush();
            streamWriter.Close();
        }

    }
}
