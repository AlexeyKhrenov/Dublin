using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dublin
{
    /// <summary>
    /// Maximum number of records - int.Max
    /// </summary>
    public class Metadata
    {
        public Queue<MetadataRecord> Records = new Queue<MetadataRecord>();

        private int recordSize = Marshal.SizeOf(typeof(MetadataRecord));

        public void AddRecord(MetadataRecord metadataRecord)
        {
            Records.Enqueue(metadataRecord);
        }

        /// <summary>
        /// Throws ArgumentException if input stream has incorrect format.
        /// Read metadata from end of the stream. 
        /// Sets stream position back after read.
        /// </summary>
        public void Read(Stream stream)
        {
            if (stream.Length == 0)
            {
                return;
            }

            if (stream.Length < 4)
            {
                throw new ArgumentException("Invalid input stream");
            }

            using (var binaryReader = new BinaryReader(stream))
            {
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
                    bufferGCHandle.Free();
                }
            }
        }

        /// <summary>
        /// Writes current metadata state at the end of the stream. 
        /// Sets stream position at the end of the stream after write.
        /// Adds additional 4 bytes to track number of records.
        /// </summary>
        public void Write(Stream stream)
        {
            stream.Seek(0, SeekOrigin.End);
            int numberOfRecords = Records.Count;
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
