using BenchmarkDotNet.Attributes;
using Dublin;
using System;
using System.IO;
using System.Threading;

namespace Benchmarking
{
    public class CompressDecompressBenchmarking
    {
        [Params(128)]
        public int ReadQueueSize { get; set; }

        [Params(128)]
        public int WriteQueueSize { get; set; }

        [Params(1024, 4 * 1024, 64 * 1024, 256 * 1024, 1024 * 1024)]
        public int BlockSize { get; set; }

        [Benchmark]
        public void Run()
        {
            var inputFile = @"SampleText.txt";
            var outputFile = @"SampleTextCompressed.txt";
            var result = @"SampleTextDecompressed.txt";

            try
            {
                var builder = new Builder(inputFile, outputFile, ReadQueueSize, WriteQueueSize, BlockSize);
                var compress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Compress);
                compress.Start(new CancellationToken());
                builder.Dispose();

                builder = new Builder(outputFile, result, ReadQueueSize, WriteQueueSize, BlockSize);
                var decompress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Decompress);
                decompress.Start(new CancellationToken());
                builder.Dispose();

                var resultText = File.ReadAllText(result);
                var expectedText = File.ReadAllText(inputFile);

                if (resultText != expectedText)
                {
                    throw new InvalidProgramException("Input and result are different");
                }
            }
            finally
            {
                File.Delete(outputFile);
                File.Delete(result);
            }
        }
    }
}
