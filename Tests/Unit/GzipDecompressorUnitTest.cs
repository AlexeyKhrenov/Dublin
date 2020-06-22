using Dublin.FileStructure;
using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using System.Threading;
using Xunit;

namespace Tests.Unit
{
    public class GzipDecompressorUnitTests
    {
        [Fact]
        public void GzipDecompressorUnitTest()
        {
            var decompressor = CreateDeCompressor();
            var source = Block.CreateBlockForDecompression(0, 5, 0, 10);
            source.Content = new byte[] {
                31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 99, 98,
                102, 97, 96, 100, 98, 225, 100, 98,
                16, 55, 97, 98, 98, 98,
                102, 101, 0, 0, 184, 251, 235, 172, 18, 0, 0, 0
            };
            decompressor.ReadQueue.Enqueue(source);
            decompressor.ReadQueue.Close();

            decompressor.Process(new CancellationToken());

            decompressor.WriteQueue.TryDequeue(out var target);

            target.Content.Should().NotBeNullOrEmpty();
            target.Metadata.Length.Should().Be(target.Content.Length);
        }

        private Decompressor CreateDeCompressor()
        {
            return new Decompressor(
                new ConcurrentQueue<Block>(1),
                new ConcurrentQueue<Block>(1));
        }
    }
}
