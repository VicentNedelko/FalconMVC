using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class RegistrationController : Controller
    {
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
    }
}
