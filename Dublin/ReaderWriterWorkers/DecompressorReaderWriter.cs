using Dublin.FileStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin.ReaderWriterWorkers
{
    public sealed class DecompressorReaderWriter : AbstractWorker
    {
        private FileMetadata fileMetadata;

        private bool _canRead;
        protected override bool CanRead => _canRead;
        
        public DecompressorReaderWriter(
            Stream input,
            Stream output,
            int readQueueSize,
            int writeQueueSize,
            int blockSize) : base(input, output, readQueueSize, writeQueueSize, blockSize)
        {
            fileMetadata = new FileMetadata();
            fileMetadata.Read(input);
            _canRead = true;
            maxNumberOfBlocks = fileMetadata.Records.Count;
        }

        public override void ReadNext()
        {
            if (fileMetadata.TryGetNextRecord(out var record))
            {
                var block = Block.CreateBlockForDecompression(record.Start, record.Length, record.StartCompressed, record.LengthCompressed);
                
                input.Position = record.StartCompressed;
                input.Read(block.Content, 0, block.Size);
                ReadQueue.Enqueue(block);
            }
            else
            {
                _canRead = false;
            }
        }

        public override bool TryWriteNext()
        {
            if (WriteQueue.TryDequeue(out var block))
            {
                if (block.Content.Length == 0 || block.Metadata.Length == 0)
                {
                    throw new ArgumentException("Invalid block format");
                }

                output.Position = block.Metadata.Start;
                output.Write(block.Content, 0, block.Size);
                
                return true;
            }

            return false;
        }
    }
}
