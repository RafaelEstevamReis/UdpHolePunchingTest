using System;
using System.Runtime.InteropServices;

namespace UHP.Lib
{
    public static  class RawDataSerialization
    {
        public static T RawDeserialize<T>(byte[] rawData) where T : struct
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, 0, buffer, rawsize);
            T obj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return obj;
        }
        public static byte[] RawSerialize<T>(T value) where T : struct
        {
            int rawSize = Marshal.SizeOf(value);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(value, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
        }

        public static void Send<T>(this Protocol protocol, T value) where T : struct
        {
            var data = RawSerialize(value);
            protocol.Send(data);
        }
        public static T ReceiveAs<T>(this Protocol protocol) where T : struct
        {
            var data = protocol.Receive();
            return RawDeserialize<T>(data.Buffer);
        }
    }
}
