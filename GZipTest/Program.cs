using GZipLib;
using GZipLib.GZip;
using GZipTest.File;
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
                FileInfo inputInfo = new FileInfo(@"D:\1024000.txt");
                FileInfo outInfo = new FileInfo(@"D:\1024000.txt.gz");

                FileHelper.CheckOutputExist(outInfo);

                var compressor = GZipCompressFactory.Get(CompressionMode.Compress, inputInfo, outInfo);
                compressor.Start();

                FileInfo inputInfo2 = new FileInfo(@"D:\1024000.txt.gz");
                FileInfo outInfo2 = new FileInfo(@"D:\1024000_test.txt");

                FileHelper.CheckOutputExist(outInfo);

                var compressor2 = GZipCompressFactory.Get(CompressionMode.Decompress, inputInfo2, outInfo2);
                compressor2.Start();



            }
            catch (InvalidDataException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}
