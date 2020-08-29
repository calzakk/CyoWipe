using System;
using System.IO;
using System.Security.Cryptography;

namespace CyoWipe
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("No folder specified!");
                    return 1;
                }

                var folder = args[0];
                if (!Directory.Exists(folder))
                {
                    Console.WriteLine("Folder doesn't exist!");
                    return 1;
                }

                CreateFile(folder);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        static void CreateFile(string folder)
        {
            for (int index = 1; ; ++index)
            {
                var pathname = Path.Combine(folder, $"__cyowipe{index}.txt");
                if (File.Exists(pathname))
                    continue;

                using (var rng = RandomNumberGenerator.Create())
                    CreateFile(pathname, rng);

                var fileInfo = new FileInfo(pathname);
                if (fileInfo.Length == 0)
                    throw new Exception("File is empty!");

                Console.WriteLine($"{fileInfo.Length} bytes written");
            }
        }

        static void CreateFile(string pathname, RandomNumberGenerator rng)
        {
            Console.WriteLine($"New file: {pathname}");

            long bytesWritten = 0;
            using var stream = new FileStream(pathname, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var started = Environment.TickCount;
            var lastProgress = started;

            const int Mebibytes = (1024 * 1024);
            const int Gibibytes = (Mebibytes * 1024);
            int bufferSize = Mebibytes;

            while (true)
            {
                try
                {
                    var bytes = new byte[bufferSize];
                    rng.GetBytes(bytes);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    bytesWritten += bufferSize;

                    var now = Environment.TickCount;
                    if (now - lastProgress >= 1000) //every second
                    {
                        Console.Write($"\r{bytesWritten / Gibibytes:n0} GiB");
                        lastProgress = now;
                    }
                }
                catch (IOException ex)
                {
                    if (bufferSize <= 1)
                    {
                        Console.WriteLine($"\n{ex.Message}");
                        break;
                    }
                    bufferSize /= 2;
                }
            }
        }
    }
}
