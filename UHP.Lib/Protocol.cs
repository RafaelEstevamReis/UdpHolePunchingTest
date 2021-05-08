using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UHP.Lib
{
    public class Protocol
    {
        private UdpClient udpClient;
        private byte[] magic;

        public IPEndPoint RemoteEndPoint { get; set; }

        // Protocol
        //  [Magic] [Len:2 bytes] [data:len]

        public byte[] MagicNumber { get; private set; }
        public bool IsAvalilableData => udpClient.Available < magic.Length + 2 + 1;

        public Protocol(UdpClient udp, byte[] magicNumber)
        {
            udpClient = udp;
            magic = magicNumber;
        }

        //public void Send<T>(T data)
        //{
        //}
        //public T Receive<T>()
        //{
        //    return default(T);
        //}

        public void Send(byte[] data)
        {
            if (data.Length > 0xFFFF) throw new Exception("Packet must be smaller than 0xFFFF");

            byte l1 = (byte)(data.Length & 0x00000FF);
            byte l2 = (byte)((data.Length & 0x000FF00) >> 8);

            var buffer = new byte[magic.Length + 2 + data.Length];
            Buffer.BlockCopy(magic, 0, buffer, 0, magic.Length);

            buffer[magic.Length + 0] = l1;
            buffer[magic.Length + 1] = l2;

            Buffer.BlockCopy(data, 0, buffer, magic.Length + 2, data.Length);

            if (RemoteEndPoint == null)
                udpClient.Send(buffer, buffer.Length);
            else
                udpClient.Send(buffer, buffer.Length, RemoteEndPoint);

        }
        public UdpReceiveResult Receive()
        {
            if (IsAvalilableData) return new UdpReceiveResult();

            var received = udpClient.ReceiveAsync().Result;

            // Test received data
            if (!compare(magic, received.Buffer)) throw new Exception("Invalid MagicNumber");
            int len = received.Buffer[magic.Length] + (received.Buffer[magic.Length + 1] << 8);
            if (len != received.Buffer.Length - 2 - magic.Length) throw new Exception("Len mismatch");
            // crop buffer header
            var data = new byte[len];
            Buffer.BlockCopy(received.Buffer, magic.Length + 2, data, 0, len);

            return new UdpReceiveResult(data, received.RemoteEndPoint);
        }
        private static bool compare(byte[] b1, byte[] b2)
        {
            for (int i = 0; i < Math.Min(b1.Length, b2.Length); i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }


    }
}
