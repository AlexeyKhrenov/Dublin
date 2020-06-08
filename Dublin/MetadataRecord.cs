using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin
{
    /// <summary>
    /// Size - 32 bytes
    /// </summary>
    public readonly struct MetadataRecord
    {
        public readonly long Start0;
        public readonly int Length0;
        public readonly long Start1;
        public readonly int Length1;

        public MetadataRecord(long start0, int length0, long start1, int length1)
        {
            Start0 = start0;
            Length0 = length0;
            Start1 = start1;
            Length1 = length1;
        }
    }
}
