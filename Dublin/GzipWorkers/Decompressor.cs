using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Dublin.GzipWorkers
{
    internal class Decompressor : IGzipWorker
    {
        public ConcurrentQueue<Block> ReadQueue { get; }
        public ConcurrentQueue<Block> WriteQueue { get; }

        public Decompressor(ConcurrentQueue<Block> readQueue, ConcurrentQueue<Block> writeQueue)
        {
            ReadQueue = readQueue;
            WriteQueue = writeQueue;
        }

        public void Process(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && ReadQueue.TryDequeue(out var block))
            {
                WriteQueue.Enqueue(Decompress(block));
            }
        }

        public Block Decompress(Block source)
        {
            // todo - initialize with capacity or buffer
            using (var compressedStream = new MemoryStream(source.Content))
            using (var decompressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                zipStream.CopyTo(decompressedStream);
                var result = Block.CreateDecompressedBlock(source, decompressedStream.ToArray());
                return result;
            }
        }

        public byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}
