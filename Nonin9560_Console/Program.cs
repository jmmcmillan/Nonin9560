using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nonin9560_BluetoothSPP;

namespace Nonin9560_Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            Nonin9560 pulseOx = new Nonin9560(false, Packets.DataFormat.DataFormat13NoATR);

            pulseOx.LowBatteryAlert += new EventHandler(pulseOx_LowBatteryAlert);

            EventHandler<PulseOxReadingUpdatedEventArgs> readingAcquired = 
                new EventHandler<PulseOxReadingUpdatedEventArgs>(pulseOx_ReadingAcquired);

            pulseOx.GetReading(readingAcquired);
            
        }

        static void pulseOx_LowBatteryAlert(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Low battery!");
        }

        static void pulseOx_ReadingAcquired(object sender, PulseOxReadingUpdatedEventArgs e)
        {
            PulseOxReading por = e.NewReading;
            Console.Out.WriteLine(por.ToString());
        }
    }
}
