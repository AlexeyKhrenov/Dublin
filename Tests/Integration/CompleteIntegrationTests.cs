﻿using Dublin;
using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Tests.Integration
{
    public class CompleteIntegrationTests
    {
        [Fact]
        public void CompleteIntegrationTests_1()
        {
            var inputFile = @"./Integration/TestSample.txt";
            var outputFile = @"./Integration/Compressed";
            var result = @"./Integration/Decompressed";

            try
            {
                var builder = new Builder(inputFile, outputFile, 4, 4, 32, null);
                var compress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Compress, Environment.ProcessorCount);
                compress.Start(new CancellationToken());
                builder.Dispose();

                builder = new Builder(outputFile, result, 4, 4, 32, null);
                var decompress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Decompress, Environment.ProcessorCount);
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
