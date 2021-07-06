using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models.BotModels
{
    public class From
    {
        public int Id { get; set; }
        public bool Is_bot { get; set; }
        public string First_name { get; set; }
        public string Language_code { get; set; }
    }
}
