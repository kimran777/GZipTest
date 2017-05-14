using GZipLib;
using GZipTest.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FileInfo inputInfo = new FileInfo(@"D:\1024000.txt_");
                FileInfo outInfo = new FileInfo(@"D:\1024000.txt.gz");

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}
