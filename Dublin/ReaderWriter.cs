using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin
{
    public class ReaderWriter : IDisposable, IConcurrentReaderWriter
    {
        private FileStream input;
        private FileStream output;

        public bool CanRead => input.CanRead;

        public ReaderWriter(string inputFile, string outputFile)
        {
            // todo - consider this constructors for perfomance
            input = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            output = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public long ReadNext(Stream destination, int length)
        {
            // make this thread-safe
            var start = input.Position;
            input.CopyTo(destination, length);
            return start;
        }

        public void WriteBlock(Stream source, int position)
        {
            output.Position = position;
            source.CopyTo(output);
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }

        public void Run()
        {
            using (var inputFile = new FileStream("enwik9", FileMode.Open, FileAccess.Read))
            {
                using (var outputFile = new FileStream("enwik9Compressed", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (var gzipStream = new GZipStream(outputFile, CompressionLevel.Optimal))
                    {
                        inputFile.CopyTo(gzipStream);
                    }
                }
            }
        }

        public void RunInBlocks()
        {
            int length = 1024 * 1024;

            using (var inputFile = new FileStream("enwik9", FileMode.Open, FileAccess.Read))
            {
                using (var outputFile = new FileStream("enwik9Compressed", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (var gzipStream = new GZipStream(outputFile, CompressionLevel.Optimal))
                    {
                        while (inputFile.CanRead)
                        {
                            inputFile.CopyTo(gzipStream, length);
                            gzipStream.Position = 0;
                        }
                    }
                }
            }
        }
    }
}
