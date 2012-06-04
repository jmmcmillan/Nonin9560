using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonin9560_BluetoothSPP
{
    public class PulseOxReadingUpdatedEventArgs : EventArgs
    {
        public PulseOxReading NewReading;

        public PulseOxReadingUpdatedEventArgs(PulseOxReading por)
        {
            this.NewReading = por;
        }
    }
}
