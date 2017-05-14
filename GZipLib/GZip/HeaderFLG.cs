using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    [Flags]
    enum HeaderFLG : byte
    {
        FTEXT    = 0,
        FHCRC    = 2,
        FEXTRA   = 4,
        FNAME    = 8,
        FCOMMENT = 16
    }
}
