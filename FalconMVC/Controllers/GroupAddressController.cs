﻿using FalconMVC.Managers;
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
        //private readonly string pathRasp = @"/home/GAs/gaList.json";
        //private readonly string pathWin = @"C:\\GAs\\gaList.txt";
        private readonly string pathJSONWin = @"C:\\GAs\\gaList.json";

        private readonly Regex _regex =
            new(@"^([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]{1})\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}$");
        public GroupAddressController(DbFalcon dbFalcon, IMonitor monitor)
        {
            _dbFalcon = dbFalcon;
            _monitor = monitor;
        }

        // TODO: add JSON serialization


        public List<GA> GetGAFromFile()
        {
            List<GA> listGA = new();
            using(StreamReader streamReader = new(pathJSONWin))
            {
                JsonSerializer jsonSerializer = new();
                string jsonString = streamReader.ReadToEnd();
                listGA = JsonConvert.DeserializeObject<List<GA>>(jsonString);
                streamReader.Close();
            }
            
            return listGA;
        }

        public void WriteGAToFile(List<GA> listGA)
        {
            using StreamWriter streamWriter = new(pathJSONWin, false);
            JsonSerializer jsonSerializer = new();
            jsonSerializer.Serialize(streamWriter, listGA);
            streamWriter.Close();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddArchive()
        {
            return View(GetGAFromFile());
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
                        var listGA = GetGAFromFile();
                        listGA.Add(new GA { GAddress = nameGA, GType = BusMonitor.DPTConvert(typeGA) });
                        WriteGAToFile(listGA);
                        return View(GetGAFromFile());
                    }
                }
                ViewBag.Error = "GA doesn't correspond the 3-level pattern - __/__/__.";
                return View("Error");
            };
            return Content("GA is also exist or equals to null.");
        }

        public IActionResult Remove(string name)
        {
            var listGA = GetGAFromFile();

            var GAToRemove = listGA.FirstOrDefault(g => g.GAddress == name);
            if(GAToRemove is not null)
            {
                listGA.Remove(GAToRemove);
                WriteGAToFile(listGA);
                return RedirectToAction("AddArchive");
            }
            else
            {
                ViewBag.Error = "GA is NULL. Reason : GA didn't find in JSON or wrong name.";
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
