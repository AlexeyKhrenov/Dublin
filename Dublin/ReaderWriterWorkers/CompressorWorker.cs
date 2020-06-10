using Dublin.FileStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin.ReaderWriterWorkers
{
    public sealed class CompressorWorker : AbstractWorker
    {
        private FileMetadata fileMetadata;

        public CompressorWorker(
            Stream input,
            Stream output,
            int readQueueSize,
            int writeQueueSize,
            int blockSize) : base(input, output, readQueueSize, writeQueueSize, blockSize)
        {
            fileMetadata = new FileMetadata();
        }

        public override void ReadNext()
        {
            var start = input.Position;

            var left = input.Length - blockSize;

            var block = Block.CreateBlockForCompression(start, blockSize < left ? (int)left : blockSize);
            input.Read(block.Content, 0, blockSize);

            ReadQueue.Enqueue(block);
        }

        public override void WriteNext()
        {
            if (WriteQueue.TryDequeue(out var block))
            {
                if (block.Content.Length == 0 || block.Metadata.LengthCompressed == 0)
                {
                    throw new ArgumentException("Invalid block format");
                }

                block.AddCompressedPositionStart(output.Position);

                output.Write(block.Content, 0, block.Size);
                fileMetadata.AddRecord(block.Metadata);
            }
        }

        public override void Close()
        {
            base.Close();
            fileMetadata.Write(output);
        }
    }
}
