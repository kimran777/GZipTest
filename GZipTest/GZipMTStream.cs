using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GZipTest
{
    class GZipMTStream : Stream
    {
        private DeflateStream _deflate;
        
        public GZipMTStream(FileInfo _inputFile, FileInfo _outputFile)
        {

        }



        public override void Write(byte[] array, int offset, int count)
        {
            //base.Write(array, offset, count);
        }

        public override int Read(byte[] array, int offset, int count)
        {
            return _deflate.Read(array, offset, count);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
