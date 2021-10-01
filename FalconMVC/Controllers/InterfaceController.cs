using Knx.Bus.Common.Configuration;
using Knx.Bus.Common.KnxIp;
using Knx.Falcon.Sdk;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FalconMVC.Managers;
using FalconMVC.ViewModels;

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
        public IActionResult Index()
        {
            InterfaceVM interfaceVM = new();
            if(_knxInterface.Ip is not null)
            {
                interfaceVM.Ip = _knxInterface.Ip;
                interfaceVM.FriendlyName = _knxInterface.InterfaceName;
                interfaceVM.State = _knxInterface.bus.State.ToString();
            }
            else
            {
                interfaceVM.Ip = "Undefined";
                interfaceVM.FriendlyName = "Undefined";
                interfaceVM.State = null;
            }
            ViewBag.InterfaceList = _knxInterface.Interfaces;
            return View(interfaceVM);
        }


        [HttpGet]
        public IActionResult ShowAll()
        {
            ViewBag.Interfaces = GetInterfacesList();
            ViewBag.InterfaceName = _knxInterface.InterfaceName;
            ViewBag.Ip = _knxInterface.Ip;
            return View();
        }
        [HttpPost]
        public IActionResult ShowAll (string interfaceIP)
        {
            if (_knxInterface.CheckConnection(interfaceIP))
            {
                _knxInterface.GetNewInterface(interfaceIP);
                return RedirectToAction("ShowAll", "Interface");
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
