using System;
using System.Collections.Generic;
using System.Text;

namespace VTSPentabPlugin
{
    class DeviceData
    {
        public string Name;
        public int VenderId;
        public int Usage;
        public int UsagePage;

        public DeviceData(string name, int venderId, int usage, int usagePage)
        {
            Name = name;
            VenderId = venderId;
            Usage = usage;
            UsagePage = usagePage;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
