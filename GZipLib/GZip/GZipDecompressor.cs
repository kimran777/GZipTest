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
        Stream _inFileStream;
        Stream _outFileStream;
        private readonly object _lockPoint = new object();
        DictionaryWithLock<DataBlock> _decompressedBlocksProd;
        Queue<Exception> _errors = new Queue<Exception>();


        public GZipDecompressor(FileInfo inFileInfo, FileInfo outFileInfo) : base(inFileInfo, outFileInfo)
        {
            _decompressedBlocksProd = new DictionaryWithLock<DataBlock>(50);
        }

        public override void Start()
        {

            PrepareInFileStream();
            PrepareOutFileStream();

            var writeThread = ThreadManager.GetSafeThread(WriteBlocks, ExceptionHandler);
            writeThread.Start();

            var decompressThreads = ThreadManager.GetSafeThreads(Environment.ProcessorCount, DecompressBlocks, ExceptionHandler);
            decompressThreads.StartThreads();


            decompressThreads.WaitThreads().ContinueWithOneTime(() =>
            {
                _decompressedBlocksProd.Stop();
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



                using (var input = new MemoryStream(buffer))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        using (var gzStream = new GZipStreamEx(input, CompressionMode.Decompress))
                        {
                            byte[] localBuffer = new byte[4096];
                            int decompCountBytes = 0;
                            do
                            {
                                decompCountBytes = gzStream.Read(localBuffer, 0, 4096);
                                output.Write(localBuffer, 0, decompCountBytes);
                            }
                            while (decompCountBytes != 0);
                        }
                        _decompressedBlocksProd.Add(blockId, new DataBlock(output.ToArray(), (int)output.Length, blockId));
                    }
                }

                //GC.Collect();
            }


        }
        

        private void WriteBlocks()
        {
            int i = 0;
            while (true)
            {
                DataBlock bl = _decompressedBlocksProd.GetValue(i);

                //aborted or end 
                if (bl == null)
                {
                    break;
                }

                _outFileStream.Write(bl.Data, 0, bl.DataSize);
                i++;
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
        private void ExceptionHandler(Exception exception)
        {
            _decompressedBlocksProd.Abort();
            lock (_errors)
            {
                _errors.Enqueue(exception);
            }
            Thread.CurrentThread.Abort();
        }
    }
}
