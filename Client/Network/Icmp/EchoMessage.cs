using System;

namespace LunaClient.Network.Icmp
{
    public class EchoMessage : InformationMessage
    {
        public byte[] Data { get; set; }

        public override byte[] GetObjectBytes()
        {
            var length = 8;
            if (Data != null)
                length += Data.Length;
            var ret = new byte[length];
            Array.Copy(base.GetObjectBytes(), 0, ret, 0, 8);
            if (Data != null)
                Array.Copy(Data, 0, ret, 8, Data.Length);
            return ret;
        }
    }
}
