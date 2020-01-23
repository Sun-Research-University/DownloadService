using Sluggy;
using SluggyUnidecode;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataSender
{
    class Program
    {
        //Static IP
        private static readonly IPAddress IP = IPAddress.Parse("192.168.0.25");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.ASCII;

            FileInfo file = new FileInfo(args[0]);
            if (!file.Exists)
                throw new FileNotFoundException();

            Console.WriteLine("Sending data...");
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IP, 420);

            //Except does not worth with this???
            byte[] fname = Encoding.ASCII.GetBytes(new string(file.Name.ToSlug("_", new UnidecodeStrategy()).Replace(' ', '_').Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray()));
            socket.Send(BitConverter.GetBytes(fname.Length));
            socket.Send(fname);

            using (Stream source = file.OpenRead())
            {
                byte[] buffer = new byte[(int)5e+8];
                int size;
                do
                {
                    size = source.Read(buffer, 0, buffer.Length);
                    socket.Send(buffer, 0, size, SocketFlags.None);

                }
                while (size > 0);
            }

            Console.WriteLine("Data sent!");

            byte[] sizeofString = new byte[4];
            socket.Receive(sizeofString);
            byte[] urlSite = new byte[BitConverter.ToInt32(sizeofString, 0)];
            socket.Receive(urlSite);

            Console.WriteLine(Encoding.ASCII.GetString(urlSite));
            socket.Disconnect(false);
            socket.Dispose();

            Console.WriteLine("");
            Console.ReadKey();
        }
    }
}
