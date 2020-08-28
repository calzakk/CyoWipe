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
                    Console.WriteLine("No disk specified!");
                    return 1;
                }

                var disk = args[0];
                if (disk.Length != 1)
                {
                    Console.WriteLine("Disk letter not specified!");
                    return 1;
                }

                FillDisk(disk);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        static void FillDisk(string disk)
        {
            Console.WriteLine($"Filling disk {disk}:...");

            for (int index = 1; ; ++index)
            {
                var pathname = $"{disk}:\\__cyowipe{index}.txt";
                if (File.Exists(pathname))
                    continue;

                using (var rng = RandomNumberGenerator.Create())
                    FillDisk(pathname, rng);

                var fileInfo = new FileInfo(pathname);
                if (fileInfo.Length == 0)
                    throw new Exception("File is empty!");

                Console.WriteLine($"{fileInfo.Length} bytes written");
            }
        }

        static void FillDisk(string pathname, RandomNumberGenerator rng)
        {
            Console.WriteLine($"New file: {pathname}");

            long bytesWritten = 0;
            using (var stream = new FileStream(pathname, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
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
}
