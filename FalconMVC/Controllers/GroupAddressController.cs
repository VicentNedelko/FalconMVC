using FalconMVC.Models;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        private readonly DbFalcon _dbFalcon;
        public GroupAddressController(DbFalcon dbFalcon)
        {
            _dbFalcon = dbFalcon;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddArchive()
        {
            return View(_dbFalcon.GAs.ToArray());
        }
        [HttpPost]
        public async Task<IActionResult> AddArchiveAsync(string addGA)
        {
            if (addGA is not null && !_dbFalcon.GAs.Any(g => g.GAddress == addGA))
            {
                var result = _dbFalcon.GAs.AddAsync(
                    new GA
                {
                    GAddress = addGA,
                });
                if (result.IsCompletedSuccessfully)
                {
                    await _dbFalcon.SaveChangesAsync();
                    var listGA = await _dbFalcon.GAs.ToArrayAsync();
                    return RedirectToAction("AddArchive", "GroupAddress", listGA);
                }
                return Content("Error! Entity NOT added.");
            }
            return Content("GA is null or also exist.");
        }

        public void ArchivGA(GroupAddress address)
        {

        }
    }
}
