using Dublin.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dublin.ReaderWriterWorkers
{
    public interface IReaderWriter
    {
        ConcurrentQueue<Block> ReadQueue { get; }

        ConcurrentQueue<Block> WriteQueue { get; }

        void Process(CancellationToken ct);
    }
}
