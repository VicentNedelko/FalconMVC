using FalconMVC.Managers;
using FalconMVC.Models;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        private readonly DbFalcon _dbFalcon;
        private readonly IMonitor _monitor;
        private readonly string pathRasp = @"/home/GAs/gaList.txt";
        private readonly string pathWin = @"C:\\GAs\\gaList.txt";

        private readonly Regex _regex =
            new(@"^([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]{1})\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}$");
        public GroupAddressController(DbFalcon dbFalcon, IMonitor monitor)
        {
            _dbFalcon = dbFalcon;
            _monitor = monitor;

        }

        public List<GA> GetGAFromFile()
        {
            List<GA> listGA = new();
            using(StreamReader streamReader = new(pathWin))
            {
                string line;
                while((line = streamReader.ReadLine()) is not null)
                {
                    var ga = line.Split(" > ");
                    try
                    {
                        listGA.Add(new GA { GAddress = ga[0], GType = BusMonitor.DPTConvert(ga[1]) });
                    }
                    catch
                    {
                        listGA.Add(new GA { GAddress = "0/0/0", GType = Enums.DptType.Unknown });
                    }
                }
                streamReader.Close();
            }
            
            return listGA;
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
                    var result = await _dbFalcon.GAs.AddAsync(new GA { GAddress = nameGA, GType = BusMonitor.DPTConvert(typeGA) });
                    if (result.State == EntityState.Added)
                    {
                        await _dbFalcon.SaveChangesAsync();
                        if (!Directory.Exists(@"C:\\GAs"))
                        {
                            Directory.CreateDirectory(@"C:\\GAs");
                        }
                        using StreamWriter streamWriter = new(pathWin, true);
                        await streamWriter.WriteLineAsync((new GA { GAddress = nameGA, GType = BusMonitor.DPTConvert(typeGA) }).ToString());
                        streamWriter.Close();
                        return View(GetGAFromFile());
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
                ViewBag.Error = "GA is NULL. Reason : GA didn't find ID in DB or wrong ID.";
                return View("Error");
            }
        }

        public IActionResult StartMonitor()
        {
            _monitor.Start();
            return RedirectToAction("AddArchive");
        }

        public IActionResult StopMonitor()
        {
            _monitor.Stop();
            return RedirectToAction("AddArchive");
        }
    }
}
