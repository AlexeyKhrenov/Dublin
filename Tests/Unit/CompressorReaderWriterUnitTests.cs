using Dublin.FileStructure;
using Dublin.ReaderWriterWorkers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit
{
    [TestClass]
    public class CompressorReaderWriterUnitTests
    {
        [TestMethod]
        public void CompressorReaderWriterUnitTest_1()
        {
            using (var ms = new MemoryStream(new byte[]{ 20, 10, 3, 5, 15, 15, 32, 64, 8 }))
            {
                var sut = new CompressorReaderWriter(ms, null, 2, 0, 8);
                sut.ReadNext();
                sut.ReadNext();

                sut.ReadQueue.TryDequeue(out var block1);
                block1.Content.Should().BeEquivalentTo(new byte[] { 20, 10, 3, 5, 15, 15, 32, 64 });
                block1.Metadata.Should().BeEquivalentTo(new MetadataRecord(0, 8, 0, 0));

                sut.ReadQueue.TryDequeue(out var block2);
                block2.Content.Should().BeEquivalentTo(new byte[] { 8 });
                block2.Metadata.Should().BeEquivalentTo(new MetadataRecord(8, 1, 0, 0));
            }
        }

        [TestMethod]
        public void CompressorReaderWriterUnitTest_2()
        {
            using (var ms = new MemoryStream(new byte[] { 20, 10, 3, 5, 15, 15, 32, 64, 8 }))
            using (var output = new MemoryStream())
            {
                var sut = new CompressorReaderWriter(ms, output, 2, 2, 8);
                sut.ReadNext();
                sut.ReadNext();

                sut.ReadQueue.TryDequeue(out var block1);
                sut.ReadQueue.TryDequeue(out var block2);

                // reverse order
                sut.WriteQueue.Enqueue(Block.CreateCompressedBlock(block2, block2.Content));
                sut.WriteQueue.Enqueue(Block.CreateCompressedBlock(block1, block1.Content));

                sut.Close();
                sut.Process(new System.Threading.CancellationToken());

                var result = output.ToArray();
                var expected = new byte[]
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

                result.Should().BeEquivalentTo(expected);
            }
        }
    }
}
