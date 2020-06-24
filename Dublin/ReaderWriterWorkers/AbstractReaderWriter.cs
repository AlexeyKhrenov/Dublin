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

        public Action<int> ReportPercentage { get; set; }

        protected Stream input;
        protected Stream output;
        protected int blockSize;
        protected int numberOfBlocks;
        protected int readBlocks;
        protected int maxNumberOfBlocks;

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
                    if (TryWriteNext())
                    {
                        ReportNextBlockProgress();
                    }
                }

                ReadNext();
            }

            ReadQueue.Close();

            while (TryWriteNext())
            {
                ReportNextBlockProgress();
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

        protected void ReportNextBlockProgress()
        {
            numberOfBlocks++;
            var percentage = (int)((double)numberOfBlocks / (double)maxNumberOfBlocks * 100);
            ReportPercentage?.Invoke(percentage);
        }
    }
}
