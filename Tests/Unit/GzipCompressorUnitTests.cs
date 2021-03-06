﻿using Dublin.FileStructure;
using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using System.Threading;
using Xunit;

namespace Tests.Unit
{
    public class GzipCompressorUnitTests
    {
        [Fact]
        public void GzipCompressorUnitTest_1()
        {
            var compressor = CreateCompressor();
            var source = Block.CreateBlockForCompression(0, 18);
            source.Content = new byte[] { 2, 3, 4, 0, 1, 2, 4, 9, 2, 0, 23, 52, 2, 2, 2, 3, 5, 0 };
            compressor.ReadQueue.Enqueue(source);
            compressor.ReadQueue.Close();

            compressor.Process(new CancellationToken());

            compressor.WriteQueue.TryDequeue(out var target);

            target.Content.Should().NotBeNullOrEmpty();
            target.Metadata.LengthCompressed.Should().Be(target.Content.Length);
        }

        private Compressor CreateCompressor()
        {
            return new Compressor(
                new ConcurrentQueue<Block>(1),
                new ConcurrentQueue<Block>(1));
        }
    }
}
