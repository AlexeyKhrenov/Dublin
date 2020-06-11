using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit
{
    [TestClass]
    public class DecompressorReaderWriterUnitTests
    {
        [TestMethod]
        public void DecompressorReaderWriterUnitTest_1()
        {
            var input = new byte[]
            {
                // data
                8, 20, 10, 3, 5, 15, 15, 32, 64,

                // last metadata record
                8, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0,

                // first metadata record
                0, 0, 0, 0, 0, 0, 0, 0,
                8, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0,
                8, 0, 0, 0, 0, 0, 0, 0,

                // number of metadata records
                2, 0, 0, 0
            };

            using (var inputMs = new MemoryStream(input))
            {
                var decompress = new DecompressorReaderWriter(inputMs, null, 2, 2, 8);
                decompress.ReadNext();
                decompress.ReadNext();

                decompress.ReadQueue.TryDequeue(out var block1);
                decompress.ReadQueue.TryDequeue(out var block2);

                block1.Content.Should().BeEquivalentTo(new byte[] { 20, 10, 3, 5, 15, 15, 32, 64 });
                block1.Metadata.Should().BeEquivalentTo(new MetadataRecord(0, 8, 1, 8 ));

                block2.Content.Should().BeEquivalentTo(new byte[] { 8 });
                block2.Metadata.Should().BeEquivalentTo(new MetadataRecord(8, 1, 0, 1 ));
            }
        }

        [TestMethod]
        public void DecompressorReaderWriterUnitTest_2()
        {
            var block1 = Block.CreateBlockForDecompression(8, 1, 0, 1);
            block1.Content = new byte[] { 8 };

            var block2 = Block.CreateBlockForDecompression(0, 8, 1, 8);
            block2.Content = new byte[] { 20, 10, 3, 5, 15, 15, 32, 64 };

            using (var inputMs = new MemoryStream())
            using (var outputMs = new MemoryStream())
            {
                var decompress = new DecompressorReaderWriter(inputMs, outputMs, 2, 2, 8);

                decompress.WriteQueue.Enqueue(block1);
                decompress.WriteQueue.Enqueue(block2);

                decompress.Close();
                decompress.Process(new System.Threading.CancellationToken());

                var result = outputMs.ToArray();

                var expected = new byte[] { 20, 10, 3, 5, 15, 15, 32, 64, 8 };

                result.Should().BeEquivalentTo(expected);
            }
        }
    }
}
