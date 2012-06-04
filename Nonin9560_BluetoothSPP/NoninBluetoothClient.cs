using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System.IO;
using Nonin9560_BluetoothSPP;
using System.Timers;
using System.Net.Sockets;
using System.Threading;

namespace Nonin9560_BluetoothSPP
{
    

    public class NoninBluetoothClient : IDisposable
    {
        public event EventHandler MostRecentReadingUpdated;
        public event EventHandler<PulseOxErrorEventArgs> UnrecoverableError;

        public PulseOxReading MostRecentReading = null;

        private static Guid serviceGuid = new Guid("{3316de30-cf61-4fb3-9a2b-997ecd1ee15d}");
        
        private BluetoothClient noninClient;
        private BluetoothEndPoint noninEndpoint;
              

        private DateTime connectionBegin = DateTime.MinValue;

        private bool ATREnabled = false;
        private Packets.DataFormat DataFormat;

        private Stream noninStream
        {
            get
            {
                if (!noninClient.Connected)
                {
                    noninClient.Connect(noninEndpoint);
                }

                return noninClient.GetStream();
            }
        }

        public bool IsDiscoverable
        {
            get { return ((DateTime.Now.Subtract(connectionBegin)) <= TimeSpan.FromMinutes(2)); }
        }

        

        private bool DataTransmissionIsContinuous
        {
            get
            {
                return (DataFormat.Equals(Packets.DataFormat.DataFormat2) ||
                    DataFormat.Equals(Packets.DataFormat.DataFormat7) ||
                    DataFormat.Equals(Packets.DataFormat.DataFormat8) );
            }
        }

        public bool IsListening
        {
            get
            {
                /* if the data gets sent continuously, we have 5 seconds post-connection to communicate */
                if (DataTransmissionIsContinuous)
                {
                    return ((DateTime.Now.Subtract(connectionBegin)) <= TimeSpan.FromSeconds(5));                    
                }

                /* if the data gets sent once, wait until it's finished */
                else
                {
                    return (MostRecentReading != null);
                }
            }
        }

        public void Dispose()
        {
            if (noninStream != null)
                noninStream.Close();
        }

        public string GetSerialNumber()
        {
            string serialNumber = String.Empty;

            WriteStream(Packets.RetrieveSerialNumber);

            byte[] responseBytes = ReadStream(15);
            byte[] serialBytes = responseBytes.Skip(4).Take(9).ToArray<byte>();

            serialNumber = PacketUtil.BytesToSerial(serialBytes);

            return serialNumber;
        }

        public DateTime GetDeviceDateTime()
        {
            DateTime dt = DateTime.MinValue;

            WriteStream(Packets.RetrieveDateTime);
            byte[] responseBytes = ReadStream(10);

            dt = PacketUtil.ParseDateTimeAsBytes(responseBytes.Skip(3).Take(6).ToArray<byte>());
            
            return dt;
        }

        public void SetDeviceDateTime(DateTime dt)
        {            
            string twoDigitYear = dt.ToString("yy");
            byte year = Convert.ToByte(Int32.Parse(twoDigitYear));

            byte month = Convert.ToByte(dt.Month);
            byte day = Convert.ToByte(dt.Day);
            
            byte hour = Convert.ToByte(dt.Hour);

            byte minute = Convert.ToByte(dt.Minute);
            byte second = Convert.ToByte(dt.Second);           
            
            byte[] dtBytes = { 0x02, 0x72, 0x06, year, month, day, hour, minute, second, 0x03 };
                        
            WriteStream(dtBytes);
        }

        private bool SetDataFormat(Packets.DataFormat df)
        {
            byte[] selectDFPacket = Packets.DataFormatPackets[df];

            WriteStream(selectDFPacket);

            byte[] responseBytes = ReadStream(1);

            if (responseBytes[0].Equals(Packets.ACK))
            {
                return true;
            }
           
            return false;
        }

        private BluetoothClient GetRadio()
        {
            BluetoothRadio btradio = BluetoothRadio.PrimaryRadio;

            if (btradio == null)
            {
                return null;
            }

            BluetoothEndPoint endPoint = new BluetoothEndPoint(btradio.LocalAddress, serviceGuid);                      
            BluetoothClient thisRadio = new BluetoothClient(endPoint);

            return thisRadio;
        }

        private BluetoothDeviceInfo GetPulseOx(BluetoothClient thisRadio)
        {
            BluetoothDeviceInfo[] devices = thisRadio.DiscoverDevices();
            BluetoothDeviceInfo pulseOx = devices.Where(di =>
                    di.DeviceName.StartsWith("Nonin_Medical")).FirstOrDefault();

            return pulseOx;
        }

        private void SetError(string errorMessage, Exception ex)
        {
            if (UnrecoverableError != null)
                UnrecoverableError(null, new PulseOxErrorEventArgs(errorMessage, ex));
        }

        private void SetError(string errorMessage)
        {
            if (UnrecoverableError != null)
                UnrecoverableError(null, new PulseOxErrorEventArgs(errorMessage));            
        }
        
        public void MakeConnection()
        {          

            BluetoothClient thisRadio = GetRadio();

            if (thisRadio == null)
            {
                SetError("Bluetooth radio not found"); 
                return;
            }

            BluetoothDeviceInfo pulseOx = GetPulseOx(thisRadio);           

            if (pulseOx == null)
            {
                SetError("Pulse oximeter not found");
                return;
            }

            noninEndpoint = new BluetoothEndPoint(pulseOx.DeviceAddress, BluetoothService.SerialPort);

            /* Listen mode   */
            if (ATREnabled)
            {
                BluetoothListener btListen = new BluetoothListener(BluetoothService.SerialPort);
                btListen.Start();
                noninClient = btListen.AcceptBluetoothClient();
            }

            /* Connect mode */
            else
            {
                noninClient = new BluetoothClient();

                try
                {
                    noninClient.Connect(noninEndpoint);
                }

                catch (SocketException ex)
                {
                    SetError("Couldn't connect to pulse oximeter", ex);
                }
            }

            if (noninClient.Connected)
            {
                connectionBegin = DateTime.Now;
            }
                      

            GetMostRecentReading();
        }

        public NoninBluetoothClient(bool atrEnabled, Packets.DataFormat dataFormat)
        {
            ATREnabled = atrEnabled;
            DataFormat = dataFormat;
        }

        public void GetMostRecentReading()
        {
            byte[] data = new byte[22];

            do
            {
                data = ReadStream(22);
            }
            while (((NetworkStream)noninStream).DataAvailable);

            PulseOxReading reading = new PulseOxReading(data);

            if (!reading.IsMissingData)
            {
                MostRecentReading = reading;

                if (MostRecentReadingUpdated != null)
                {
                    MostRecentReadingUpdated(this, new EventArgs());
                }

            }
        }

       
        private byte[] ReadStream(int numBytes)
        {
            byte[] dataBytes = new byte[numBytes];

            for (int i = 0; i < numBytes; i++)
            {   
                Stream data = noninStream;
                dataBytes[i] = (byte)data.ReadByte();
            }

            return dataBytes;
        }

        private void WriteStream(byte[] packet)
        {            
            noninStream.Write(packet, 0, packet.Length);
        }


        private void WatchStream()
        {
            if (true)
            {
                Stream data = noninClient.GetStream();

                byte[] dataBytes = new byte[22];

                data.Read(dataBytes, 0, 22);

                Console.Out.WriteLine("\n");

                foreach (byte b in dataBytes)
                    Console.Out.Write(b + " ");

                Console.Out.WriteLine("\n");

            }


        }
    }


}
