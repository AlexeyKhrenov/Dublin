using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Dublin.FileStructure
{
    public class Block
    {
        public byte[] Content { get; set; }

        public MetadataRecord Metadata { get; private set; }

        public int Size { get; private set; }

        private Block() { }

        private Block(int size)
        {
            Content = new byte[size];
            Size = size;
        }

        public void AddCompressedPositionStart(long start)
        {
            Metadata = new MetadataRecord(
                Metadata.Start,
                Metadata.Length,
                start,
                Metadata.LengthCompressed);
        }

        public static Block CreateBlockForCompression(long start, int size)
        {
            var result = new Block(size);
            result.Metadata = new MetadataRecord(start, size);
            return result;
        }

        public static Block CreateCompressedBlock(Block sourceBlock, byte[] content)
        {
            var result = new Block(content.Length);
            result.Content = content;

            result.Metadata = new MetadataRecord(
                sourceBlock.Metadata.Start,
                sourceBlock.Metadata.Length,
                0,
                content.Length);

            return result;
        }

        public static Block CreateDecompressedBlock(Block sourceBlock, byte[] content)
        {
            var result = new Block(content.Length);
            result.Content = content;

            result.Metadata = new MetadataRecord(
                sourceBlock.Metadata.Start,
                sourceBlock.Metadata.Length);

            return result;
        }

        public static Block CreateBlockForDecompression(
            long startDecompressed,
            int lengthDecompressed,
            long startCompressed,
            int lengthCompressed)
        {
            var result = new Block(lengthCompressed);
            result.Metadata = new MetadataRecord(startDecompressed, lengthDecompressed, startCompressed, lengthCompressed);

            return result;
        }
    }
}
