using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Globals
{
    public class Secret
    {
        public static string Tbot { get; } = "1708987680:AAEuqfqY6gesBkJrWgv99S9QqYMrQn8nrCk";
        public static string GAMonitor { get; set; } = "monitoring.txt";
        public static string GAWithThMonitor { get; set; } = "notification.txt";
        public static string GAList { get; set; } = "gaList.json";
        public static string GAThList { get; set; } = "gaThList.json";
    }
}
