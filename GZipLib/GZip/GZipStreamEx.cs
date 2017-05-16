using GZipLib.GZip.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{
    public class GZipStreamEx : GZipStream
    {
        static byte[] _zeroFileData = new byte[]
        {
            0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
            0x01, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00
        };

        private bool _isFirstWrite = true;
        private const int _posOfCompressedSize = 16;

        
        public string OriginalFileName
        {
            get;
            private set;
        }

        public CompressionMode CompressionMode
        {
            get;
            private set;
        }

        public Stream LocalBaseStream { get; private set; }

        public GZipStreamEx(Stream stream, CompressionMode mode)
            : base(stream, mode, true)
        {
            CompressionMode = mode;
            LocalBaseStream = stream;

            if (CompressionMode == CompressionMode.Compress)
            {
                if (!stream.CanRead || !stream.CanWrite)
                {
                    throw new NotSupportedException("Поток не поддерживает чтение.");
                }
            }

        }

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);

            if (_isFirstWrite && count > 0)
            {
                _isFirstWrite = false;

                AddExtraCompressedSize();
            }

        }
        

        private void AddExtraCompressedSize()
        {
            if (BaseStream.Length < 10)
            {
                throw new InvalidDataException();
            }
            
            byte[] buffer = new byte[BaseStream.Length];

            BaseStream.Seek(0, SeekOrigin.Begin);
            BaseStream.Read(buffer, 0, (int)BaseStream.Length);

            List<byte> fixBytes = buffer.ToList();

            //fix flag
            fixBytes[3] = (byte)(fixBytes[3] | (byte)HeaderFLG.FEXTRA);

            //add extra field with compressed size
            //2 byte length of extra
            //2 byte SI1 and SI1
            //2 byte length of sub extra
            //n byte of extra
            var extraFieldBytes = new byte[]
            {
                0x08, 0x00, 0x42, 0x53, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            fixBytes.InsertRange(10, extraFieldBytes);

            BaseStream.Seek(0, SeekOrigin.Begin);
            BaseStream.Write(fixBytes.ToArray(), 0, fixBytes.Count);

        }
        
        public override void Close()
        {
            //fix zero length input with compress 
            if (CompressionMode == CompressionMode.Compress)
            {
                if (BaseStream.Length == 0)
                {
                    BaseStream.Write(_zeroFileData, 0, _zeroFileData.Length);
                    AddExtraCompressedSize();
                }
            }

            base.Close();


            if (CompressionMode == CompressionMode.Compress)
            {
                //16 - position to write extra field Block Size
                LocalBaseStream.Seek(16, SeekOrigin.Begin);
                var sizeBytes = BitConverter.GetBytes((int)LocalBaseStream.Length);
                LocalBaseStream.Write(sizeBytes, 0, 4);
            }

            LocalBaseStream = null;
        }
        

        public static bool TryGetBlockSize(Stream stream, out int blockSize)
        {
            blockSize = 0;
            var extraFields = new List<ExtraField>();
            try
            {
                extraFields = ExtraFieldParser.GetExtraFields(stream);
            }
            catch
            {
                return false;
            };

            foreach (var ef in extraFields)
            {
                if (ef.SubID1 == 0x42 && ef.SubID2 == 0x53)
                {
                    blockSize = BitConverter.ToInt32(ef.Data, 0);
                    return true;
                }
            }
            return false;

        }
        
    }


}
