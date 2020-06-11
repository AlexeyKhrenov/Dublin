using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dublin.FileStructure
{
    public class FileMetadata
    {
        public Queue<MetadataRecord> Records = new Queue<MetadataRecord>();

        private int recordSize = Marshal.SizeOf(typeof(MetadataRecord));

        public void AddRecord(MetadataRecord metadataRecord)
        {
            Records.Enqueue(metadataRecord);
        }

        public bool TryGetNextRecord(out MetadataRecord record)
        {
            if (!Records.Any())
            {
                record = default;
                return false;
            }
            else
            {
                record = Records.Dequeue();
                return true;
            }
        }

        /// <summary>
        /// Throws ArgumentException if input stream has incorrect format.
        /// Read metadata from end of the stream. 
        /// Sets stream position back after read.
        /// Returns start position of metadata
        /// </summary>
        public long Read(Stream stream)
        {
            if (stream.Length == 0)
            {
                return 0;
            }

            if (stream.Length < 4)
            {
                throw new ArgumentException("Invalid input stream");
            }

            using (var binaryReader = new BinaryReader(stream, Encoding.Default, true))
            {
                var savedStreamPosition = stream.Position;

                var start = 0;
                var buffer = new long[recordSize / 8];
                var bufferGCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var bufferPtr = bufferGCHandle.AddrOfPinnedObject();

                try
                {
                    stream.Seek(-4, SeekOrigin.End);
                    var numberOfRecords = binaryReader.ReadInt32();

                    while (start != numberOfRecords)
                    {
                        start++;

                        stream.Seek(-4 - (start * recordSize), SeekOrigin.End);
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = binaryReader.ReadInt64();
                        }

                        var result = (MetadataRecord)Marshal.PtrToStructure(bufferPtr, typeof(MetadataRecord));
                        Records.Enqueue(result);
                    }
                }
                finally
                {
                    stream.Position = savedStreamPosition;
                    bufferGCHandle.Free();
                }

                return -4 - (long)(start * recordSize);
            }
        }

        /// <summary>
        /// Writes current metadata state at the end of the stream. 
        /// Sets stream position at the end of the stream after write.
        /// Adds additional 4 bytes to track number of records if there're records
        /// </summary>
        public void Write(Stream stream)
        {
            stream.Seek(0, SeekOrigin.End);
            int numberOfRecords = Records.Count;

            if (numberOfRecords == 0)
            {
                return;
            }

            var buffer = new byte[recordSize];
            IntPtr ptr = Marshal.AllocHGlobal(recordSize);

            while (Records.Count != 0)
            {
                var record = Records.Dequeue();
                Marshal.StructureToPtr(record, ptr, true);
                Marshal.Copy(ptr, buffer, 0, recordSize);
                stream.Write(buffer, 0, recordSize);
            }

            Marshal.FreeHGlobal(ptr);

            stream.Write(BitConverter.GetBytes(numberOfRecords), 0, 4);
        }
    }
}
