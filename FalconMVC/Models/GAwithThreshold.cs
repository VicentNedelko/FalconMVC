using FalconMVC.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models
{
    public class GAwithThreshold
    {
        public Guid Id { get; set; }
        public string GAddress { get; set; }
        public DptType? GType { get; set; }
        public string Description { get; set; }
        public decimal ThresholdMin { get; set; }
        public decimal ThresholdMax { get; set; }
        public bool IsCheck { get; set; } = false;
    }
}
