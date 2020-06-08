using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dublin
{
    public class Compressor : IDisposable
    {
        private IConcurrentReaderWriter readerWriter;
        private GZipStream gzipStream;
        private int blockSize;
        private MemoryStream buffer;

        public Compressor(IConcurrentReaderWriter readerWriter, int blockSize, CompressionLevel compressionLevel)
        {
            this.readerWriter = readerWriter;
            this.blockSize = blockSize;
            buffer = new MemoryStream();

            gzipStream = new GZipStream(buffer, compressionLevel);
        }

        public void Run(CancellationToken cancellationToken)
        {
            while (readerWriter.CanRead)
            {
                cancellationToken.ThrowIfCancellationRequested();
                readerWriter.ReadNext(gzipStream, blockSize);
                readerWriter.Write(buffer, position);
            }
        }

        public void Dispose()
        {
            gzipStream.Dispose();
        }
    }
}
