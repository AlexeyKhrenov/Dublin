using Dublin;
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
    public class MetadataUnitTests
    {
        [TestMethod]
        public void MetadataUnitTest_1()
        {
            var metadata = new Metadata();
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 2, 3 }, 0, 2);

                metadata.AddRecord(new MetadataRecord(3, 5, 10, 0));

                metadata.Write(ms);

                var result = ms.ToArray();
                var expected = new byte[]
                    {
                        2, 3, // initial bytes not changed
                        3, 0, 0, 0, 0, 0, 0, 0,
                        5, 0, 0, 0, 0, 0, 0, 0,
                        10, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        1, 0, 0, 0 // number of records at the end
                    };

                result.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void MetadataUnitTest_2()
        {
            var sourceBytes = new byte[]
            {
                2, 3, // initial bytes not changed
                3, 0, 0, 0, 0, 0, 0, 0,
                5, 0, 0, 0, 0, 0, 0, 0,
                10, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0 // number of records at the end
            };

            var metadata = new Metadata();

            using (var ms = new MemoryStream())
            {
                ms.Write(sourceBytes, 0, sourceBytes.Length);
                metadata.Read(ms);
            }

            var expectedMetadata = new Metadata();
            expectedMetadata.AddRecord(new MetadataRecord(3, 5, 10, 0));

            metadata.Should().BeEquivalentTo(expectedMetadata);
        }

        [TestMethod]
        public void MetadataUnitTest_3()
        {
            var source = new Metadata();
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 2, 3 }, 0, 2);

                var sourceRecords = new MetadataRecord[3]
                {
                    new MetadataRecord(3, 5, 10, 0),
                    new MetadataRecord(5, 24252525, 23425, 58581758),
                    new MetadataRecord(long.MaxValue, int.MaxValue, long.MaxValue, int.MaxValue),
                };

                foreach (var record in sourceRecords)
                {
                    source.AddRecord(record);
                }

                source.Write(ms);

                var target = new Metadata();
                target.Read(ms);

                var targetRecords = target.Records.ToArray();

                sourceRecords.Should().BeEquivalentTo(targetRecords);
            }
        }

        [TestMethod]
        public void MetadataUnitTest_4()
        {
            var source = new Metadata();
            using (var ms = new MemoryStream())
            {
                var sourceRecords = new MetadataRecord[3]
                {
                    new MetadataRecord(3, 5, 10, 0),
                    new MetadataRecord(5, 24252525, 23425, 58581758),
                    new MetadataRecord(long.MaxValue, int.MaxValue, long.MaxValue, int.MaxValue),
                };

                foreach (var record in sourceRecords)
                {
                    source.AddRecord(record);
                }

                source.Write(ms);

                var target = new Metadata();
                target.Read(ms);

                var targetRecords = target.Records.ToArray();

                sourceRecords.Should().BeEquivalentTo(targetRecords);
            }
        }
    }
}
