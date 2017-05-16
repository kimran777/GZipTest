using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    class ExtraField
    {
        public byte SubID1
        {
            get; private set;
        }

        public byte SubID2
        {
            get; private set;
        }

        public byte[] Data
        {
            get;
            private set;
        }

        public ExtraField(byte sid1, byte sid2, byte[] data)
        {
            SubID1 = sid1;
            SubID2 = sid2;
            Data = data;
        }
    }
}
