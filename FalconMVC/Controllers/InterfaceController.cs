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
        public static string interfaceIP { get; set; } = "Undefined";
        [HttpGet]
        public IActionResult ShowAll()
        {
            return View(GetInterfacesList());
        }
        [HttpPost]
        public IActionResult ShowAll(string checkInterface)
        {
            interfaceIP = checkInterface; // transfer to HOME
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
