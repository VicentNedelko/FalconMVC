using FalconMVC.Models;
using FalconMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Controllers
{
    public class RegistrationController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogInAsync(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.Users.FirstOrDefault(u => u.Email == model.Email);
                if(user is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return Content("SignIn failed. Try again.");
                    }
                }
                else
                {
                    return Content("Error! User NOT found.");
                }
            }
            else
            {
                return Content("Error! Model is NOT valid. Email or Password required.");
            }
        }

        [HttpGet]
        public IActionResult LogOff()
        {
            if (User.Identity.IsAuthenticated)
            {
                _signInManager.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddUserAsync(NewUserViewModel model)
        {
            if (ModelState.IsValid && (await _userManager.FindByEmailAsync(model.Email) is null))
            {
                User user = new User
                {
                    UserName = model.UserName,
                    FName = model.FName,
                    LName = model.LName,
                    Email = model.Email,
                    
                };
                var addUserResult = await _userManager.CreateAsync(user, model.Password);
                if (addUserResult.Succeeded)
                {
                    return RedirectToAction("LogIn", "Registration");
                }
                else
                {
                    var errors = addUserResult.Errors.ToList();
                    string errorContent = string.Empty;
                    foreach(var e in errors)
                    {
                        errorContent = String.Concat(e.Description, " ");
                    }
                    return Content(errorContent);
                }
            }
            else
            {
                return Content("Model is NOT valid.");
            }
        }
    }
}
