using System;

namespace LunaClient.Network.Icmp
{
    public abstract class IcmpMessage
    {
        public byte Code { get; set; } = 0;
        public byte Type { get; set; }
        public ushort CheckSum { get; set; }

        public virtual byte[] GetObjectBytes()
        {
            var ret = new byte[4];
            Array.Copy(BitConverter.GetBytes(Type), 0, ret, 0, 1);
            Array.Copy(BitConverter.GetBytes(Code), 0, ret, 1, 1);
            Array.Copy(BitConverter.GetBytes(CheckSum), 0, ret, 2, 2);
            return ret;
        }

        public ushort GetChecksum()
        {
            ulong sum = 0;
            var bytes = GetObjectBytes();
            // Sum all the words together, adding the final byte if size is odd
            int i;
            for (i = 0; i < bytes.Length - 1; i += 2)
            {
                sum += BitConverter.ToUInt16(bytes, i);
            }
            if (i != bytes.Length)
                sum += bytes[i];
            // Do a little shuffling
            sum = (sum >> 16) + (sum & 0xFFFF);
            sum += sum >> 16;
            return (ushort)~sum;
        }
    }
}
