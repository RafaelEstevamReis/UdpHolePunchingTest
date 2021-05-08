using System.Net;
using System.Net.Sockets;

namespace UHP.Lib
{
    public static class SocketHelper
    {
        public static UdpClient BuildUdpClient()
        {
            var udp = new UdpClient()
            {
                ExclusiveAddressUse = false,
            };
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            return udp;
        }
        public static UdpClient BuildUdpClientBind(int port)
        {
            var udp = BuildUdpClient();
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            return udp;
        }

    }
}
