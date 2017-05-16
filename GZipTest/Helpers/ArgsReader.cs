using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GZipTest.Helpers
{
    class ArgsReader
    {
        public static CompressInfo Read(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Использование: GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла]");
            }



            string strComprMode = args[0];

            if (strComprMode == "compress")
            {
                return new CompressInfo(CompressionMode.Compress, new FileInfo(args[1]), new FileInfo(args[2]));
            }
            else if (strComprMode == "decompress")
            {

                return new CompressInfo(CompressionMode.Decompress, new FileInfo(args[1]), new FileInfo(args[2]));
            }
            else
            {
                throw new ArgumentException("Первый параметр должен быть compress или decompress");
            }


        }
    }
}
