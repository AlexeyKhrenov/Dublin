using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dublin
{
    public class Orchestrator
    {
        private IGzipWorker[] gzipWorkers;
        private IReaderWriter readerWriter;

        public Orchestrator(IGzipWorker[] gzipWorkers, IReaderWriter readerWriter)
        {
            this.readerWriter = readerWriter;
            this.gzipWorkers = gzipWorkers;
        }

        public void Start(CancellationToken ct)
        {
            var gzipThreads = new Thread[gzipWorkers.Length];

            for (var i = 0; i < gzipWorkers.Length; i++)
            {
                var l = i;
                gzipThreads[l] = new Thread(() => gzipWorkers[l].Process(ct));
                gzipThreads[l].Start();
            }

            var writerThread = new Thread(() => readerWriter.Process(ct));
            writerThread.Start();

            foreach (var thread in gzipThreads)
            {
                thread.Join();
            }

            readerWriter.Close();

            writerThread.Join();
        }
    }
}
