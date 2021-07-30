using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PapayaDemo
{
    class Agilent_33401_Test
    {
        public static void Main()
        {
            Agilent_33401 agilent = new Agilent_33401("gpib0,27", "192.168.2.181");
            double acVoltage; double acCurrent; double dcVoltage; double dcCurrent;
            for (int i = 0; i < 10; i++)
            {
                acVoltage = agilent.acVoltage();
                Debug.WriteLine("AC Volt is " + acVoltage.ToString());

                acCurrent = agilent.acCurrent();
                Debug.WriteLine("AC Curr is " + acCurrent.ToString());

                dcVoltage = agilent.dcVoltage();
                Debug.WriteLine("DC Volt is " + dcVoltage.ToString());

                dcCurrent = agilent.acVoltage();
                Debug.WriteLine("DC Curr is " + dcCurrent.ToString());
            }
            
        }
    }
}
