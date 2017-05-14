using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    class DataBlock
    {
        public DataBlock(byte[] _data, int _dataSize, long _index)
        {
            Data = _data;
            Index = _index;
            DataSize = _dataSize;
        }

        public byte[] Data
        {
            get;
            private set;
        }
        public long Index
        {
            get;
            private set;
        }
        public int DataSize
        {
            get;
            private set;
        }

    }
}
