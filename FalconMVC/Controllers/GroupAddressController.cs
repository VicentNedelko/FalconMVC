using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddArchive()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddArchive(string addGA)
        {
            if (addGA is not null)
            {
                //_ = new GroupAddress();
                GroupAddress addressToArchive;
                if (GroupAddress.TryParse(addGA, out addressToArchive))
                {
                    ArchivGA(addressToArchive);
                }

            }
            return Content("Empty");
        }

        public void ArchivGA(GroupAddress address)
        {

        }
    }
}
