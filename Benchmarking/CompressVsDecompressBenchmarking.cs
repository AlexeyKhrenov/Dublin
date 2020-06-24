using System.IO;
using System.IO.Compression;
using System.Text;
using BenchmarkDotNet.Attributes;
using FluentAssertions;

namespace Benchmarking
{
    public class CompressVsDecompressBenchmarking
    {
        private static string inputFile = @"alice29.txt";
        
        private static byte[] toCompress;
        private static byte[] toDecompress;
        
        [GlobalSetup]
        public void Setup()
        {
            var text = File.ReadAllText(inputFile);
            toCompress = Encoding.UTF8.GetBytes(text);
            toDecompress = CompressBytes(toCompress);
        }

        [Benchmark]
        public void Compress()
        {
            CompressBytes(toCompress);
        }

        [Benchmark]
        public void Decompress()
        {
            DecompressBytes(toDecompress);
        }

        private static byte[] CompressBytes(byte[] input)
        {
            using(var compressed = new MemoryStream())
            using (var zip = new GZipStream(compressed, CompressionMode.Compress))
            {
                zip.Write(toCompress, 0, toCompress.Length);
                zip.Close(); 
                return compressed.ToArray();
            }
        }

        public static byte[] DecompressBytes(byte[] input)
        {
            using (var compressed = new MemoryStream(input))
            using (var zip = new GZipStream(compressed, CompressionMode.Decompress))
            using (var result = new MemoryStream())
            {
                zip.CopyTo(result);
                return result.ToArray();
            }
        }
    }
}