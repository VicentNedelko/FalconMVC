using Knx.Bus.Common.Configuration;
using Knx.Bus.Common.KnxIp;
using Knx.Falcon.Sdk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FalconMVC.Managers;

namespace FalconMVC.Controllers
{
    public class InterfaceController : Controller
    {
        private readonly IInterfaceConnect _knxInterface;
        public InterfaceController(IInterfaceConnect knxInterface)
        {
            _knxInterface = knxInterface;
        }

        [HttpGet]
        public IActionResult ShowAll()
        {
            ViewBag.Interfaces = GetInterfacesList();
            return View();
        }
        [HttpPost]
        public IActionResult ShowAll (string interfaceIP)
        {
            if (_knxInterface.CheckConnection(interfaceIP))
            {
                return Content($"Communication established - {interfaceIP}");
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
