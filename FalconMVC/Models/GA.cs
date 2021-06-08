using FalconMVC.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models
{
    public class GA
    {
        public int Id { get; set; }
        public string GAddress { get; set; }
        public DptType? GType { get; set; }
    }
}
