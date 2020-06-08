using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dublin
{
    public interface IConcurrentReaderWriter
    {
        bool CanRead { get; }

        /// <summary>
        /// returns position of the start of the block
        /// </summary>
        long ReadNext(Stream destination, int blockSize);

        void Write(Stream source, int position);
    }
}
