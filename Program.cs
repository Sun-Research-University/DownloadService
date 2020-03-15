using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DownloadManagerService
{
    class Program
    {
        private static readonly DirectoryInfo downloads = new DirectoryInfo("storage/cdn/files/downloads");


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
                foreach (FileInfo file in from FileInfo file in downloads.EnumerateFilesRecursively().ToArray()
                                          let time = DateTime.Now - file.CreationTime
                                          where time >= TimeSpan.FromDays(3)
                                          select file)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to remove {file.FullName}!");
                    }
                    Console.WriteLine($"{file.Name} removed!");
                }

                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }
    }
}
