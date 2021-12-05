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

        private List<GA> GAList { get; set; }

        private List<GAwithThreshold> GaThList { get; set; }

        public BusMonitor(IInterfaceConnect connection, IWebHostEnvironment env, IBot bot)
        {
            _connection = connection;
            _env = env;
            _bot = bot;
            GaThList = new List<GAwithThreshold>();
            GAList = new List<GA>();
            using StreamReader srTh = new(Path.Combine(_env.WebRootPath, Secret.GAThList));
            GaThList = JsonSerializer.Deserialize<List<GAwithThreshold>>(srTh.ReadToEnd());
            using StreamReader sr = new(Path.Combine(_env.WebRootPath, Secret.GAList));
            GAList = JsonSerializer.Deserialize<List<GA>>(sr.ReadToEnd());
        }

        public InterfaceVM GetInterfaceData()
        {
            InterfaceVM interfaceVM;
            if (_connection.Ip is not null)
            {
                interfaceVM = new InterfaceVM
                {
                    Ip = _connection.Ip,
                    FriendlyName = _connection.InterfaceName,
                };
                interfaceVM.State = "Test";
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
            using StreamWriter sw = new(Path.Combine(_env.WebRootPath, Secret.GAMonitor), false);
            sw.Write(string.Empty);
            sw.Close();
            _connection.bus = new(new KnxIpTunnelingConnectorParameters(_connection.Ip, 0x0e57, false));
            _connection.bus.Connect();
            _connection.bus.GroupValueReceived += Bus_GroupValueReceived;
            _bot.SendMessageAsync($"Bus state - {_connection.bus.State}; Subscribe to GA_Sbc.");
        }

        public void Stop()
        {
            _connection.bus.GroupValueReceived -= Bus_GroupValueReceived;
            _connection.bus.Dispose();
            _bot.SendMessageAsync($"Unsubscribe from GA_Sbc.");
        }

        public string GetInterfaceInfo()
        {
            if (_connection.bus.State == BusConnectionStatus.Connected)
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
                "Brightness" => DptType.Brightness,
                _ => DptType.Unknown,
            };
        }

        private void Bus_GroupValueReceived(GroupValueEventArgs obj)
        {
            if (GAList.Any(ga => ga.GAddress == obj.Address.ToString()))
            {
                var gaVerified = GAList.First(ga => ga.GAddress == obj.Address.ToString());
                var convertedValueFull = gaVerified.GType switch
                {
                    DptType.Brightness => string.Concat(new Dpt9().ToTypedValue(obj.Value).ToString(), " lux"),
                    DptType.Temperature => string.Concat(new Dpt9().ToTypedValue(obj.Value).ToString(), " °C"),
                    DptType.Percent => string.Concat(new Dpt5().ToTypedValue(obj.Value).ToString(), " %"),
                    DptType.Switch => new Dpt2().ToTypedValue(obj.Value).ToString(),
                    _ => obj.Value.ToString(),
                };
                using StreamWriter streamWriter = new(Path.Combine(_env.WebRootPath, Secret.GaArchive, DateTime.Now.ToShortDateString()), true);
                var jsonData = JsonSerializer.Serialize(obj);
                streamWriter.Write(jsonData);
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
            if (_connection.bus.State != BusConnectionStatus.Connected)
            {
                _connection.bus.Connect();
            }
            _connection.bus.GroupValueReceived += Bus_GroupValueReceivedNotify;
        }

        private void Bus_GroupValueReceivedNotify(GroupValueEventArgs obj)
        {
            // StartReceiving();  ?
            if (GaThList.Any(gaTh => gaTh.GAddress == obj.Address))
            {
                var processedValue = GaThList.First(gaTh => gaTh.GAddress == obj.Address);
                switch (processedValue.GType)
                {
                    case DptType.Temperature:
                        var valueToCheckTemp = new Dpt9().ToTypedValue(obj.Value);
                        if (((float)processedValue.ThresholdMax <= valueToCheckTemp
                            || (float)processedValue.ThresholdMin > valueToCheckTemp)
                            && !processedValue.IsCheck)
                        {
                            SendGAThValueTemperature(processedValue, valueToCheckTemp);
                            WriteWarningToFile($"Warning!  {processedValue.Description} - {valueToCheckTemp} °C");
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = true;
                        }
                        else if (((float)processedValue.ThresholdMax > valueToCheckTemp
                            && (float)processedValue.ThresholdMin < valueToCheckTemp)
                            && processedValue.IsCheck)
                        {
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = false;
                        }
                        break;
                    case DptType.Percent:
                        var valueToCheckPercent = new Dpt5().ToTypedValue(obj.Value);
                        if (((byte)processedValue.ThresholdMax <= valueToCheckPercent
                            || (byte)processedValue.ThresholdMin > valueToCheckPercent)
                            && !processedValue.IsCheck)
                        {
                            SendGAThValuePercent(processedValue, valueToCheckPercent);
                            WriteWarningToFile($"Warning! {processedValue.Description} - {valueToCheckPercent} %");
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = true;
                        }
                        else if (((byte)processedValue.ThresholdMax > valueToCheckPercent
                            && (byte)processedValue.ThresholdMin < valueToCheckPercent)
                            && processedValue.IsCheck)
                        {
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = false;
                        }
                        break;
                    case DptType.Switch:
                        var valueToCheckSwitch = new Dpt1().ToTypedValue(obj.Value);
                        if (valueToCheckSwitch && !processedValue.IsCheck)
                        {
                            SendGAThValueSwitch(processedValue, valueToCheckSwitch);
                            WriteWarningToFile($"Warning! {processedValue.Description} - {valueToCheckSwitch} state");
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = true;
                        }
                        else if (!valueToCheckSwitch && processedValue.IsCheck)
                        {
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = false;
                        }
                        break;
                    case DptType.Brightness:
                        var valueToCheckBrightness = new Dpt9().ToTypedValue(obj.Value);
                        if (((float)processedValue.ThresholdMax <= valueToCheckBrightness
                            || (float)processedValue.ThresholdMin > valueToCheckBrightness)
                            && !processedValue.IsCheck)
                        {
                            SendGAThValueBrightness(processedValue, valueToCheckBrightness);
                            WriteWarningToFile($"Warning! {processedValue.Description} - {valueToCheckBrightness} lux");
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = true;
                        }
                        else if (((float)processedValue.ThresholdMax > valueToCheckBrightness
                            && (float)processedValue.ThresholdMin < valueToCheckBrightness)
                            && processedValue.IsCheck)
                        {
                            var ind = GaThList.FindIndex(ga => ga.Id == processedValue.Id);
                            GaThList[ind].IsCheck = false;
                        }
                        break;
                };
            };
        }

        public void StopNotificator()
        {
            _connection.bus.GroupValueReceived -= Bus_GroupValueReceivedNotify;
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

        public void SendGAThValueBrightness(GAwithThreshold sample, float value)
        {
            _bot.SendMessageAsync($"Warning!  {sample.Description} - {value} lux");
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
