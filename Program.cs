using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DownloadsManagerService
{
    class Program
    {
        private static readonly DirectoryInfo downloads = new DirectoryInfo("/storage/files/downloads");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.ASCII;

            //Init Thread
            Console.WriteLine("Starting file check thread.");
            Thread fileCheckThread = new Thread(FileCheck);
            fileCheckThread.Start();
            Console.WriteLine("Init finished.");
        }

        public static void FileCheck()
        {
            while (true)
            {
                IEnumerable<FileInfo> Files = downloads.EnumerateFilesRecursively().Where(x => (DateTime.UtcNow - x.LastAccessTimeUtc) >= TimeSpan.FromDays(3));
                foreach (FileInfo file in Files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to remove {file.FullName}!");
                        continue;
                    }
                    Console.WriteLine($"{file.FullName} removed!");
                }

                IEnumerable<DirectoryInfo> Directories = downloads.EnumerateDirectories("*", SearchOption.AllDirectories).Where(x => x.EnumerateFilesRecursively().Count() == 0 && (DateTime.UtcNow - x.LastAccessTimeUtc) >= TimeSpan.FromDays(3));
                foreach (DirectoryInfo directory in Directories)
                {
                    try
                    {
                        directory.Delete();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to remove {directory.FullName}!");
                        continue;
                    }
                    Console.WriteLine($"{directory.FullName} removed!");
                }
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }
    }
}
