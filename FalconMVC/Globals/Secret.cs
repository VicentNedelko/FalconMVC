using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Globals
{
    public class Secret
    {
        public static string Tbot { get; } = "1987317825:AAGWK_-w_WN5eWf3YxYrwLyYARcx19c4zhU";
        public static string GAMonitor { get; set; } = "monitoring.txt";
        public static string GAWithThMonitor { get; set; } = "notification.txt";
        public static string GAList { get; set; } = "gaList.json";
        public static string GAThList { get; set; } = "gaThList.json";
        public static long GIRAChatId { get; } = 481679093;
        public static string BotName { get; } = "@GIRA_Notification_Bot";
    }
}
