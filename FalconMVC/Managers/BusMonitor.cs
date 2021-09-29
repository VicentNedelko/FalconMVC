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
using FalconMVC.Globals;
using FalconMVC.ViewModels;

namespace FalconMVC.Managers
{
    public class BusMonitor : IMonitor
    {
        private readonly IInterfaceConnect _connection;

        private readonly IWebHostEnvironment _env;
        private readonly IBot _bot;

        private List<GA> GAList
        {
            get
            {
                using StreamReader sr = new(Path.Combine(_env.WebRootPath, Secret.GAList));
                return JsonSerializer.Deserialize<List<GA>>(sr.ReadToEnd());
            }
        }

        private List<GAwithThreshold> GaThList
        {
            get
            {
                using StreamReader sr = new(Path.Combine(_env.WebRootPath, Secret.GAThList));
                return JsonSerializer.Deserialize<List<GAwithThreshold>>(sr.ReadToEnd());
            }
        }

        public BusMonitor(IInterfaceConnect connection, IWebHostEnvironment env, IBot bot)
        {
            _connection = connection;
            _env = env;
            _bot = bot;
        }

        public InterfaceVM GetInterfaceData()
        {
            InterfaceVM interfaceVM;
            if(_connection.Ip is not null)
            {
                interfaceVM = new InterfaceVM
                {
                    Ip = _connection.Ip,
                    FriendlyName = _connection.InterfaceName,
                    State = _connection.bus.State.ToString(),
                };
            }
            else
            {
                interfaceVM = new InterfaceVM
                {
                    Ip = "Not connected",
                    FriendlyName = "Not connected",
                    State = "Not connected",
                };
            }
            return interfaceVM;
        }

        public void Start()
        {
            // clear monitoring file
            using (StreamWriter sw = new(Path.Combine(_env.WebRootPath, Secret.GAMonitor), false))
            {
                sw.Write(string.Empty);
                sw.Close();
            }
            if(_connection.bus.State != BusConnectionStatus.Connected)
            {
                _connection.bus.Connect();
            }
            _connection.bus.GroupValueReceived += Bus_GroupValueReceived;
        }

        public void Stop()
        {
            _connection.bus.GroupValueReceived -= Bus_GroupValueReceived;
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
            if(GAList.Any(ga => ga.GAddress == obj.Address.ToString()))
            {
                var gaVerified = GAList.First(ga => ga.GAddress == obj.Address.ToString());
                var convertedValueFull = gaVerified.GType switch
                {
                    DptType.Temperature => string.Concat(new Dpt9().ToTypedValue(obj.Value).ToString(), " °C"),
                    DptType.Percent => string.Concat(new Dpt5().ToTypedValue(obj.Value).ToString(), " %"),
                    DptType.Switch => new Dpt2().ToTypedValue(obj.Value).ToString(),
                    _ => obj.Value.ToString(),
                };
                using StreamWriter streamWriter = new(Path.Combine(_env.WebRootPath, Secret.GAMonitor), true);
                streamWriter.WriteLine($"{obj.Address} - {convertedValueFull} - {DateTime.Now}");
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        public void StartNotificator()
        {
            // clear monitoring notificator file
            using StreamWriter sw = new(Path.Combine(_env.WebRootPath, Secret.GAWithThMonitor));
            sw.Write(string.Empty);
            sw.Close();
            if(_connection.bus.State != BusConnectionStatus.Connected)
            {
                _connection.bus.Connect();
            }
            _connection.bus.GroupValueReceived += Bus_GroupValueReceivedNotify;
        }

        private void Bus_GroupValueReceivedNotify(GroupValueEventArgs obj)
        {
            // StartReceiving();  ?
            if(GaThList.Any(gaTh => gaTh.GAddress == obj.Address))
            {
                var processedValue = GaThList.First(gaTh => gaTh.GAddress == obj.Address);
                switch (processedValue.GType)
                {
                    case DptType.Temperature:
                        var valueToCheckTemp = new Dpt9().ToTypedValue(obj.Value);
                        if(((float)processedValue.ThresholdMax <= valueToCheckTemp
                            || (float)processedValue.ThresholdMin > valueToCheckTemp)
                            && !processedValue.IsCheck)
                        {
                            SendGAThValueTemperature(processedValue, valueToCheckTemp);
                            WriteWarningToFile($"Warning!  {processedValue.Description} - {valueToCheckTemp} °C");
                            processedValue.IsCheck = true;
                        }
                        else if(((float)processedValue.ThresholdMax > valueToCheckTemp
                            && (float)processedValue.ThresholdMin < valueToCheckTemp)
                            && processedValue.IsCheck)
                        {
                            processedValue.IsCheck = false;
                        }
                        break;
                    case DptType.Percent:
                        var valueToCheckPercent = new Dpt5().ToTypedValue(obj.Value);
                        SendGAThValuePercent(processedValue, valueToCheckPercent);
                        break;
                    case DptType.Switch:
                        var valueToCheckSwitch = new Dpt1().ToTypedValue(obj.Value);
                        SendGAThValueSwitch(processedValue, valueToCheckSwitch);
                        break;
                };


            };

        }

        public void StopNotificator()
        {
            throw new NotImplementedException();
        }

        public void SendGAThValueTemperature(GAwithThreshold sample, float value)
        {
            _bot.SendMessageAsync($"Warning!  {sample.Description} - {value} °C");
        }

        public void SendGAThValuePercent(GAwithThreshold sample, byte value)
        {
            _bot.SendMessageAsync($"Warning! {sample.Description} - {value} %");
        }

        public void SendGAThValueSwitch(GAwithThreshold sample, bool value)
        {
            _bot.SendMessageAsync($"Warning! {sample.Description} - {value} state.");
        }

        private void WriteWarningToFile(string message)
        {
            using StreamWriter sw = new(Path.Combine(_env.WebRootPath, Secret.GAWithThMonitor), true);
            sw.WriteLine(message);
            sw.Flush();
            sw.Close();
        }

    }
}
