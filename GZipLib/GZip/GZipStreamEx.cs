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
        private bool _writeOriginalFileName = false;
        private bool _writeCompressedSize = false;
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

        public GZipStreamEx(Stream stream, CompressionMode mode, string originalFileName = null, bool writeCompressedSize = false)
            : base(stream, mode, true)
        {

            OriginalFileName = originalFileName;
            if (!string.IsNullOrEmpty(originalFileName))
            {
                _writeOriginalFileName = true;
            }

            CompressionMode = mode;
            LocalBaseStream = stream;
            _writeCompressedSize = writeCompressedSize;

            if (mode == CompressionMode.Compress && _writeCompressedSize == true && _writeOriginalFileName == true)
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

                if (_writeOriginalFileName)
                {
                    FixFileName();
                }
                if (_writeCompressedSize)
                {
                    FixExtraCompressedSize();
                }
            }

        }

        private void FixExtraCompressedSize()
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

        private void FixFileName()
        {
            if (BaseStream.Length < 10)
            {
                throw new InvalidDataException();
            }

            byte[] buffer = new byte[BaseStream.Length];
            BaseStream.Seek(0, SeekOrigin.Begin);
            BaseStream.Read(buffer, 0, (int)BaseStream.Length);

            List<byte> fixBytes = buffer.ToList();

            if (fixBytes[3] != 0)
            {
                BaseStream.Seek(0, SeekOrigin.End);
                return;
            }

            //fix flag
            fixBytes[3] = (byte)(HeaderFLG.FNAME);

            //get name in Latin-1 encoding with 0 byte at end
            var bytesFileName = StringToGZipBytes(OriginalFileName);

            //insert after header 10 byte
            fixBytes.InsertRange(10, bytesFileName);

            BaseStream.Seek(0, SeekOrigin.Begin);
            BaseStream.Write(fixBytes.ToArray(), 0, fixBytes.Count);
        }

        private byte[] StringToGZipBytes(string input)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            return Encoding.Convert(iso, utf8, iso.GetBytes(input)).Concat(new byte[] { 0 }).ToArray();
        }
        
        public override void Close()
        {
            if (CompressionMode == CompressionMode.Compress)
            {
                if (BaseStream.Length == 0)
                {
                    BaseStream.Write(_zeroFileData, 0, _zeroFileData.Length);

                    if (_writeOriginalFileName)
                    {
                        FixFileName();
                    }
                    if (_writeCompressedSize)
                    {
                        FixExtraCompressedSize();
                    }

                }
            }

            base.Close();


            if (CompressionMode == CompressionMode.Compress && _writeCompressedSize)
            {
                LocalBaseStream.Seek(16, SeekOrigin.Begin);
                var sizeBytes = BitConverter.GetBytes((int)LocalBaseStream.Length);
                LocalBaseStream.Write(sizeBytes, 0, 4);
            }
            LocalBaseStream = null;
        }
        
        public static bool TryGetBlockSize(Stream stream, out int blockSize)
        {            
            blockSize = 0;

            byte[] buffer = new byte[20];
            int readedBytes = stream.Read(buffer, 0, 20);
            stream.Seek(-20, SeekOrigin.Current);
            if (readedBytes == 20)
            {
                if( ((HeaderFLG)buffer[3] & HeaderFLG.FEXTRA) != 0)
                {
                    if(buffer[12] == 0x42 && buffer[13] == 0x53)
                    {
                        blockSize = BitConverter.ToInt32(buffer, 16);
                        return true;
                    }
                }
            }

            return false;

        }
        
    }


}
