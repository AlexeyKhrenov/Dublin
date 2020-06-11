using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dublin
{
    class Program
    {
        static void Main(string[] args)
        {
            Builder builder = null;

            try
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException("Expected number of arguments - 3");
                }

                var mode = ParseCompressDecomress(args[0]);

                builder = new Builder(
                    args[1],
                    args[2],
                    GetValueFromConfig("ReadQueueSize"),
                    GetValueFromConfig("WriteQueueSize"),
                    GetValueFromConfig("BlockSize"));

                var orc = builder.BuildOrchestrator(mode);

                var cts = new CancellationTokenSource();
                var orcThread = new Thread(() => orc.Start(cts.Token));
                orcThread.Start();

                Console.WriteLine("Started processing, press any key to cancel");

                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        throw new OperationCanceledException("Operation was cancelled. Clearing up resources");
                    }

                    if (orcThread.Join(500))
                    {
                        break;
                    }
                }

                Console.WriteLine("Successfully finished");
            }
            catch (Exception ex)
            {
                Error(ex);
                try
                {
                    builder?.Clearup();
                }
                catch (Exception clearUpEx)
                {
                    Error(clearUpEx);
                }
            }
            finally
            {
                builder?.Dispose();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        /// <summary>
        /// returns true if compress
        /// </summary>
        private static CompressionMode ParseCompressDecomress(string input)
        {
            if (input.Equals("compress", StringComparison.OrdinalIgnoreCase))
            {
                return CompressionMode.Compress;
            }

            if (input.Equals("decompress", StringComparison.OrdinalIgnoreCase))
            {
                return CompressionMode.Decompress;
            }

            throw new ArgumentException("Couldn't parse first argument - should be 'compress' or 'decompress'");
        }

        private static void Error(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
        }

        private static int GetValueFromConfig(string name)
        {
            var a = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(a))
            {
                throw new ArgumentException($"Value {name} is not specified in App.config");
            }
            if (!Int32.TryParse(a, out var result))
            {
                throw new ArgumentException($"Couldn't parse value {a} for key {name} in App.config");
            }

            return result;
        }
    }
}
