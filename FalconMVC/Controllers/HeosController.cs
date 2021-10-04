using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class HeosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
