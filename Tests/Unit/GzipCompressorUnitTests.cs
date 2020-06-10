using Dublin.FileStructure;
using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Tests.Unit
{
    [TestClass]
    public class GzipCompressorUnitTests
    {
        [TestMethod]
        public void GzipCompressorUnitTest_1()
        {
            var compressor = CreateCompressor();
            var source = Block.CreateBlockForCompression(0, 10);
            source.Content = new byte[] { 2, 3, 4, 0, 1, 2, 4, 9, 2, 0 };
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
