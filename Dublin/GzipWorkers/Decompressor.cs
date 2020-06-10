using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Dublin.GzipWorkers
{
    internal class Decompressor : IGzipWorker
    {
        protected ConcurrentQueue<Block> readQueue;
        protected ConcurrentQueue<Block> writeQueue;

        public Decompressor(ConcurrentQueue<Block> readQueue, ConcurrentQueue<Block> writeQueue)
        {
            this.readQueue = readQueue;
            this.writeQueue = writeQueue;
        }

        public void Process(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && readQueue.TryDequeue(out var block))
            {
                writeQueue.Enqueue(Decompress(block));
            }
        }

        public Block Decompress(Block source)
        {
            // todo - initialize with capacity or buffer
            using (var decompressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(decompressedStream, CompressionMode.Decompress))
            {
                zipStream.Write(source.Content, 0, source.Size);
                var result = Block.CreateDecompressedBlock(source, decompressedStream.ToArray());
                return result;
            }
        }
    }
}
