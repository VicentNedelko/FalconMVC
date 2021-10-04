using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IHeos
    {
        public List<Item> Items { get; set; }
        public class Item
        {
            public int Pid { get; set; }
            public string Ip { get; set; }
            public string Name { get; set; }
        }

    }
}
