using Knx.Bus.Common.Configuration;
using Knx.Bus.Common.KnxIp;
using Knx.Falcon.Sdk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class InterfaceController : Controller
    {
        [HttpGet]
        public IActionResult ShowAll()
        {
            return View(GetInterfacesList());
        }
        [HttpPost]
        public IActionResult ShowAll(string interfaceIP)
        {
            using (Bus bus = new(new KnxIpTunnelingConnectorParameters(interfaceIP, 0x0e57, false)))
            {
                bus.Connect();
                if(bus.CheckCommunication() == Knx.Bus.Common.CheckCommunicationResult.Ok)
                {
                    var connection = bus.GetLocalConfiguration();
                    return Content($"Communication established - {connection.Address}");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        private DiscoveryResult[] GetInterfacesList()
        {
            DiscoveryClient discoveryClient = new DiscoveryClient(adapterType: AdapterTypes.All);
            DiscoveryResult[] results = discoveryClient.Discover();
            return results;
        }
    }
}
