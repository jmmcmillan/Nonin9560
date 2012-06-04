using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonin9560_BluetoothSPP
{
    public class Nonin9560
    {
        private NoninBluetoothClient btClient;

        private string serialNumber = String.Empty;
        public string SerialNumber
        {
            get
            {
                if (String.IsNullOrEmpty(serialNumber))
                {
                    serialNumber = btClient.GetSerialNumber();
                }
                return serialNumber;
            }
        }

        private DateTime deviceDateTime = DateTime.MinValue;
        public DateTime DeviceDateTime
        {
            get
            {
                if (deviceDateTime.Equals(DateTime.MinValue))
                {
                    deviceDateTime = btClient.GetDeviceDateTime();
                }
                return deviceDateTime;
            }
        }


        public bool IsEnabled { get; set; }

        public bool AttemptToReconnect { get; set; }
        public Packets.DataFormat DataFormat { get; set; }

        public event EventHandler LowBatteryAlert;
        public event EventHandler<PulseOxReadingUpdatedEventArgs> ReadingAcquired;

        private void ReadingUpdated(object obj, EventArgs args)
        {
            PulseOxReading MostRecentReading = btClient.MostRecentReading;

            if (MostRecentReading.DeviceLowBattery)
            {
                if (LowBatteryAlert != null)
                    LowBatteryAlert(this, new EventArgs());
            }

            if (ReadingAcquired != null)
            {
                ReadingAcquired(this, new PulseOxReadingUpdatedEventArgs(MostRecentReading));
            }
        }


        public Nonin9560(bool attemptToReconnectEnabled, Packets.DataFormat dataFormat)
        {
            this.AttemptToReconnect = attemptToReconnectEnabled;
            this.DataFormat = dataFormat;

            btClient = new NoninBluetoothClient(AttemptToReconnect, DataFormat);

            btClient.MostRecentReadingUpdated += new EventHandler(ReadingUpdated);
            btClient.UnrecoverableError += new EventHandler<PulseOxErrorEventArgs>(btClient_UnrecoverableError);

            this.IsEnabled = true;
        }

        void btClient_UnrecoverableError(object sender, PulseOxErrorEventArgs e)
        {
            this.IsEnabled = false;

            string message = e.ErrorMessage;
            Console.Out.WriteLine(message);

            if (e.OriginalException != null)
                Console.Out.WriteLine(e.OriginalException.StackTrace);
        }

        public void GetReading(EventHandler<PulseOxReadingUpdatedEventArgs> readingUpdated)
        {
            ReadingAcquired += new EventHandler<PulseOxReadingUpdatedEventArgs>(readingUpdated);

            try
            {
                btClient.MakeConnection();
            }

            catch (Exception ex)
            {
                this.IsEnabled = false;
                Console.Out.WriteLine(ex.StackTrace);
            }
        }




    }
}
