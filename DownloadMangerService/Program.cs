using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadManagerService
{
    class Program
    {
        private static readonly DirectoryInfo downloads = new DirectoryInfo
#if !DEBUG
            ("/storage/downloads");
#else
            ("storage/cdn/downloads");
#endif

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.ASCII;

            //Init Threads
            Console.WriteLine("Starting socket thread.");
            Thread socketThread = new Thread(SocketConnection);
            socketThread.Start();

            Console.WriteLine("Starting file check thread.");
            Thread fileCheckThread = new Thread(FileCheck);
            fileCheckThread.Start();
        }

        public static void SocketConnection()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = true
            };
            socket.Bind(new IPEndPoint(IPAddress.Any, 420));
            socket.Listen(5);
            while (true)
            {
                Socket dataSocket = socket.Accept();

                _ = RecieveFile(dataSocket);
            }
        }

        public static async Task RecieveFile(Socket dataSocket)
        {
            dataSocket.ReceiveTimeout = 1800;

            byte[] sizeofString = new byte[4];
            ArraySegment<byte> size = new ArraySegment<byte>(sizeofString);
            await dataSocket.ReceiveAsync(size, SocketFlags.None);

            byte[] rawFileName = new byte[BitConverter.ToInt32(sizeofString, 0)];
            ArraySegment<byte> fileNameArray = new ArraySegment<byte>(rawFileName);
            await dataSocket.ReceiveAsync(fileNameArray, SocketFlags.None);

            string fileName = Encoding.ASCII.GetString(fileNameArray);
            Console.WriteLine($"{fileName} recieved!");


            FileInfo file = downloads.GetFile(fileName);
            FileStream stream = file.Open(FileMode.Create);
            byte[] bytes = new byte[(int)2.5e+8];
            while (true)
            {
                try
                {
                    //we dont use RecieveAsync so we don't because it does not timeout automatically
                    int recv = dataSocket.Receive(bytes);
                    await stream.WriteAsync(bytes, 0, recv);
                }
                catch (Exception)
                {
                    break;
                }
            }
            stream.Flush();
            stream.Close();

            Console.WriteLine($"{fileName} written!");

            byte[] url = Encoding.ASCII.GetBytes($"Link to file: {new Uri($"https://downloads.sunthecourier.net/{fileName}").AbsoluteUri}");
            ArraySegment<byte> urlArray = new ArraySegment<byte>(url);
            await dataSocket.SendAsync(BitConverter.GetBytes(urlArray.Count), SocketFlags.None);
            await dataSocket.SendAsync(urlArray, SocketFlags.None);
            dataSocket.Close();

            //GC does not like to clean our data so im doing it myself
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static void FileCheck()
        {
            while (true)
            {
                foreach (FileInfo file in from FileInfo file in downloads.EnumerateFilesRecursively().ToArray()
                                          let time = DateTime.Now - file.CreationTime
                                          where time >= TimeSpan.FromDays(4)
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
