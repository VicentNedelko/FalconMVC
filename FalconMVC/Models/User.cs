using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models
{
    public class User : IdentityUser
    {
        public string FName { get; set; }
        public string LName { get; set; }
    }
}
