using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UHP.Lib
{
    public static class SocketHelper
    {
        public static IEnumerable<IPAddress> GetAllBroadcastAddresses()
        {
            yield return IPAddress.Broadcast;

            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Name.Contains("virtual", System.StringComparison.InvariantCultureIgnoreCase)) continue;
                if (ni.Name.Contains("loopback", System.StringComparison.InvariantCultureIgnoreCase)) continue;

                var ipProps = ni.GetIPProperties();

                foreach (var gw in ipProps.GatewayAddresses)
                {
                    if (gw.Address.AddressFamily == AddressFamily.InterNetworkV6) continue;

                    yield return gw.Address;

                    var bytes = gw.Address.GetAddressBytes();
                    bytes[3] = 0xff;
                    yield return new IPAddress(bytes);

                }

                foreach (var mc in ipProps.MulticastAddresses)
                {
                    if (mc.Address.AddressFamily == AddressFamily.InterNetworkV6) continue;
                    yield return mc.Address;
                }
            }

        }
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
            udp.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            return udp;
        }

    }
}
