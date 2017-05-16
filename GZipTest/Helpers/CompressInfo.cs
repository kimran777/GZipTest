using System.IO;
using System.IO.Compression;

namespace GZipTest.Helpers
{
    class CompressInfo
    {
        public CompressInfo(CompressionMode compressMode, FileInfo inFileName, FileInfo outFileName)
        {
            CompressMode = compressMode;
            InFileName = inFileName;
            OutFileName = outFileName;
        }

        public CompressionMode CompressMode
        {
            get;
            private set;
        }

        public FileInfo InFileName
        {
            get;
            private set;
        }

        public FileInfo OutFileName
        {
            get;
            private set;
        }
    }

}