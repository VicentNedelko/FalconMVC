using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Managers
{
    public interface IInterfaceConnect
    {
        public bool CheckConnection(string interfaceIP);
    }
}
