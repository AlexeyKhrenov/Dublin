using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using System;
using System.IO;

namespace Dublin
{
    public abstract class AbstractWorker : IDisposable
    {
        public ConcurrentQueue<Block> ReadQueue { get; private set; }

        public ConcurrentQueue<Block> WriteQueue { get; private set; }

        protected Stream input;
        protected Stream output;
        protected int blockSize;

        protected virtual bool CanRead => input.Position != input.Length;

        public AbstractWorker(
            Stream input,
            Stream output,
            int readQueueSize,
            int writeQueueSize,
            int blockSize
        )
        {
            this.input = input;
            this.output = output;

            ReadQueue = new ConcurrentQueue<Block>(readQueueSize);
            WriteQueue = new ConcurrentQueue<Block>(writeQueueSize);

            this.blockSize = blockSize;
        }

        public void Process()
        {
            while (CanRead)
            {
                while (!WriteQueue.IsEmpty && !ReadQueue.IsEmpty)
                {
                    WriteNext();
                }

                if (ReadQueue.IsEmpty)
                {
                    ReadNext();
                }
            }

            ReadQueue.Close();
        }

        public abstract void ReadNext();

        public abstract void WriteNext();

        public virtual void Close()
        {
            while (!WriteQueue.IsEmpty)
            {
                WriteNext();
            }
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }
    }
}
