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
    public sealed class CompressorReaderWriter : AbstractWorker
    {
        public FileMetadata FileMetadata { get; }

        public CompressorReaderWriter(
            Stream input,
            Stream output,
            int readQueueSize,
            int writeQueueSize,
            int blockSize) : base(input, output, readQueueSize, writeQueueSize, blockSize)
        {
            FileMetadata = new FileMetadata();
        }

        public override void ReadNext()
        {
            var start = input.Position;

            var left = input.Length - input.Position;
            var nextBlockSize = blockSize < left ? blockSize : (int)left;

            var block = Block.CreateBlockForCompression(start, nextBlockSize);
            input.Read(block.Content, 0, nextBlockSize);

            ReadQueue.Enqueue(block);
        }

        public override bool TryWriteNext()
        {
            if (WriteQueue.TryDequeue(out var block))
            {
                if (block.Content.Length == 0 || block.Metadata.LengthCompressed == 0)
                {
                    throw new ArgumentException("Invalid block format");
                }

                block.AddCompressedPositionStart(output.Position);

                output.Write(block.Content, 0, block.Size);
                FileMetadata.AddRecord(block.Metadata);
                return true;
            }

            return false;
        }

        public override void Finish()
        {
            base.Finish();
            FileMetadata.Write(output);
        }
    }
}
