using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PapayaDemo
{
    class TecTest
    {
        //private static System.Timers.Timer aTimer;
        public static void Main()
        {
            // Instantiating instruments
            Agilent_33401 a33401 = new Agilent_33401("gpib0,27", "192.168.2.181");
            Agilent_E3631 a3631 = new Agilent_E3631("gpib0,6", "192.168.2.181");
            Agilent_86142 a86142 = new Agilent_86142("gpib0,11", "192.168.2.181");
            Keithley_2510 k2510 = new Keithley_2510("gpib0,5", "192.168.2.181");
            Keithley2400vxi11 k2400 = new Keithley2400vxi11("gpib0,15", "192.168.2.181", 2000);

            // Agilent 86142 Variables
            String trace; String[] ary; double stopwav;
            List<double> y;

            // Agilent 33401 Variables
            double acVoltage; double acCurrent; double dcVoltage; double dcCurrent;
            double twoWireRead; double fourWireRead; double measureDiode; double db;

            // Keithley 2510 Variables
            double tempOut;
            double queryResult;

            // Agilent E3631 Variables
            bool onOffTest1; bool onOffTest2;

            // Keithley 2400 Variables
            Keithley2400vxi11.SourceModeType sourceVoltage = Keithley2400vxi11.SourceModeType.voltage;
            Keithley2400vxi11.SourceModeType sourceCurrent = Keithley2400vxi11.SourceModeType.current;

            Keithley2400vxi11.SenseModeType senseAllOn = Keithley2400vxi11.SenseModeType.AllOn;
            Keithley2400vxi11.SenseModeType senseAllOff = Keithley2400vxi11.SenseModeType.AllOff;
            Keithley2400vxi11.SenseModeType senseVoltage = Keithley2400vxi11.SenseModeType.Voltage;
            Keithley2400vxi11.SenseModeType senseCurrent = Keithley2400vxi11.SenseModeType.Current;
            Keithley2400vxi11.SenseModeType senseResistance = Keithley2400vxi11.SenseModeType.Resistance;

            Keithley2400vxi11.SelectTerminals selectFront = Keithley2400vxi11.SelectTerminals.Front;
            Keithley2400vxi11.SelectTerminals selectRear = Keithley2400vxi11.SelectTerminals.Rear;

            for (int i = 0; i < 10; i++)
            {
                // AGILENT 86142 TEST
                
                a86142.startWaveLength = 900;
                a86142.stopWaveLength =  1700;
                a86142.traceLength = 1001;
               
                trace = a86142.getTrace();
                ary = trace.Split(',');
                y = new List<double>();
                for (int j = 0; j < ary.Length; j++)
                {
                    y.Add(Convert.ToDouble(ary[j]));    
                }
                stopwav = Convert.ToDouble(a86142.stopWaveLength);

                Debug.WriteLine("a86142 start wavelength : " + Convert.ToString(a86142.startWaveLength));
                Debug.WriteLine("a86142 start wavelength : " + Convert.ToString(a86142.stopWaveLength));
                Debug.WriteLine("a86142 trace length : " + Convert.ToString(a86142.traceLength));
                //Debug.WriteLine("Printing getTrace: ");
                Debug.WriteLine("Lenth of trace : " + Convert.ToString(ary.Length));
                /*
                foreach(String x in ary)
                {
                    Debug.WriteLine(x);
                }
                */
                
               // Debug.Write(ary);
               // Debug.Write(y);
                

                // AGILENT 33401 TEST
                
                /*
                dcVoltage = a33401.dcVoltage();
                dcCurrent = a33401.dcCurrent();
                */
                acVoltage = a33401.acVoltage();
                acCurrent = a33401.acCurrent();
                twoWireRead = a33401.twoWireRes();
                System.Threading.Thread.Sleep(2000);
                fourWireRead = a33401.fourWireRes();
                measureDiode = a33401.measureDiode();
                db = a33401.dbValue();

                Debug.WriteLine("Agilent 33401 AC Volt is " + acVoltage.ToString());
                Debug.WriteLine("Agilent 33401 AC Curr is " + acCurrent.ToString());
                /*
                Debug.WriteLine("Agilent 33401 DC Volt is " + dcVoltage.ToString());
                Debug.WriteLine("Agilent 33401 DC Curr is " + dcCurrent.ToString());
                */
                Debug.WriteLine("Agilent 33401 2 wire resistance is " + twoWireRead.ToString());
                Debug.WriteLine("Agilent 33401 4 wire resistance is " + fourWireRead.ToString());
                Debug.WriteLine("Agilent 33401 Diode reading is " + measureDiode.ToString());
                Debug.WriteLine("Agilent 33401 dB reading is " + db.ToString());

                // KEITHLEY 2510 TEST
                k2510.temp = Convert.ToDouble(i+1);
                tempOut = k2510.temp;
                
                k2510.output = Convert.ToDouble((i+1) % 2);
                queryResult = k2510.output;

                Debug.WriteLine("Keithley 2510 Test, Temp is " + Convert.ToString(tempOut));
                Debug.WriteLine("Keithley 2510 Test, Output is " + Convert.ToString(queryResult));

                // AGILENT E3631 TEST
                /*
                // Testing on off power supply
                a3631.outputOnOff = Convert.ToBoolean(i % 2 == 0);
                onOffTest1 = a3631.outputOnOff;
                a3631.outputOnOff = Convert.ToBoolean((i + 1) % 2 == 0); 
                onOffTest2 = a3631.outputOnOff;
                

                // Scaling index to appropriate voltage ranges (0-6V, 0-25V, -25-0V)
                a3631.P6Supply = i * 0.6;
                a3631.P25Supply = i * 2.5;
                a3631.N25Supply = i * -2.5;

                double P6Output = a3631.P6Supply;
                double P25Output = a3631.P25Supply;
                double N25Output = a3631.N25Supply;

                Debug.WriteLine("onOff 1 = " + Convert.ToString(onOffTest1) +
                                "onOff 2 = " + Convert.ToString(onOffTest2));
                Debug.WriteLine("P6 Voltage is " + P6Output);
                Debug.WriteLine("P25 Voltage is " + P25Output);
                Debug.WriteLine("N25 Voltage is " + N25Output);
                */
                // KEITHLEY 2400 TEST
                // Need to read thru expected vaules still
                /*
                if (i % 2 == 0)
                {
                    k2400.SourceMode = sourceVoltage;
                } 
                else
                {
                    k2400.SourceMode = sourceCurrent;
                }
                */
                //set to on
                k2400.OutputIsOn = Convert.ToBoolean(0);
                Debug.WriteLine("Source Before : " + Convert.ToString(k2400.SourceMode));
                k2400.SourceMode = sourceCurrent;
                Debug.WriteLine("Source Current : " + Convert.ToString(k2400.SourceMode));
                k2400.SourceMode = sourceVoltage;
                Debug.WriteLine("Source Voltage : " + Convert.ToString(k2400.SourceMode));

                // Must Turn Sense Mode Off between function calls to test individual setting
                k2400.SenseMode = senseAllOff;
                Debug.WriteLine("Sense All Off : " + Convert.ToString(k2400.SenseMode));
                k2400.SenseMode = senseAllOn;
                Debug.WriteLine("Sense All On : " + Convert.ToString(k2400.SenseMode));
                k2400.SenseMode = senseAllOff;
                k2400.SenseMode = senseVoltage;
                Debug.WriteLine("Sense Voltage : " + Convert.ToString(k2400.SenseMode));
                k2400.SenseMode = senseAllOff;
                k2400.SenseMode = senseCurrent;
                Debug.WriteLine("Sense Current : " + Convert.ToString(k2400.SenseMode));
                k2400.SenseMode = senseAllOff;
                k2400.SenseMode = senseResistance;
                Debug.WriteLine("Sense Resistance : " + Convert.ToString(k2400.SenseMode));
                k2400.SenseMode = senseAllOff;
                k2400.SenseMode = senseCurrent;

                k2400.Terminal = selectFront;
                Debug.WriteLine("Terminal Front : " + Convert.ToString(k2400.Terminal));
                k2400.Terminal = selectRear;
                Debug.WriteLine("Terminal Rear : " + Convert.ToString(k2400.Terminal));
                k2400.OutputIsOn = true;
                System.Threading.Thread.Sleep(2000);
                
                Debug.WriteLine("Keithley2400 current compliance = " + Convert.ToString(k2400.CurrentCompliance));
                Debug.WriteLine("Keithley2400 voltage compliance = " + Convert.ToString(k2400.VoltageCompliance));
                if (k2400.OutputIsOn)
                {
                    // sense volt source curr 
                    
                    k2400.VoltageSetpoint = (i + 1)*0.5; // Voltage limit from 20 to 210V
                    k2400.CurrentCompliance = 1.0; // Voltage lmit 10nA to 3.15A

                    System.Threading.Thread.Sleep(2000);

                    k2400.SenseMode = senseAllOff;
                    k2400.SenseMode = senseCurrent;
                    k2400.CurrentCompliance = (i + 1) * 10; // Current limit 1nA to 1.05A
                    
                }
                k2400.SenseMode = senseAllOff;
                k2400.FourWireIsOn = Convert.ToBoolean((i+1) % 2 == 0);
                Debug.WriteLine("Keithley2400 voltage setpoint = " + Convert.ToString(k2400.VoltageSetpoint));
                Debug.WriteLine("Keithley2400 current compliance = " + Convert.ToString(k2400.CurrentCompliance));
                Debug.WriteLine("Keithley2400 voltage compliance = " + Convert.ToString(k2400.VoltageCompliance));
                Debug.WriteLine("Keithley2400 output on/off = " + Convert.ToString(k2400.OutputIsOn));
                Debug.WriteLine("Keithley2400 4 wire on/off = " + Convert.ToString(k2400.FourWireIsOn));

                /*
                 * Need to finish Terminal, SenseMode, SourceType
                if (i % 2 == 0) { sourceType = "VOLT"; }
                else { sourceType = "CURR";  }
                k2400.SourceMode = sourceType;
                */
                
            }
        }
        
    }
}
