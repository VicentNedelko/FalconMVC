using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IMonitor
    {
        public void Start();
        public void Stop();
    }
}
