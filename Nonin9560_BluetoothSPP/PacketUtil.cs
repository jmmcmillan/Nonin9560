using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonin9560_BluetoothSPP
{
    public static class PacketUtil
    {
        public static DateTime ParseDateTimeAsBytes(byte[] responseBytes)
        {
            DateTime dt = DateTime.MinValue;

            if (responseBytes.Length == 6)
            {
                try
                {
                    int year = Convert.ToInt32(responseBytes[0]);
                    int month = Convert.ToInt32(responseBytes[1]);
                    int day = Convert.ToInt32(responseBytes[2]);
                    int hour = Convert.ToInt32(responseBytes[3]);
                    int minute = Convert.ToInt32(responseBytes[4]);
                    int second = Convert.ToInt32(responseBytes[5]);

                    dt = new DateTime(year, month, day, hour, minute, second);
                }

                catch (Exception ex) { }
            }

            return dt;
        }

        public static DateTime ParseDateTimeAsBCDs(byte[] responseBytes)
        {
            DateTime dt = DateTime.MinValue;

            if (responseBytes.Length == 6)
            {
                try
                {
                    int year = BCDToInt(responseBytes[0]);
                    int month = BCDToInt(responseBytes[1]);
                    int day = BCDToInt(responseBytes[2]);
                    int hour = BCDToInt(responseBytes[3]);
                    int minute = BCDToInt(responseBytes[4]);
                    int second = BCDToInt(responseBytes[5]);

                    dt = new DateTime(year + 2000, month, day, hour, minute, second);
                }

                catch (Exception ex) { }
            }

            return dt;
        }

        private static int BCDToInt(byte b)
        {   
            int leftChar = (b >> 4) & 0x0F;
            int rightChar = b & 0x0F;

            string num = leftChar.ToString() + rightChar.ToString();
            return Int32.Parse(num);
        }

        public static string BytesToSerial(byte[] data)
        {
            StringBuilder sb = new StringBuilder(9);

            foreach (byte b in data)
                sb.Append((char)b);

            return sb.ToString();
        }
    

    }
}
