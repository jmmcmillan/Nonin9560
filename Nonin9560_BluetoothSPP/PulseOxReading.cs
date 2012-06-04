using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Nonin9560_BluetoothSPP
{
    public class PulseOxReading
    {
        public int SpO2 { get; set; }
        public int HeartRate { get; set; }
        public bool IsReliable { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRealTime { get; set; }
        public bool IsMissingData { get; set; }
        public bool DeviceLowBattery { get; set; }
        public PulseOxReading()
        {
            IsReliable = false;
        }

        public PulseOxReading(byte[] data): this()
        {
            if (data.Length != 22)
                return;

            BitArray statusMSB = new BitArray(new byte[] { data[14] });
            BitArray statusLSB = new BitArray(new byte[] { data[15] });

            IsMissingData = statusMSB.Get(0);  //no measurement of either HR or O2
            IsReliable = statusMSB.Get(1);  //reading uses Nonin's smart point algorithm

            DeviceLowBattery = statusLSB.Get(0);
            IsRealTime = statusLSB.Get(4);  //as opposed to a reading from stored memory

            Timestamp = PacketUtil.ParseDateTimeAsBCDs(data.Skip(7).Take(6).ToArray<byte>());

            SpO2 = data[19];            
            HeartRate = BitConverter.ToInt16(new byte[] {data[17], data[16]}, 0);

        }

        public override string ToString()
        {
            return String.Format("Pulse: {0}, SpO2: {1}% ({2:g})", HeartRate, SpO2, Timestamp);
        }
    }
}
