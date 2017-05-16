using GZipLib.GZip;
using GZipLib.GZip.Exceptions;
using GZipTest.File;
using GZipTest.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                CompressInfo cInfo = ArgsReader.Read(args);

                cInfo = new CompressInfo(CompressionMode.Compress, new FileInfo(@"D:\3221225470.data"), new FileInfo(@"D:\3221225470.data.gz"));


                FileHelper.CheckOutputExist(cInfo.OutFileName);

                var compressor = GZipCompressFactory.Get(cInfo.CompressMode, cInfo.InFileName, cInfo.OutFileName);
                compressor.Start();



                cInfo = new CompressInfo(CompressionMode.Decompress, new FileInfo(@"D:\3221225470.data.gz"), new FileInfo(@"D:\3221225470_Out.data"));
                
                FileHelper.CheckOutputExist(cInfo.OutFileName);

                compressor = GZipCompressFactory.Get(cInfo.CompressMode, cInfo.InFileName, cInfo.OutFileName);
                compressor.Start();

                //FileInfo inputInfo = new FileInfo(@"D:\1024000.txt");
                //FileInfo outInfo = new FileInfo(@"D:\1024000.txt.gz");

                //FileHelper.CheckOutputExist(outInfo);

                //var compressor = GZipCompressFactory.Get(CompressionMode.Compress, inputInfo, outInfo);
                //compressor.Start();

                //FileInfo inputInfo2 = new FileInfo(@"D:\1024000.txt.gz");
                //FileInfo outInfo2 = new FileInfo(@"D:\1024000_test.txt");

                //FileHelper.CheckOutputExist(outInfo2);

                //var compressor2 = GZipCompressFactory.Get(CompressionMode.Decompress, inputInfo2, outInfo2);
                //compressor2.Start();


            }
            catch (InvalidDataException e)
            {
                Console.WriteLine("Архив повреждён");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);            
            }
            
                        
        }

    }
}
