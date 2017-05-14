using GZipLib.GZip;
using GZipLib.GZip.Exceptions;
using GZipTest.File;
using System;
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
                //FileInfo inputInfo = new FileInfo(@"D:\1024000.txt");
                //FileInfo outInfo = new FileInfo(@"D:\1024000.txt.gz");

                //FileHelper.CheckOutputExist(outInfo);

                //var compressor = GZipCompressFactory.Get(CompressionMode.Compress, inputInfo, outInfo);
                //compressor.Start();

                FileInfo inputInfo2 = new FileInfo(@"D:\1024000.txt.gz");
                FileInfo outInfo2 = new FileInfo(@"D:\1024000_test.txt");

                FileHelper.CheckOutputExist(outInfo2);

                var compressor2 = GZipCompressFactory.Get(CompressionMode.Decompress, inputInfo2, outInfo2);
                compressor2.Start();
                

            }
            catch (InvalidDataException e)
            {
                Console.WriteLine("Архив повреждён");
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (FileExtensionException e)
            {
                Console.WriteLine(e.Message);
            }


#if DEBUG
            Console.ReadKey();
#endif
        }

    }
}
