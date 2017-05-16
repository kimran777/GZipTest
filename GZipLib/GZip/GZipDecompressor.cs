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
    public class GZipDecompressor : Compressor
    {
        private readonly int _numReadThreads = 0;
        Stream _inFileStream;
        Stream _outFileStream;
        private readonly object _lockPoint = new object();
        QueueWithLock<DataBlock>[] _decompressedBlocksProd;
        Queue<Exception> _errors = new Queue<Exception>();


        public GZipDecompressor(FileInfo inFileInfo, FileInfo outFileInfo) : base(inFileInfo, outFileInfo)
        {
            _numReadThreads = GetNumReadThreads();
            _decompressedBlocksProd = new QueueWithLock<DataBlock>[_numReadThreads];
            for(int i = 0; i < _decompressedBlocksProd.Length; i++)
            {
                _decompressedBlocksProd[i] = new QueueWithLock<DataBlock>(100);
            }
        }

        public override void Start()
        {

            PrepareInFileStream();
            PrepareOutFileStream();

            var writeThread = ThreadManager.GetSafeThread(WriteBlocks, ExceptionHandler);
            writeThread.StartWithPriority(ThreadPriority.BelowNormal);

            var decompressThreads = ThreadManager.GetSafeThreads(Environment.ProcessorCount, DecompressBlocks, ExceptionHandler);
            decompressThreads.StartThreads(ThreadPriority.AboveNormal);


            decompressThreads.WaitThreads().ContinueWithOneTime(() =>
            {
                foreach(var prod in _decompressedBlocksProd)
                {
                    prod.Stop();
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

        private void DecompressBlocks()
        {

            int readedData = 0;
            int blockId = 0;
            string origFileName = InFileInfo.Name;
            byte[] buffer = null;
            int size = 0;



            while (true)
            {
                lock (_lockPoint)
                {
                    if (_inFileStream.Length == _inFileStream.Position)
                    {
                        break;
                    }

                    int blockSize = 0;


                    if (GZipStreamEx.TryGetBlockSize(_inFileStream, out blockSize) == false)
                    {
                        throw new InvalidDataException("архив повреждён");
                    }

                    buffer = new byte[blockSize];


                    readedData = _inFileStream.Read(buffer, 0, buffer.Length);

                    blockId = blockIter;
                    blockIter++;
                    
                }

                size = BitConverter.ToInt32(buffer, buffer.Length - 4);

                using (MemoryStream output = new MemoryStream())
                {
                    using (var input = new MemoryStream(buffer))
                    {
                        using (var gzStream = new GZipStreamEx(input, CompressionMode.Decompress))
                        {

                            byte[] localBuffer = new byte[size];
                            int decompCountBytes = 0;

                            do
                            {
                                decompCountBytes = gzStream.Read(localBuffer, 0, size);
                                output.Write(localBuffer, 0, decompCountBytes);
                            }
                            while (decompCountBytes != 0);

                            _decompressedBlocksProd[blockId % _numReadThreads].Enqueue(new DataBlock(output.ToArray(), (int)output.Length, blockId));

                        }
                    }
                }

            }


        }
        

        private void WriteBlocks()
        {

            while (true)
            {
                for(int i = 0; i < _decompressedBlocksProd.Length; i++)
                {
                    DataBlock bl = _decompressedBlocksProd[i].Dequeue();
                    //aborted or end 
                    if (bl == null)
                    {
                        return;
                    }

                    _outFileStream.Write(bl.Data, 0, bl.DataSize);
                }


            }
        }


        private void PrepareInFileStream()
        {
            if (InFileInfo.Extension.ToLower() != ".gz")
            {
                throw new FileExtensionException("Исходный файл должен быть формата .gz");
            }

            _inFileStream = InFileInfo.OpenRead();

        }

        private void PrepareOutFileStream()
        {
            if (OutFileInfo.Extension.ToLower() == ".gz")
            {
                throw new FileExtensionException("Результирующий файл не может быть формата .gz");
            }

            _outFileStream = OutFileInfo.Open(FileMode.Create, FileAccess.Write);

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
            for (int i = 0; i < _decompressedBlocksProd.Length; i++)
            {
                _decompressedBlocksProd[i].Abort();
            }
            //lock (_errors)
            //{
            //    _errors.Enqueue(exception);
            //}
            Thread.CurrentThread.Abort();
        }
    }
}
