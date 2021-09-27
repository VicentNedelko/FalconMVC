using FalconMVC.Globals;
using FalconMVC.Managers;
using FalconMVC.Models;
using FalconMVC.ViewModels;
using Knx.Bus.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class GroupAddressController : Controller
    {
        private readonly DbFalcon _dbFalcon;
        private readonly IMonitor _monitor;
        private readonly IBot _tbot;
        private readonly IWebHostEnvironment _env;

        private readonly Regex _regex =
            new(@"^([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]{1})\/([0-9]|[1-9][0-9]|[1-2][0-5][0-5]){1}$");
        public GroupAddressController(DbFalcon dbFalcon, IMonitor monitor, IBot tbot, IWebHostEnvironment env)
        {
            _dbFalcon = dbFalcon;
            _monitor = monitor;
            _tbot = tbot;
            _env = env;
        }


        public List<GA> GetGAFromFile()
        {
            List<GA> listGA = new();
            string pathJSON = Path.Combine(_env.WebRootPath, "gaList.json");

            using(StreamReader streamReader = new(pathJSON))
            {
                string jsonString = streamReader.ReadToEnd();
                if(jsonString.Substring(0, 1) == "[")
                {
                    listGA = JsonSerializer.Deserialize<List<GA>>(jsonString);
                    streamReader.Close();
                }
            }
            return listGA;
        }

        public List<GAwithThreshold> GetGAWithThFromFile()
        {
            List<GAwithThreshold> gaWithThresholds = new();
            var JSONwithThPath = Path.Combine(_env.WebRootPath, "gaThList.json");

            using(StreamReader sr = new(JSONwithThPath))
            {
                var str = sr.ReadToEnd();
                if(str.Substring(0, 1) == "[")
                {
                    gaWithThresholds = JsonSerializer.Deserialize<List<GAwithThreshold>>(str);
                    sr.Close();
                }
            };
            return gaWithThresholds;
        }

        public void WriteGAWithThToFile(List<GAwithThreshold> gaList)
        {
            var JSONThPath = Path.Combine(_env.WebRootPath, "gaThList.json");
            var json = JsonSerializer.Serialize(gaList);
            using StreamWriter sw = new(JSONThPath, false);
            sw.Write(json);
            sw.Close();
        }

        public List<GAwithThreshold> ConvertToGAwithThreshold(List<GA> listGA)
        {
            List<GAwithThreshold> listGAwithThresholds = new();
            listGAwithThresholds = listGA.Select(ga => new GAwithThreshold
            {
                Id = ga.Id,
                GAddress = ga.GAddress,
                GType = ga.GType,
                Description = ga.Description,
                ThresholdMin = 0,
                ThresholdMax = 0,
                IsCheck = false,
            }).ToList();

            return listGAwithThresholds;
        }

        public void WriteGAToFile(List<GA> listGA)
        {
            string pathJSON = Path.Combine(_env.WebRootPath, "gaList.json");
            using StreamWriter streamWriter = new(pathJSON, false);
            var json = JsonSerializer.Serialize(listGA);
            streamWriter.Write(json);
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
            ViewBag.InterfaceData = _monitor.GetInterfaceData();
            var listModel = GetGAWithThFromFile();
            return View(listModel);
        }

        public IActionResult AddThresholdGA(string nameGA, string descriptionGA, string typeGA, decimal maxValue, decimal minValue)
        {
            if (_regex.IsMatch(nameGA))
            {
                var gaWithThreshold = new GAwithThreshold
                {
                    Id = Guid.NewGuid(),
                    GAddress = nameGA,
                    GType = BusMonitor.DPTConvert(typeGA),
                    Description = descriptionGA,
                    ThresholdMin = minValue,
                    ThresholdMax = maxValue,
                    IsCheck = false,
                };
                var list = GetGAWithThFromFile();
                list.Add(gaWithThreshold);
                WriteGAWithThToFile(list);
            }
            else
            {
                ViewBag.Error = "GA doesn't correspond the 3-level pattern - __/__/__.";
                return View("Error");
            }
            return RedirectToAction("Thresholds");

        }

        public IActionResult RemoveGAWithTh(string address)
        {
            var gaThList = GetGAWithThFromFile();
            var itemToRemove = gaThList.Single(ga => ga.GAddress == address);
            if(itemToRemove is not null)
            {
                gaThList.Remove(itemToRemove);
                WriteGAWithThToFile(gaThList);
                return RedirectToAction("Thresholds");
            }
            else
            {
                ViewBag.Error = "Error! GA didn't find in JSON list.";
                return View("Error");
            }
        }

        public void StartNotificator()
        {
            _monitor.StartNotificator();
        }

        public void StopNotificator()
        {
            _monitor.StopNotificator();
        }


        [HttpGet]
        public IActionResult AddArchive()
        {
            ViewBag.InterfaceData = _monitor.GetInterfaceData();
            return View(GetGAFromFile());
        }

        [HttpPost]
        public IActionResult AddArchive(string nameGA, string typeGA, string descriptionGA)
        {
            if(nameGA is not null)
            {
                if (_regex.IsMatch(nameGA))
                {
                    var listGA = GetGAFromFile();
                    listGA.Add(new GA { Id = Guid.NewGuid(), GAddress = nameGA, GType = BusMonitor.DPTConvert(typeGA), Description = descriptionGA });
                    WriteGAToFile(listGA);
                    //await _tbot.SendMessageAsync($"new GA {nameGA} added");
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
