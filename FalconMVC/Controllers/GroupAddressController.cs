﻿using FalconMVC.Managers;
using FalconMVC.Models;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        private readonly DbFalcon _dbFalcon;
        private readonly IInterfaceConnect _connection;

        private readonly Regex _regex =
            new Regex(@"^([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]{1})\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}$");
        public GroupAddressController(DbFalcon dbFalcon, IInterfaceConnect connection)
        {
            _dbFalcon = dbFalcon;
            _connection = connection;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddArchiveAsync()
        {
            return View(await _dbFalcon.GAs.AsNoTracking().ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddArchiveAsync(string nameGA, string typeGA)
        {
            if(nameGA is not null && !_dbFalcon.GAs.Any(ga => ga.GAddress == nameGA))
            {
                if (_regex.IsMatch(nameGA))
                {
                    var result = await _dbFalcon.GAs.AddAsync(new GA { GAddress = nameGA, GType = typeGA });
                    if (result.State == EntityState.Added)
                    {
                        await _dbFalcon.SaveChangesAsync();
                        return View(await _dbFalcon.GAs.AsNoTracking().ToListAsync());
                    }
                }
                ViewBag.Error = "GA doesn't correspond the 3-level pattern - __/__/__.";
                return View("Error");
            };
            return Content("GA is also exist or equals to null.");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var GAToRemove = await _dbFalcon.GAs.FindAsync(id);
            if(GAToRemove is not null)
            {
                var result = _dbFalcon.GAs.Remove(GAToRemove);
                if (result.State == EntityState.Deleted)
                {
                    await _dbFalcon.SaveChangesAsync();
                    return RedirectToAction("AddArchive", "GroupAddress");
                }
                else
                {
                    ViewBag.Error = "DB error.";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.Error = "GA is NULL. Reason : GA didn't find id DB or wrong ID.";
                return View("Error");
            }
        }

        public IActionResult StartMonitor()
        {
            _connection.bus.Connect();
            _connection.bus.ReadValue("1/1/1");
        }

        public IActionResult StopMonitor()
        {

        }

        public void ArchivGA(GroupAddress address)
        {

        }
    }
}
