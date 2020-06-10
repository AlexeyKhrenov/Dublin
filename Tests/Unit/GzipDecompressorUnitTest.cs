using Dublin.GzipWorkers;
using Dublin.ReaderWriterWorkers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit
{
    [TestClass]
    public class GzipDecompressorUnitTests
    {
        [TestMethod]
        public void GzipDecompressorUnitTest()
        {

        }

        private Compressor CreateCompressor()
        {
            return new Compressor(
                new ConcurrentQueue<Block>(1),
                new ConcurrentQueue<Block>(1));
        }
    }
}
