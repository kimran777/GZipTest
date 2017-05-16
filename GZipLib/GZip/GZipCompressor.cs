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
        private readonly int _numReadThreads;
        Stream _inFileStream;
        Stream _outFileStream;


        int _blockSize = 1024 * 16;

        QueueWithLock<DataBlock>[] _compressedBlocksProd;
        Queue<Exception> _errors = new Queue<Exception>();
        private readonly object _lockPoint = new object();

        public GZipCompressor(FileInfo inFileInfo, FileInfo outFileInfo) : base(inFileInfo, outFileInfo)
        {
            _numReadThreads = GetNumReadThreads();

            _compressedBlocksProd = new QueueWithLock<DataBlock>[_numReadThreads];
            int producerMaxSize = 1024 * 1024 * 100 / _blockSize / _compressedBlocksProd.Length;

            if (producerMaxSize == 0)
            {
                producerMaxSize = 1;
            }

            for (int i = 0; i < _compressedBlocksProd.Length; i++)
            {

                _compressedBlocksProd[i] = 
                    new QueueWithLock<DataBlock>(producerMaxSize);
            }



        }

        public override void Start()
        {
            PrepareInFileStream();
            PrepareOutFileStream();

            var writeThread = ThreadManager.GetSafeThread(WriteBlocks, ExceptionHandler);
            writeThread.StartWithPriority(ThreadPriority.BelowNormal);

            var compressThreads =
                ThreadManager.GetSafeThreads(_numReadThreads, CompressBlocks, ExceptionHandler);


            compressThreads.StartThreads(ThreadPriority.AboveNormal);

            compressThreads.WaitThreads().ContinueWithOneTime(() =>
            {
                foreach(var p in _compressedBlocksProd)
                {
                    p.Stop();
                }
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
            //Random rnd = new Random(Thread.CurrentThread.ManagedThreadId);
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
                    using (var gzStream = new GZipStreamEx(mem, CompressionMode.Compress))
                    {
                        gzStream.Write(buffer, 0, readedData);
                    }

                    //Console.WriteLine("Thread #{0} write block #{1, -10} to queue #{2}", Thread.CurrentThread.ManagedThreadId, blockId, blockId % _numReadThreads);
                    //Console.WriteLine("Producer #{0} has {1, 10} elements", blockId % _numReadThreads, _compressedBlocksProd[blockId % _numReadThreads].Count());
                    //Thread.Sleep(rnd.Next(500, 1500));
                    _compressedBlocksProd[blockId % _numReadThreads].Enqueue(new DataBlock(mem.ToArray(), (int)mem.Length, blockId));

                }


            }


        }
        

        private void WriteBlocks()
        {
            while (true)
            {
                for(int i = 0; i < _compressedBlocksProd.Length; i++)
                {
                    DataBlock bl = _compressedBlocksProd[i].Dequeue();

                    //aborted or end 
                    if (bl == null)
                    {
                        return;
                    }

                    _outFileStream.Write(bl.Data, 0, bl.DataSize);
                }
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

        private int GetNumReadThreads()
        {
            if (Environment.ProcessorCount > 1)
            {
                return Environment.ProcessorCount - 1;
            }
            else
            {
                return 1;
            }
        }

        private void ExceptionHandler(Exception exception)
        {
            lock (_errors)
            {
                _errors.Enqueue(exception);
            }
            foreach(var p in _compressedBlocksProd)
            {
                p.Abort();
            }
            Thread.CurrentThread.Abort();
        }


    }
}
