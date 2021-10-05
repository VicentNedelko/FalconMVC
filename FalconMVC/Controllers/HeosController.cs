using FalconMVC.Managers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class HeosController : Controller
    {
        private readonly IHeos _heos;

        public HeosController(IHeos heos)
        {
            _heos = heos;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View(_heos.ReadIpsFromFile());
        }

        [HttpPost]
        public IActionResult Settings(string ip, string description)
        {
            var ips = _heos.ReadIpsFromFile();
            ips.Add(ip);
            _heos.WriteIpsToFile(ips);
            return View(_heos.ReadIpsFromFile());
        }

        [HttpGet]
        public IActionResult RemoveIpFromList(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                var ips = _heos.ReadIpsFromFile();
                var i = ips.FindIndex(adr => adr == ip);
                ips.RemoveAt(i);
                _heos.WriteIpsToFile(ips);
            }
            return RedirectToAction("Settings", "Heos");
        }

        [HttpGet]
        public IActionResult FindDevices()
        {
            
            return View();
        }
    }
}
