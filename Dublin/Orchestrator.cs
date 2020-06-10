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
            throw new NotImplementedException();
        }
    }
}
