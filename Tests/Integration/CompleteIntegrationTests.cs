using Dublin;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Integration
{
    [TestClass]
    public class CompleteIntegrationTests
    {
        [TestMethod]
        public void CompleteIntegrationTests_1()
        {
            var inputFile = @"./Integration/TestSample.txt";
            var outputFile = @"./Integration/Compressed";
            var result = @"./Integration/Decompressed";

            try
            {
                var builder = new Builder(inputFile, outputFile, 4, 4, 32);
                var compress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Compress);
                compress.Start(new CancellationToken());
                builder.Dispose();

                builder = new Builder(outputFile, result, 4, 4, 32);
                var decompress = builder.BuildOrchestrator(System.IO.Compression.CompressionMode.Decompress);
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
