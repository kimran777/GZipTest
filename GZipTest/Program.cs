using GZipLib;
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
            FileInfo inputInfo = new FileInfo(@"D:\0.txt");
            FileInfo outInfo = new FileInfo(@"D:\0.txt.gz");

            using (FileStream fs = inputInfo.Open(FileMode.Open, FileAccess.Read))
            {
                using (FileStream ms = outInfo.Open(FileMode.Create, FileAccess.ReadWrite))
                {
                    using (GZipStreamEx gzip = new GZipStreamEx(ms, CompressionMode.Compress, inputInfo.Name))
                    {
                        byte[] buffer = new byte[4096];
                        int readedByte = 0;
                        do
                        {
                            readedByte = fs.Read(buffer, 0, 4096);
                            gzip.Write(buffer, 0, readedByte);
                        }
                        while (readedByte != 0);
                    }
                }
            }

            FileInfo inputInfo2 = new FileInfo(@"D:\0.txt.gz");
            FileInfo outInfo2 = new FileInfo(@"D:\0_test.txt");

            using (FileStream fs = inputInfo2.Open(FileMode.Open, FileAccess.Read))
            {
                using (FileStream ms = outInfo2.Open(FileMode.Create, FileAccess.ReadWrite))
                {
                    using (GZipStreamEx gzip = new GZipStreamEx(fs, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int readedByte = 0;
                        do
                        {
                            readedByte = gzip.Read(buffer, 0, 4096);
                            ms.Write(buffer, 0, readedByte);
                        }
                        while (readedByte != 0);
                    }
                }
            }

        }



    }
}
