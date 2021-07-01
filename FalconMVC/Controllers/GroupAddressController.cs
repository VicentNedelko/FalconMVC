using FalconMVC.Managers;
using FalconMVC.Models;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
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
        private readonly IBot _tbot;
        //private readonly string pathRasp = @"/home/GAs/gaList.json";
        //private readonly string pathWin = @"C:\\GAs\\gaList.txt";
        private readonly string pathJSONWin = @"C:\\GAs\\gaList.json";

        private readonly Regex _regex =
            new(@"^([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]{1})\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}$");
        public GroupAddressController(DbFalcon dbFalcon, IMonitor monitor, IBot tbot)
        {
            _dbFalcon = dbFalcon;
            _monitor = monitor;
            _tbot = tbot;
        }

        public List<GA> GetGAFromFile()
        {
            List<GA> listGA = new();
            
            using(StreamReader streamReader = new(pathJSONWin))
            {
                JsonSerializer jsonSerializer = new();
                string jsonString = streamReader.ReadToEnd();
                var listGAJson = JsonConvert.DeserializeObject<List<GA>>(jsonString);
                streamReader.Close();
                if(listGAJson is not null)
                {
                    return listGAJson;
                }
            }
            return listGA;
        }

        public List<GAwithThreshold> ConvertToGAwithThreshold(List<GA> listGA)
        {
            List<GAwithThreshold> listGAwithThresholds = new();
            foreach(var ga in listGA)
            {
                listGAwithThresholds.Add(
                    new GAwithThreshold
                    {
                        Id = ga.Id,
                        GAddress = ga.GAddress,
                        GType = ga.GType,
                        ThresholdMin = 0,
                        ThresholdMax = 0,
                        IsCheck = false
                    });
            }
            return listGAwithThresholds;
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

        // Thresholds

        [HttpGet]
        public IActionResult Thresholds()
        {
            return View(ConvertToGAwithThreshold(GetGAFromFile()));
        }


        // Thresholds

        [HttpGet]
        public IActionResult AddArchive()
        {
            return View(GetGAFromFile());
        }

        [HttpPost]
        public async Task<IActionResult> AddArchiveAsync(string nameGA, string typeGA)
        {
            if(nameGA is not null)
            {
                if (_regex.IsMatch(nameGA))
                {
                    //if (!Directory.Exists(@"C:\\GAs"))
                    //{
                    //    Directory.CreateDirectory(@"C:\\GAs");
                    //}
                    var listGA = GetGAFromFile();
                    listGA.Add(new GA { Id = Guid.NewGuid(), GAddress = nameGA, GType = BusMonitor.DPTConvert(typeGA) });
                    WriteGAToFile(listGA);
                    await _tbot.SendMessageAsync($"new GA {nameGA} added");
                    return View(GetGAFromFile());
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
