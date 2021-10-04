using DreamNucleus.Heos;
using DreamNucleus.Heos.Commands.Player;
using DreamNucleus.Heos.Infrastructure.Heos;
using DreamNucleus.Heos.Infrastructure.Telnet;
using FalconMVC.Globals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public class Heos : IHeos
    {
        public List<IHeos.Item> Items { get ; set ; }
        
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
    }
}
