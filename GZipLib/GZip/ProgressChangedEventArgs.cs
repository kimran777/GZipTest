using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    class ProgressChangedEventArgs : EventArgs
    {
        public int Progress { get; private set; }

        public ProgressChangedEventArgs()
        {

        }
    }
}
