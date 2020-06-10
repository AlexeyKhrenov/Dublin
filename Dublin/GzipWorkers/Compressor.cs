using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dublin.GzipWorkers
{
    internal class Compressor : IGzipWorker
    {
        public ConcurrentQueue<Block> ReadQueue { get; }
        public ConcurrentQueue<Block> WriteQueue { get; }

        public Compressor(ConcurrentQueue<Block> readQueue, ConcurrentQueue<Block> writeQueue)
        {
            ReadQueue = readQueue;
            WriteQueue = writeQueue;
        }

        public void Process(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && ReadQueue.TryDequeue(out var block))
            {
                WriteQueue.Enqueue(Compress(block));
            }
        }

        public Block Compress(Block source)
        {
            // todo - initialize with capacity or buffer
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(source.Content, 0, source.Size);
                var result = Block.CreateCompressedBlock(source, compressedStream.ToArray());
                WriteQueue.Enqueue(result);
                return result;
            }
        }
    }
}
