using DreamNucleus.Heos;
using DreamNucleus.Heos.Commands.Player;
using DreamNucleus.Heos.Infrastructure.Heos;
using DreamNucleus.Heos.Infrastructure.Telnet;
using FalconMVC.Globals;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public class Heos : IHeos
    {
        public List<IHeos.Item> Items { get; set; } = new();
        private readonly IWebHostEnvironment _env;

        public Heos(IWebHostEnvironment env)
        {
            _env = env;
        }
        
        public async Task AssignPlayersToIdsAsync()
        {
            var ips = Items.Select(i => i.Ip).ToArray();
            var telnetClient = new SimpleTelnetClient(ips);
            var heosClient = new HeosClient(telnetClient, CancellationToken.None);
            var commandProcessor = new CommandProcessor(heosClient);
            var playersResponse = await commandProcessor.Execute(new GetPlayersCommand(), r => r.Any(), 5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2));
            if (playersResponse.Success)
            {
                foreach(var i in Items)
                {
                    if (playersResponse.Payload.Select(p => p.Ip).Contains(i.Ip))
                    {
                        var item = playersResponse.Payload.First(p => p.Ip == i.Ip);
                        i.Pid = item.Pid;
                        i.Name = i.Name;
                    }
                }
            }
        }

        public List<string> ReadIpsFromFile()
        {
            List<string> listIps = new();
            using StreamReader sr = new(Path.Combine(_env.WebRootPath, Secret.HeosIps));
            var json = sr.ReadToEnd();
            if (json.StartsWith('['))
            {
                listIps = JsonSerializer.Deserialize<List<string>>(json);
            }
            sr.Close();
            return listIps;
        }

        public void WriteIpsToFile(List<string> ips)
        {
            using StreamWriter sw = new(Path.Combine(_env.WebRootPath, Secret.HeosIps));
            if(Items.Count > 0)
            {
                Items.Clear();
            }
            if(ips.Count > 0)
            {
                foreach (var ip in ips)
                {
                    Items.Add(
                        new IHeos.Item
                        {
                            Ip = ip,
                            Name = "Undefined",
                            Pid = 0,
                        }
                        );
                }
            }
            sw.WriteLine(JsonSerializer.Serialize(ips));
            sw.Flush();
            sw.Close();
        }
    }
}
