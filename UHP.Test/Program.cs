using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UHP.Lib;

namespace UHP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //TcpListener listener = new TcpListener(15000);
            //listener.Start();

            //List<>

            //while (true)
            //{
            //    Thread.Sleep(250);
            //    if (!listener.Pending()) continue;
            //}

            var udp1 = SocketHelper.BuildUdpClientBind(20_000);
            while (true)
            {
                Thread.Sleep(100);
                if (udp1.Available == 0) continue;
                Thread.Sleep(100);

                IPEndPoint endPoint = null;
                var data = udp1.Receive(ref endPoint);

                udp1.Send(data, data.Length, endPoint);
            }



            //UPnP.Discover();

            // Server
            //var udp1 = SocketHelper.BuildUdpClientBind(20_000);
            //Protocol comm1 = new Protocol(udp1, new byte[] { 82, 82, 82 });

            //// client
            //var udp2 = SocketHelper.BuildUdpClient();
            //udp2.Connect("127.0.0.1", 444); //"www.test-api.kinghost.net", 444);
            //udp2.Send(new byte[] { 1, 2, 3, 4 }, 4);
            //var dataBack = udp2.ReceiveAsync().Result;

            //dataBack = dataBack;

            //Protocol comm2 = new Protocol(udp2, new byte[] { 82, 82, 82 });

            //comm2.Send(new byte[] { 1, 2, 3, 4 });

            //var data = comm1.Receive();
            //data = data;





        }
    }
}
