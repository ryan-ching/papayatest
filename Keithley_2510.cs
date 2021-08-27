/*
Copyright (C) 2020 Piek Solutions LLC
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PapayaDemo
{
    public class Keithley_2510
    {
        string deviceName;
        string ipAddress;
        VXI11Class vxi11Device;

        public Keithley_2510(string name, string address)
         {
             this.deviceName = name;
             this.ipAddress = address;
             // Set lockDevice = false, timeout = 2000 ms
             vxi11Device = new VXI11Class(name, address, false, 2000);
         }

        public string DeviceName
        {
            set
            {
                this.deviceName = value;
            }
            get
            {
                return this.deviceName;
            }
        }

        public string IPAddress
        {
            set
            {
                this.ipAddress = value;
            }
            get
            {
                return this.ipAddress;
            }
        }

        public string checkForError()
        {
            return vxi11Device.query("syst:err?");
        }

        public double temp // Temperature must be between 0 and 40 degrees
        {
            set
            {
                vxi11Device.write(":SOUR:TEMP " + Convert.ToString(value));
            }
            get
            {
                try
                {
                    return Convert.ToDouble(vxi11Device.query(":MEAS:TEMP?"));
                }
                catch (System.Exception)
                {
                    return -1000.0;
                    throw new System.Exception("Keithley 2510 warning: temp read error...");
                }
            }
        }

        public double output
        {
            set
            {
                try
                {
                    vxi11Device.write(":OUTPUT " + Convert.ToString(value));
                }
                catch (System.Exception)
                {
                    throw new System.Exception("Keithley 2510 write outp fails");
                }
            }
            get
            {
                try
                {
                    return Convert.ToDouble(vxi11Device.query(":OUTPUT?"));
                }
                catch (System.Exception)
                {
                    throw new System.Exception("Keithley 2510 query outp fails");
                }
            }
        }
    }
}