using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipLib
{
    class FileNameHelper
    {
        static Encoding iso = Encoding.GetEncoding("ISO-8859-1");
        static Encoding utf8 = Encoding.UTF8;

        public static string BytesToGZipString(byte[] input)
        {
            return iso.GetString(Encoding.Convert(utf8, iso, input));
        }
        public static byte[] StringToGZipBytes(string input)
        {
            return Encoding.Convert(iso, utf8, iso.GetBytes(input));
        }
    }
}
