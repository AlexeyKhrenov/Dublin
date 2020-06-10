using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin.FileStructure
{
    /// <summary>
    /// Size - 32 bytes
    /// </summary>
    public struct MetadataRecord
    {
        // todo - make this readonly structure
        public long Start { get; set;}
        public int Length { get; set;}
        public long StartCompressed { get; set;}
        public int LengthCompressed { get; set; }

        public MetadataRecord(long start, int length)
        {
            Start = start;
            Length = length;
            StartCompressed = 0;
            LengthCompressed = 0;
        }

        public MetadataRecord(long start, int length, long startCompressed, int lengthCompressed)
        {
            Start = start;
            Length = length;
            StartCompressed = startCompressed;
            LengthCompressed = lengthCompressed;
        }

        
    }
}
