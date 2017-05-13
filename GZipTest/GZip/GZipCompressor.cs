using GZipTest.File;
using GZipTest.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest.GZip 
{
    class GZipCompressor : Comressor, IDisposable
    {
        int _blockSize = 1024 * 1024;
        long _numOfBlocks = 0;
        Stream _inFileStream;
        Stream _outFileStream;
        DictionaryWithLock<DataBlock> _compressedBlocksProd;


        public GZipCompressor(FileInfo inFileInfo, FileInfo outFileInfo) : base(inFileInfo, outFileInfo)
        {
            _numOfBlocks = CalcNumOfBlocks(InFileInfo.Length);
            _compressedBlocksProd = new DictionaryWithLock<DataBlock>(1024 * 1024 * 100 / _blockSize);
        }

        public override void Start()
        {
            //TODO: debug
            _inFileStream = InFileInfo.OpenRead();
            PrepareOutFileStream();

            var writeThread = CreateWriter();
            writeThread.Start();

            var compressTreads = ThreadManager.GetThreads(Environment.ProcessorCount, CompressBlocks);
            ThreadManager.StartThreads(compressTreads);
            ThreadManager.WaitThreads(compressTreads);

            writeThread.Join();

        }


        int blockIter = 0;
        private readonly object _lockPoint = new object();
        private void CompressBlocks()
        {
            byte[] buffer = new byte[_blockSize];
            int readedData = 0;
            int blockId = 0;
            string fileName = OutFileInfo.Name;

            while (true)
            {                
                lock (_lockPoint)
                {
                    readedData = _inFileStream.Read(buffer, 0, buffer.Length);
                    if(readedData == 0 && blockIter != 0)
                    {
                        return;
                    }
                    blockId = blockIter;
                    blockIter++;
                }

                using (var mem = new MemoryStream())
                {

                    using (var gzStream = new GZipLib.GZipStreamEx(mem, CompressionMode.Compress, fileName, true))
                    {
                        gzStream.Write(buffer, 0, readedData);
                    }

                    _compressedBlocksProd.Add(blockId, new DataBlock(mem.ToArray(), (int)mem.Length, blockId));

                }
                

                GC.Collect();


            }
        }

        private Thread CreateWriter()
        {
            Thread thread = new Thread(WriteBlocks);

            return thread;
        }        
                
        private void WriteBlocks()
        {
            int i = 0;
            while(true)
            {
                DataBlock bl = _compressedBlocksProd.GetValue(i);

                if (bl == null)
                {
                    break;
                }

                _outFileStream.Write(bl.Data, 0, bl.DataSize);

                GC.Collect();


            }
        }

        private void PrepareOutFileStream()
        {
            if(OutFileInfo.Extension.ToLower() != ".gz")
            {
                OutFileInfo = new FileInfo(OutFileInfo.FullName + ".gz");
            }

            _outFileStream = FileHelper.CreateFileToWrite(OutFileInfo);
                        
        }

        private long CalcNumOfBlocks(long length)
        {
            if (length == 0)
            {
                return 1;
            }

            if (length % _blockSize == 0)
            {
                return length / _blockSize;
            }
            else
            {
                return length / _blockSize + 1;
            }

        }


        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _inFileStream.Close();
                }
                
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
