using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    public class GZipCompressFactory
    {
        public static Compressor Get(CompressionMode compressionMode, FileInfo inFileInfo, FileInfo outFileInfo)
        {
            switch (compressionMode)
            {
                case CompressionMode.Compress:
                    {
                        return new GZipCompressor(inFileInfo, outFileInfo);
                    }

                case CompressionMode.Decompress:
                    {
                        return new GZipDecompressor(inFileInfo, outFileInfo);
                    }
                default:
                    throw new ArgumentOutOfRangeException("compressionMode");
            }

        }
    }
}
