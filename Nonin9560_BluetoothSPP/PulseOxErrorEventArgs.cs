using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonin9560_BluetoothSPP
{
    public class PulseOxErrorEventArgs : EventArgs
    {
        public string ErrorMessage;
        public Exception OriginalException;

        public PulseOxErrorEventArgs(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        public PulseOxErrorEventArgs(string errorMessage, Exception originalException) 
            : this(errorMessage)
        {            
            this.OriginalException = originalException;
        }
    }
}
