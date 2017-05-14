using GZipLib.GZip.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Threading;

namespace GZipLib.GZip
{
    public class GZipCompressor : Compressor
    {
        long _numOfBlocks = 0;
        Stream _inFileStream;
        Stream _outFileStream;
        DictionaryWithLock<DataBlock> _compressedBlocksProd;
        Queue<Exception> _errors = new Queue<Exception>();
        private readonly object _lockPoint = new object();
        
        public GZipCompressor(FileInfo inFileInfo, FileInfo outFileInfo) : base(inFileInfo, outFileInfo)
        {
            _numOfBlocks = CalcNumOfBlocks(InFileInfo.Length);
            _compressedBlocksProd = new DictionaryWithLock<DataBlock>(1024 * 1024 * 100 / _blockSize);
        }

        public override void Start()
        {
            PrepareInFileStream();
            PrepareOutFileStream();

            var writeThread = ThreadManager.GetSafeThread(WriteBlocks, ExceptionHandler);
            writeThread.Start();

            var compressThreads = 
                ThreadManager.GetSafeThreads(Environment.ProcessorCount, CompressBlocks, ExceptionHandler);
            

            compressThreads.StartThreads();

            compressThreads.WaitThreads().ContinueWithOneTime(() =>
            {
                _compressedBlocksProd.Stop();
            });

            writeThread.Join();

            _inFileStream.Close();            
            _outFileStream.Close();

            if (_errors.Any())
            {
                OutFileInfo.Delete();
                throw _errors.Dequeue();
            }
        }

        int blockIter = 0;
        private void CompressBlocks()
        {
            byte[] buffer = new byte[_blockSize];
            int readedData = 0;
            int blockId = 0;
            string originalFileName = InFileInfo.Name;
            while (true)
            {
                lock (_lockPoint)
                {
                    readedData = _inFileStream.Read(buffer, 0, buffer.Length);
                    if (readedData == 0 && blockIter != 0)
                    {
                        break;
                    }
                    blockId = blockIter;
                    blockIter++;
                }


                using (var mem = new MemoryStream())
                {
                    using (var gzStream = new GZipStreamEx(mem, CompressionMode.Compress, originalFileName, true))
                    {
                        gzStream.Write(buffer, 0, readedData);
                    }


                    _compressedBlocksProd.Add(blockId, new DataBlock(mem.ToArray(), (int)mem.Length, blockId));
                }

                //GC.Collect();
            }


        }



        private void WriteBlocks()
        {
            int i = 0;
            while (true)
            {
                DataBlock bl = _compressedBlocksProd.GetValue(i);

                //aborted or end 
                if (bl == null)
                {
                    break;
                }

                _outFileStream.Write(bl.Data, 0, bl.DataSize);
                i++;
            }
        }

        private void PrepareOutFileStream()
        {
            if (OutFileInfo.Extension.ToLower() != ".gz")
            {
                throw new FileExtensionException("Результирующий файл должен быть формата .gz");
            }

            _outFileStream = OutFileInfo.Open(FileMode.Create, FileAccess.Write);

        }

        private void PrepareInFileStream()
        {
            if (InFileInfo.Extension.ToLower() == ".gz")
            {
                throw new FileExtensionException("Исходный файл не может быть формата .gz");
            }

            _inFileStream = InFileInfo.OpenRead();

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

        private void ExceptionHandler(Exception exception)
        {
            _compressedBlocksProd.Abort();
            lock (_errors)
            {
                _errors.Enqueue(exception);
            }
            Thread.CurrentThread.Abort();
        }
        

    }

}
