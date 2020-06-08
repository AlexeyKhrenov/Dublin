using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin
{
    public class FileMetadata
    {
        // todo - add initial info

        private Queue<MetadataRecord> records;

        public FileMetadata()
        {
        }

        /// <summary>
        /// Read metadata from end of the stream. Sets stream position back after read.
        /// </summary>
        public void Read(Stream stream)
        {
            var initialPosition = stream.Position;
            var readValues = new List<long>();
        }

        /// <summary>
        /// Writes current metadata state at the end of the stream.
        /// </summary>
        public void Write(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
            }
        }

        private class MetadataRecord
        {
            public long Start0;
            public int Length0;

            public long Start1;
            public int Length1;
        }
    }
}
