using System;
using System.Net;
using System.Net.Sockets;
using UHP.Lib;

namespace UHP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Server
            var udp1 = SocketHelper.BuildUdpClientBind(20_000);
            Protocol comm1 = new Protocol(udp1, new byte[] { 82, 82, 82 });

            // client
            var udp2 = SocketHelper.BuildUdpClient();
            udp2.Connect("127.0.0.1", 20_000);

            Protocol comm2 = new Protocol(udp2, new byte[] { 82, 82, 82 });

            comm2.Send(new byte[] { 1, 2, 3, 4 });

            var data = comm1.Receive();
            data = data;

        }
    }
}
