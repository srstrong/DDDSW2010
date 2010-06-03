using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileFinder
{
    class DuplicateFileFinder
    {
        static readonly ConcurrentDictionary<string, string> Dictionary = new ConcurrentDictionary<string, string>();

        static void Main()
        {
            ProcessDirectory(@"c:\windows");
        }

        static void ProcessDirectory(string path)
        {
            try
            {
                var directories = Directory.GetDirectories(path);

                foreach (var directory in directories)
                {
                    ProcessDirectory(directory);
                }

            }
            catch (UnauthorizedAccessException) { }

            ProcessFiles(path);
        }

        static void ProcessFiles(string path)
        {
            var files = Directory.GetFiles(path);

            Parallel.ForEach(files, fileName =>
                                        {
                                            byte[] data;
                                            try
                                            {
                                                data = File.ReadAllBytes(fileName);
                                            }
                                            catch (IOException)
                                            {
                                                return;
                                            }

                                            var checksum = ComputeChecksum(data);

                                            if (!Dictionary.TryAdd(checksum, fileName))
                                            {
                                                Console.WriteLine("\nDuplicate!\n{0}\nis a duplicate of\n{1}", fileName, Dictionary[checksum]);
                                            }
                                        });
        }

        private static string ComputeChecksum(byte[] data)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);

            return hash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2")), sb => sb.ToString());
        }
    }
}
