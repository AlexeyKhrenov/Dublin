using Dublin;
using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Tests.Integration
{
    public class DifferentProcessorsCountTests
    {

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        public void DifferentProcessorsCountTest_1(int processorCount)
        {
            var inputFile = @"./Integration/TestSample.txt";
            var outputFile = @"./Integration/Compressed";
            var result = @"./Integration/Decompressed";

            try
            {
                var builder = new Builder(inputFile, outputFile, 4, 4, 32);
                var compress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Compress, processorCount);
                compress.Start(new CancellationToken());
                builder.Dispose();

                builder = new Builder(outputFile, result, 4, 4, 32);
                var decompress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Decompress, processorCount);
                decompress.Start(new CancellationToken());
                builder.Dispose();

                var resultText = File.ReadAllText(result);
                var expectedText = File.ReadAllText(inputFile);

                resultText.Should().Be(expectedText);
            }
            finally
            {
                File.Delete(outputFile);
                File.Delete(result);
            }
        }
    }
}
