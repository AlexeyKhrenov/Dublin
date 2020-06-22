using Dublin;
using Dublin.FileStructure;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace Tests.Unit
{
    public class MetadataUnitTests
    {
        [Fact]
        public void MetadataUnitTest_1()
        {
            var metadata = new FileMetadata();
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

        [Fact]
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

            var metadata = new FileMetadata();

            using (var ms = new MemoryStream())
            {
                ms.Write(sourceBytes, 0, sourceBytes.Length);
                metadata.Read(ms);
            }

            var expectedMetadata = new FileMetadata();
            expectedMetadata.AddRecord(new MetadataRecord(3, 5, 10, 0));

            metadata.Should().BeEquivalentTo(expectedMetadata);
        }

        [Fact]
        public void MetadataUnitTest_3()
        {
            var source = new FileMetadata();
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

                var target = new FileMetadata();
                target.Read(ms);

                var targetRecords = target.Records.ToArray();

                sourceRecords.Should().BeEquivalentTo(targetRecords);
            }
        }

        [Fact]
        public void MetadataUnitTest_4()
        {
            var source = new FileMetadata();
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

                var target = new FileMetadata();
                target.Read(ms);

                var targetRecords = target.Records.ToArray();

                sourceRecords.Should().BeEquivalentTo(targetRecords);
            }
        }

        [Fact]
        public void MetadataUnitTest_5()
        {
            var metadata = new FileMetadata();
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 2, 3 }, 0, 2);
                Assert.Throws<ArgumentException>(() => metadata.Read(ms).Should());
            }
        }

        [Fact]
        public void MetadataUnitTest_6()
        {
            var metadata = new FileMetadata();
            using (var ms = new MemoryStream())
            {
                metadata.Read(ms);
            }
        }

        [Fact]
        public void MetadataUnitTest_7()
        {
            var metadata = new FileMetadata();
            using (var ms = new MemoryStream())
            {
                metadata.Write(ms);
                ms.Length.Should().Be(0);
            }
        }
    }
}
