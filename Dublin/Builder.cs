using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using System;
using System.IO;
using System.IO.Compression;

namespace Dublin
{
    public class Builder : IDisposable
    {
        private Stream inputStream;
        private Stream outputStream;
        private string inputFile;
        private string outputFile;
        private int readQueueSize = 0;
        private int writeQueueSize = 0;
        private int blockSize = 0;

        public Builder(string inputFile, string outputFile, int readQueueSize, int writeQueueSize, int blockSize)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException("File not found: " + inputFile);
            }

            if (File.Exists(outputFile))
            {
                throw new InvalidOperationException("File already exists: " + outputFile);
            }

            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.readQueueSize = readQueueSize;
            this.writeQueueSize = writeQueueSize;
            this.blockSize = blockSize;

            inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
        }

        public Orchestrator BuildOrchestrator(CompressionMode compressionMode, int processorCount)
        {
            IReaderWriter readerWriter;
            IGzipWorker[] workers = new IGzipWorker[Environment.ProcessorCount];

            switch (compressionMode)
            {
                case CompressionMode.Compress:
                    readerWriter = new CompressorReaderWriter(inputStream, outputStream, readQueueSize, writeQueueSize, blockSize);
                    for (var i = 0; i < workers.Length; i++)
                    {
                        workers[i] = new Compressor(readerWriter.ReadQueue, readerWriter.WriteQueue);
                    }
                    break;
                case CompressionMode.Decompress:
                    readerWriter = new DecompressorReaderWriter(inputStream, outputStream, readQueueSize, writeQueueSize, blockSize);
                    for (var i = 0; i < workers.Length; i++)
                    {
                        workers[i] = new Decompressor(readerWriter.ReadQueue, readerWriter.WriteQueue);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown compression mode");
            }

            return new Orchestrator(workers, readerWriter);
        }

        public void Clearup()
        {
            File.Delete(outputFile);
        }

        public void Dispose()
        {
            inputStream.Dispose();
            outputStream.Dispose();
        }
    }
}
