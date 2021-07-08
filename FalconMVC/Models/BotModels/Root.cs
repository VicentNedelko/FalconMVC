using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models.BotModels
{
    public class Root
    {
        public bool Ok { get; set; }
        public List<Updates> Result { get; set; }
    }
}
