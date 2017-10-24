using System;

namespace LunaClient.Network.Icmp
{
    public class InformationMessage : IcmpMessage
    {
        public ushort Identifier { get; set; } = 0;
        public ushort SequenceNumber { get; set; } = 0;

        public override byte[] GetObjectBytes()
        {
            var ret = new byte[8];
            Array.Copy(base.GetObjectBytes(), 0, ret, 0, 4);
            Array.Copy(BitConverter.GetBytes(Identifier), 0, ret, 4, 2);
            Array.Copy(BitConverter.GetBytes(SequenceNumber), 0, ret, 6, 2);
            return ret;
        }
    }
}
