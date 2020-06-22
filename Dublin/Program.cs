using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Benchmarking")]
[assembly: InternalsVisibleTo("Tests")]
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

                var conf = GetConfiguration();

                builder = new Builder(
                    args[1],
                    args[2],
                    GetValueFromConfig(conf, "ReadQueueSize"),
                    GetValueFromConfig(conf, "WriteQueueSize"),
                    GetValueFromConfig(conf, "BlockSize"));

                var orc = builder.BuildOrchestrator(mode, Environment.ProcessorCount);

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

        private static int GetValueFromConfig(IConfigurationRoot conf, string name)
        {
            var a = conf.GetSection(name).Value;
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

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .Build();
        }
    }
}
