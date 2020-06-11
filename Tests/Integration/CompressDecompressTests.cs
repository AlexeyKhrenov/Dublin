using Dublin.FileStructure;
using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Integration
{
    [TestClass]
    public class CompressDecompressTests
    {
        [TestMethod]
        public void CompressDecompressTest_1()
        {
            UnicodeEncoding uniEncode = new UnicodeEncoding();
            byte[] bytesToCompress = uniEncode.GetBytes("example text to compress and decompress");

            var block = Block.CreateBlockForCompression(0, bytesToCompress.Length);
            block.Content = bytesToCompress;

            var compressor = CreateCompressor();
            compressor.ReadQueue.Enqueue(block);
            compressor.ReadQueue.Close();
            compressor.Process(new CancellationToken());

            compressor.WriteQueue.TryDequeue(out var compressed);

            var decompressor = CreateDeCompressor();
            decompressor.ReadQueue.Enqueue(compressed);
            decompressor.ReadQueue.Close();
            decompressor.Process(new CancellationToken());

            decompressor.WriteQueue.TryDequeue(out var result);

            result.Content.Should().BeEquivalentTo(block.Content);
            result.Content.Should().NotBeEmpty();
        }

        private Compressor CreateCompressor()
        {
            return new Compressor(
                new ConcurrentQueue<Block>(1),
                new ConcurrentQueue<Block>(1));
        }

        private Decompressor CreateDeCompressor()
        {
            return new Decompressor(
                new ConcurrentQueue<Block>(1),
                new ConcurrentQueue<Block>(1));
        }
    }
}
