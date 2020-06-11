using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using System;
using System.IO;
using System.Threading;

namespace Dublin
{
    public abstract class AbstractWorker : IReaderWriter
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

        public void Process(CancellationToken ct)
        {
            while (CanRead)
            {
                ct.ThrowIfCancellationRequested();

                while (!WriteQueue.IsEmpty && !ReadQueue.IsEmpty)
                {
                    TryWriteNext();
                }

                ReadNext();
            }

            ReadQueue.Close();

            while (TryWriteNext())
            {
            }

            Finish();
        }

        public abstract void ReadNext();

        public abstract bool TryWriteNext();

        public virtual void Finish()
        {
        }

        public void Close()
        {
            WriteQueue.Close();
        }
    }
}
