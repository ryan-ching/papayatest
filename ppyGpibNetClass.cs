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
using System.Threading;
using System.Net.Sockets;
using System.Net; //needed for net socket communication
using System.Runtime.InteropServices; // needed for Marshal
using NationalInstruments.Visa;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

/*
 * Papaya C# driver using National instrument VisaNs to communicate to the
 * Papaya Controller.
 * The code is tested with National Instrument NI-VISA 19.0
 * 
 */


namespace PapayaDemo
{
    


    public class VXI11Class
    {
        // this is the multi-threaded version whereby the gpib dev is mapped to a single connection
        // so multiple inst to a host
        #region property
        Dictionary<int, bool> gpibVsLock = new Dictionary<int, bool>(); // keeps track of all the gpib addresses and lock status
        MessageBasedSession mSession = null; // this object mSession
        bool connectionActive = false;
        string error = string.Empty;
        private int _readBufferSize = 16384; // 16kB 
        
        
        public string ipAddress
        {
           set
           {
               ipAddress = value;
           }
            get
            {
                return ipAddress;
            }
        }
        public string device
        {
            set
            {
                device = value;
            }
            get
            {
                return device;
            }
        }

        public int TimeOut
        {
            // connection timeout is in ms
            set
            {
                this.mSession.TimeoutMilliseconds = value;
                
            }
            get
            {
                return this.mSession.TimeoutMilliseconds;
            }
        }

        public int BufferSize
        {
            set
            {
                this._readBufferSize = value;
                
            }
            get
            {
                return this._readBufferSize;
            }
        }
        #endregion property

        #region method

        public VXI11Class()
        {

        }

        public VXI11Class(string device, string ipAddress)
        {
            VXI11ClassConnect(device, ipAddress, false, 0);
        }

        public VXI11Class(string device, string ipAddress, bool lockDevice)
        {
            VXI11ClassConnect(device, ipAddress, lockDevice, 0);
        }

        public VXI11Class(string device, string ipAddress, bool lockDevice, int timeout_ms) 
        {
            VXI11ClassConnect(device, ipAddress, lockDevice, timeout_ms);
        }



        public void VXI11ClassConnect(string device, string ipAddress, bool lockDevice, int timeout_ms)
        {
            // ipAddress is the dot separate address
            // device can be of the form gpib0,gpibAddress
            string[] ary;
            int igpibAddress = 0;
            int typeFormat = 0;
            string resourceName;


            this.error = string.Empty;
            if (device.Contains(@"gpib0")) 
            {
                ary = device.Split(new char[] {','});
                igpibAddress = Convert.ToInt16(ary[1]);
                typeFormat = 0;
            }
            else if(device.Contains(@"inst"))
            {
                igpibAddress = Convert.ToInt16(device.IndexOf(device,4,(int)(device.Length-4)));
                typeFormat = 1;
            }
            else
            {
                throw new Exception("wrong device format");
            }

            resourceName = "TCPIP0::" + ipAddress + "::" + device + "::INSTR";
            //this.ipAddress = ipAddress;
            //ResourceManager localresource =  ResourceManager.GetLocalManager();

            
            try 
            {

                var rmSession = new ResourceManager();
                mSession = (MessageBasedSession)rmSession.Open(resourceName);
                mSession.FormattedIO.ReadBufferSize = _readBufferSize;

                this.TimeOut = 10000;
                Console.WriteLine(mSession.HardwareInterfaceName);
                connectionActive = true;

                // add in the dictionary
                if (lockDevice) 
                {
                    try
                    {
                        this.VXI11Lock(timeout_ms);
                    }
                    catch (InvalidCastException)
                    {
                        MessageBox.Show("Resource selected must be a message-based session");
                    }
                    catch  (Exception ex) 
                    {
                        
                        if (ex is Ivi.Visa.IOTimeoutException)
                            Console.WriteLine( "lock timeout");
                        if (ex is Ivi.Visa.NativeVisaException ) //VisaStatusCode.ErrorIO
                            Console.WriteLine(" io timeout");
                        
                        Console.WriteLine(ex.ToString());
                    }
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
           

        }

       
        public void VXI11Lock()
        {
            try
            {
                this.VXI11Lock(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void VXI11Lock(int timeout_ms)
        {
            try
            {
                mSession.LockResource(timeout_ms);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void VXI11Unlock()
        {
            try
            {
                mSession.UnlockResource();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string read()
        {
            string resp;
            try
            {

                resp = mSession.RawIO.ReadString((long)_readBufferSize);
                return resp;
            }
            
            catch (Exception ex)
            {
                if (ex is Ivi.Visa.IOTimeoutException)
                    Console.WriteLine("io timeout");
                throw ex;
            }

        }

        /*public string read(int timeout_ms)
        {
            // timeout_ms is the time out to lock the device
            string resp= string.Empty;
            try
            {
                mSession.LockResource(timeout_ms);
                resp = mSession.RawIO.ReadString((long)_readBufferSize);
                mSession.UnlockResource();
                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }*/

        public byte[] readBytes()
        {
            try
            {
                
                return mSession.RawIO.Read((long)_readBufferSize);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void write(string cmd)
        {
            try
            {
                mSession.RawIO.Write(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void write(byte[] cmd)
        {
            try
            {
                mSession.RawIO.Write(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /*public void write(string cmd, int timeout_ms)
        {
            // timeout_ms is the time out to lock the device
            try
            {
                mSession.LockResource(timeout_ms);
                mSession.RawIO.Write(cmd);
                mSession.UnlockResource();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
            
        }*/
        
        public void writeI2c(byte devAddr, byte regAddr, byte len,  byte[] data)
        {
            // this is the marshall in order to use vxi11
            // first byte is cmd, 0 = scan, 1 = read, 2 = write
            byte[] byteAry = new byte[4 + len];
            byteAry[0] = 0x02;
            byteAry[1] = devAddr;
            byteAry[2] = regAddr;
            byteAry[3] = Convert.ToByte(len);

            for (int i = 0; i < len; i++)
            {
                byteAry[i + 4] = data[i];
            }
            //string cmd = System.Text.Encoding.Default.GetString(byteAry, 0, byteAry.Length);
            try
            {
                mSession.RawIO.Write(byteAry);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void readI2c(byte devAddr, byte regAddr, byte len,  ref byte[] data)
        {
            // this is the marshall in order to use vxi11
            // first byte is cmd, 0 = scan, 1 = read, 2 = write
            // for read, it is split into 2 steps, write and read
            
            byte[] byteAry = new byte[4];
            byteAry[0] = 0x01;
            byteAry[1] = devAddr;
            byteAry[2] = regAddr;
            byteAry[3] = Convert.ToByte(len);
            
            if (mSession != null)
            {
                this.write(byteAry);
            }
            // complete write
            data = this.readBytes();
        }

        public void writeUart(byte devAddr, byte regAddr, int len, ref byte[] data)
        {
            // this is the marshall in order to use vxi11
            // first byte is cmd, 0 = scan, 1 = read, 2 = write
            // for read, it is split into 2 steps, write and read

            //set_uart_attribs (int fd, int baudrate, int numbits, int parityConfig,int stopbits, int timeout_ms) // time out for serial ports

            // need a method to set, args are baudrate, config
            // need a method to get config
            //
            // need a method to set the timeout to read/write the terminal fiel
            // g_uartByteTimeout_us and g_uartMsgTimeout_ms



            //IntPtr memPtr = IntPtr.Zero;
            IntPtr memPtr = Marshal.AllocHGlobal(5); // allocate 5 bytes
            UInt16 len_word = Convert.ToUInt16(len);
            byte cmdByte = 0x01;
            Marshal.StructureToPtr(cmdByte, memPtr, true);
            Marshal.StructureToPtr(devAddr, memPtr+1, true);
            Marshal.StructureToPtr(regAddr, memPtr + 2, true);
            cmdByte = Convert.ToByte(len);
            Marshal.StructureToPtr(len_word, memPtr + 3, true);
            byte[] byteAry = new byte[4]; //only to test
            byteAry[0] = 0x01;
            byteAry[1] = devAddr;
            byteAry[2] = regAddr;
            byteAry[3] = Convert.ToByte(len);
            
            string cmd_orig = System.Text.Encoding.Default.GetString(byteAry, 0, byteAry.Length);
            string cmd = Marshal.PtrToStringAnsi(memPtr,5);
            byte[] byteAry1 = new byte[5];
            Marshal.Copy(memPtr,byteAry1,0,5);
            
            if (mSession != null)
            {
                this.write(byteAry1);
            }
            Marshal.FreeHGlobal(memPtr);
            // complete write
            data = this.readBytes();
        }

        public void uartSetConfig(int dataRate, byte numBits, byte parity, byte stopBits, int msgTimeout_ms, int byteReadTimeout_us)
        {
            // need a method to set the timeout to read/write the terminal fiel
            // g_uartByteTimeout_us and g_uartMsgTimeout_ms
            // byte[0] : command, readbytes, writebytes, configUart (byteConfig-bitnums,parity,stop,(int)baudrate, (int)msgtimeout,(int)bytetimeout)
            // byte[1]: data length
            // byte[2]: data


            // 
            // command (1), data length(2), uartConfig (1), dataRate(4), ,msgTimeout_ms(4), byteTimeout_us(4)
            // data length is the sum from uartConfig to the end, = 13 

            // bit7, bit6, bit5, bit4 | bit3, bit2, bit1|bit0
            // RS_CHAR_8|RS_PARITY_NONE|RS_STOP_1

            IntPtr memPtr = Marshal.AllocHGlobal(16);
            byte cmdByte = 0x03; // write configuration
            Marshal.StructureToPtr(cmdByte, memPtr,true);
            UInt16 len_word = 0x000D; //13
            Marshal.StructureToPtr(len_word, memPtr + 1, true);

            byte uartByteConfig = 0x00;
            byte tmp = 0x00;
            switch (numBits)
            {
                case 5:
                    tmp = 0x00;
                    break;
                case 6:
                    tmp = 0x01;
                    break;
                case 7:
                    tmp = 0x02;
                    break;
                case 8:
                    tmp = 0x03;
                    break;
                default:
                    tmp = 0x03;
                    break;
            }
            
            uartByteConfig = (byte)(tmp << 0x04);
            // parity
            uartByteConfig |= (byte)(parity << 0x01);
            // stop bits
            uartByteConfig |= (byte)(stopBits-1);
            Marshal.StructureToPtr(uartByteConfig, memPtr + 3, true);
            Marshal.StructureToPtr(dataRate, memPtr + 4, true);
            Marshal.StructureToPtr(msgTimeout_ms, memPtr + 8, true);
            Marshal.StructureToPtr(byteReadTimeout_us, memPtr + 12, true);


            byte[] byteAry1 = new byte[16];
            Marshal.Copy(memPtr, byteAry1, 0, 16);

            if (mSession != null)
            {
                try
                {
                    this.write(byteAry1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            Marshal.FreeHGlobal(memPtr);
            

        }



        public void uartGetConfig(ref int dataRate, ref byte numBits, ref byte parity, ref byte stopBits, ref int msgTimeout_ms, ref int byteReadTimeout_us)
        {
            // command (1), data length(2), uartConfig (1), dataRate(4), ,msgTimeout_ms(4), byteTimeout_us(4)
            // data length is the sum from uartConfig to the end, = 13 

            // bit7, bit6, bit5, bit4 | bit3, bit2, bit1|bit0
            // RS_CHAR_8|RS_PARITY_NONE|RS_STOP_1

            IntPtr memPtr = Marshal.AllocHGlobal(3);
            byte cmdByte = 0x04; // get uart configuration
            Marshal.StructureToPtr(cmdByte, memPtr, true);
            UInt16 len_word = 0x0000; //zero length
            Marshal.StructureToPtr(len_word, memPtr + 1, true);
            byte[] byteAry1 = new byte[3];
            Marshal.Copy(memPtr, byteAry1, 0, 3);
            if (mSession != null)
            {
                try
                {
                    this.write(byteAry1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            Marshal.FreeHGlobal(memPtr);

            byte[] dataReadback = new byte[16];
            dataReadback = this.readBytes();
            
            memPtr = Marshal.AllocHGlobal(dataReadback.Length);
            Marshal.Copy(dataReadback,0,memPtr,dataReadback.Length);
            
            // return the uart parameter
            // check the return length, should be 13
            Int16 val = Marshal.ReadInt16(memPtr + 1);           
            int len =  System.Net.IPAddress.NetworkToHostOrder((short)val); // 16 bit need to change Endian
            if (len != 13)
            {
                InvalidDataException ex = new InvalidDataException();
                throw ex;
            }

            byte uartConfig = dataReadback[3];
            
            stopBits = (byte)(uartConfig & 0x1);
            stopBits += 1;
            
            parity = (byte)((uartConfig & 0x06) >> 1);
            numBits = (byte)(uartConfig >> 4);
            numBits += 5;
            // data rate
            int val32 = Marshal.ReadInt32(memPtr + 4);
            dataRate = val32;
            val32 = Marshal.ReadInt32(memPtr + 8);
            msgTimeout_ms = val32;
            val32 = Marshal.ReadInt32(memPtr + 12);
            byteReadTimeout_us = val32;
        }

        public void uartWriteBytes( int len, byte[] data)
        {
            // need a method to set the timeout to read/write the terminal fiel
            // g_uartByteTimeout_us and g_uartMsgTimeout_ms
            // byte[0] : command, readbytes, writebytes, configUart (byteConfig-bitnums,parity,stop,(int)baudrate, (int)msgtimeout,(int)bytetimeout)
            // byte[1]: data length
            // byte[2]: data


            // 
            // command (1), data length(2), data(data_length)
            // data length is the number of bytes of data not including data_length in bytes

            

            IntPtr memPtr = Marshal.AllocHGlobal(len + 3);
            byte cmdByte = 0x02; // write data
            Marshal.StructureToPtr(cmdByte, memPtr, true);
            UInt16 len_word = Convert.ToUInt16(len);
            Marshal.StructureToPtr(len_word, memPtr + 1, true);

            for (int i = 0; i < len; i++ )
            {
                Marshal.StructureToPtr(data[i], memPtr + 3+i, true);

            }

            

            byte[] byteAry1 = new byte[len+3];
            Marshal.Copy(memPtr, byteAry1, 0, len + 3);

            if (mSession != null)
            {
                try
                {
                    this.write(byteAry1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            Marshal.FreeHGlobal(memPtr);


        }

        public void uartQueryBytes(int len, byte[] data, ref byte[] dataReadback)
        {
            // need a method to set the timeout to read/write the terminal fiel
            // g_uartByteTimeout_us and g_uartMsgTimeout_ms
            // byte[0] : command, readbytes, writebytes, configUart (byteConfig-bitnums,parity,stop,(int)baudrate, (int)msgtimeout,(int)bytetimeout)
            // byte[1]: data length
            // byte[2]: data


            // 
            // command (1), data length(2), data(data_length)
            // data length is the number of bytes of data not including data_length in bytes



            IntPtr memPtr = Marshal.AllocHGlobal(len + 3);
            byte cmdByte = 0x01; // query data
            Marshal.StructureToPtr(cmdByte, memPtr, true);
            UInt16 len_word = Convert.ToUInt16(len);
            Marshal.StructureToPtr(len_word, memPtr + 1, true);

            for (int i = 0; i < len; i++)
            {
                Marshal.StructureToPtr(data[i], memPtr + 3 + i, true);

            }



            byte[] byteAry1 = new byte[len + 3];
            Marshal.Copy(memPtr, byteAry1, 0, len + 3);

            if (mSession != null)
            {
                try
                {
                    this.write(byteAry1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            Marshal.FreeHGlobal(memPtr);

            dataReadback = this.readBytes();


        }
        public void scanI2c(ref byte[] data)
        {
            // marshal for i2c for vxi11
            byte[] byteAry = new byte[1];
            byteAry[0] = 0x00;
            string cmd = System.Text.Encoding.Default.GetString(byteAry, 0, byteAry.Length);
            if (mSession != null)
            {
                this.write(cmd);
            }
            // complete write
            data = this.readBytes();
        }

        public string query(string cmd)
        {
            string resp =  null;
            try
            {
                this.write(cmd);
                resp = this.read();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resp;
        }

        public string query(string cmd, int timeout_ms)
        {
            
            string resp = null;
            try
            {
                this.VXI11Lock(timeout_ms);
                
            }
            catch (Exception ex)
            {
               
                if (ex is Ivi.Visa.IOTimeoutException)
                   return "timeout";
                if (ex is Ivi.Visa.NativeVisaException)
                {
                    Ivi.Visa.NativeVisaException tp = (Ivi.Visa.NativeVisaException)ex;
                    if (tp.ErrorCode == Ivi.Visa.NativeErrorCode.ResourceLocked)
                        return "lock timeout";
                }       
                
                return "io error";
            }
           
            try
            {
                resp = this.query(cmd);
                
            }
            catch (Exception ex)
            {
                // error occurs during query
                // resource has been locked previously
                // perform unlock
                this.VXI11Unlock();

                if (ex is Ivi.Visa.IOTimeoutException)
                    return "timeout";

                return "io error";
            }

            try
            {
                this.VXI11Unlock();
            }
            catch (Exception ex)
            {
                if (ex is Ivi.Visa.NativeVisaException)
                {
                    Ivi.Visa.NativeVisaException tp = (Ivi.Visa.NativeVisaException)ex;
                    if (tp.ErrorCode == Ivi.Visa.NativeErrorCode.SessionNotLocked)
                        return "unlock failure";
                }
                return ex.Message;
                
            }
            
            return resp;
        }
		
		public byte[] queryByte(string cmd)
        {
            byte[] resp = null;
            try
            {
                this.write(cmd);
                resp = this.readBytes();// mSession.ReadByteArray();
            }
            catch (Exception ex)
            {
                //resp = exp.ToString();// string.Empty;
                throw ex;
            }
            return resp;
        }

        public byte[] queryByte(string cmd, int timeout_ms)
        {

            byte[] resp = null;
            try
            {
                this.VXI11Lock(timeout_ms);

            }
            catch (Exception ex)
            {
                // time out occur
                throw ex;
                
            }

            try
            {
                this.write(cmd);//mSession.Write(cmd);
                resp = this.readBytes();// mSession.ReadByteArray();

            }
            catch (Exception ex)
            {
                // time out occur
                throw ex;
                
            }

            try
            {
                this.VXI11Unlock();

            }
            catch (Exception ex)
            {
                //if (ex.ErrorCode == VisaStatusCode.ErrorSessionNotLocked)
                //    return "unlock failure";
                throw ex;
            }

            return resp;
        }
		
		public void binaryToDouble64(byte[] resp, ref double[] real64, bool swappedOrder = false)
        {
            // ieee binary format
            char[] tpStr = System.Text.Encoding.ASCII.GetChars(resp, 0, 1);
            
            
            if (tpStr[0] == '#')
            {
                // ieee 488.2 + ieee 754 format
                
                int lenBytes = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(resp, 1, 1));
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(resp, 1, 1));
                //Console.WriteLine(System.Text.Encoding.ASCII.GetString(resp, 2, lenBytes));
                string lenStr = System.Text.Encoding.ASCII.GetString(resp, 2, lenBytes);
                int lenReal64 = Convert.ToInt32(lenStr) / 8;
                real64 = new double[lenReal64];
                long dummy = 0;
                byte[] dummyByte = null;
                for (int kk = 0; kk < lenReal64; kk++)
                {
                    if (swappedOrder)
                    {
                        dummy = BitConverter.ToInt64(resp, 2 + lenBytes + kk * 8);
                        dummy = System.Net.IPAddress.NetworkToHostOrder(dummy);
                        dummyByte = BitConverter.GetBytes(dummy);
                        //Console.WriteLine(BitConverter.ToDouble(dummyByte,0));
                        real64[kk] = BitConverter.ToDouble(dummyByte, 0);
                    }
                    else
                    {
                        real64[kk] = BitConverter.ToDouble(resp, 2 + lenBytes + kk * 8);
                    }
                }


            }
            
        }

        public delegate string beginQuery(string cmd, int timeout_ms);
        public delegate string beginQuery1(string cmd);


        public void close()
        {
            mSession.Dispose();
        }
        #endregion method
    }

    public enum RpiCommandType { READ = 0, WRITE = 1, QUERY = 2 };
    public class commandClass
    {

        string gpibAddress;
        public int numOfCmds;
        public int numOfErrs;
        public int numOfLockTimeoutErrs;
        public List<double> delay;

        public RpiCommandType cmdType;
        public string cmdStr;

        public commandClass()
        {
            this.numOfCmds = 0;
            this.numOfErrs = 0;
            this.numOfLockTimeoutErrs = 0;
            this.cmdStr = string.Empty;
            this.delay = new List<double>(); //new List<TimeSpan>();
        }

        public string gpibAddr
        {
            get { return this.gpibAddress; }
            set
            {
                this.gpibAddress = value;
            }
        }

    }

  
 
}
