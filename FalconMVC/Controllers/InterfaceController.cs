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

        [HttpPost]
        public IActionResult Index(string ip)
        {
            _knxInterface.GetNewInterface(ip);
            return View(new InterfaceVM { Ip = _knxInterface.Ip, FriendlyName = _knxInterface.InterfaceName, State = _knxInterface.bus.State.ToString()});
        }
    }
}
