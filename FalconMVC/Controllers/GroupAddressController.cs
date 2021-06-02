using FalconMVC.Models;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        public IList<GA> _groupAddressList;
        public GroupAddressController()
        {
            _groupAddressList = new List<GA>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddArchive()
        {
            List<GA> tempList;
            try
            {
                tempList = JsonConvert.DeserializeObject<List<GA>>((string)TempData["listGA"]);
            }
            catch(ArgumentNullException)
            {
                tempList = new List<GA>();
            }
            ViewBag.ListGA = tempList;
            return View();
        }

        [HttpPost]
        public IActionResult AddArchive(string addGA, string typeGA)
        {
            List<GA> tempList;
            try
            {
                tempList = JsonConvert.DeserializeObject<List<GA>>((string)TempData["listGA"]);
            }
            catch(ArgumentNullException)
            {
                tempList = new List<GA>();
            }
            
            if (addGA is not null && !tempList.Any(ga => ga.GAddress == addGA))
            {
                tempList.Add(
                    new GA
                {
                    GAddress = addGA,
                    GType = typeGA,
                });
                TempData["listGA"] = JsonConvert.SerializeObject(tempList);
                return RedirectToAction("AddArchive", "GroupAddress");
            }
            return Content("GA is null or also exist.");
        }

        public void ArchivGA(GroupAddress address)
        {

        }
    }
}
