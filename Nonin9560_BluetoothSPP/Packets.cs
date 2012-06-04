using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Nonin9560_BluetoothSPP
{
    public static class Packets
    {
        public const byte ACK = 0x06;
        public const byte NAK = 0x15;

        public enum DataFormat { DataFormat13, DataFormat13NoATR, DataFormat8, DataFormat2, DataFormat7 };

        public static byte[] SelectDataFormat8 = { 0x02, 0x70, 0x04, 0x02, 0x02, 0x00, 0x78, 0x03 };
        public static byte[] SelectDataFormat13 = { 0x02, 0x70, 0x04, 0x02, 0x0D, 0x00, 0x83, 0x03 };
        public static byte[] SelectDataFormat13NoATR = { 0x02, 0x70, 0x02, 0x00, 0x02, 0x03 };

        public static byte[] RetrieveDateTime = { 0x02, 0x72, 0x00, 0x03 };
        public static byte[] RetrieveSerialNumber = { 0x02, 0x74, 0x02, 0x02, 0x02, 0x03 };


        public static Dictionary<DataFormat, byte[]> DataFormatPackets = new Dictionary<DataFormat, byte[]>()
        {
            {DataFormat.DataFormat8, SelectDataFormat8 },
            {DataFormat.DataFormat13, SelectDataFormat13 },
            {DataFormat.DataFormat13NoATR, SelectDataFormat13NoATR }

        };

    }
}
