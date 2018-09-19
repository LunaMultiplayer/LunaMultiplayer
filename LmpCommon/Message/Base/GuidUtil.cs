using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Reflection;
// ReSharper disable InconsistentNaming

namespace LmpCommon.Message.Base
{
    public static class GuidUtil
    {
        public static int ByteSize => 16;

        private static readonly ConcurrentBag<byte[]> ArrayPool = new ConcurrentBag<byte[]>();

        private static readonly FieldInfo _a = typeof(Guid).GetField("_a", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _b = typeof(Guid).GetField("_b", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _c = typeof(Guid).GetField("_c", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _d = typeof(Guid).GetField("_d", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _e = typeof(Guid).GetField("_e", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _f = typeof(Guid).GetField("_f", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _g = typeof(Guid).GetField("_g", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _h = typeof(Guid).GetField("_h", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _i = typeof(Guid).GetField("_i", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _j = typeof(Guid).GetField("_j", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _k = typeof(Guid).GetField("_k", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Serialize(Guid guid, NetOutgoingMessage lidgrenMsg)
        {
            if (!ArrayPool.TryTake(out var array))
                array = new byte[16];

            array[0] = (byte)((int)_a.GetValue(guid));
            array[1] = (byte)((int)_a.GetValue(guid) >> 8);
            array[2] = (byte)((int)_a.GetValue(guid) >> 16);
            array[3] = (byte)((int)_a.GetValue(guid) >> 24);
            array[4] = (byte)((short)_b.GetValue(guid));
            array[5] = (byte)((short)_b.GetValue(guid) >> 8);
            array[6] = (byte)((short)_c.GetValue(guid));
            array[7] = (byte)((short)_c.GetValue(guid) >> 8);
            array[8] = (byte)_d.GetValue(guid);
            array[9] = (byte)_e.GetValue(guid);
            array[10] = (byte)_f.GetValue(guid);
            array[11] = (byte)_g.GetValue(guid);
            array[12] = (byte)_h.GetValue(guid);
            array[13] = (byte)_i.GetValue(guid);
            array[14] = (byte)_j.GetValue(guid);
            array[15] = (byte)_k.GetValue(guid);

            lidgrenMsg.Write(array, 0, 16);

            ArrayPool.Add(array);
        }

        public static Guid Deserialize(NetIncomingMessage lidgrenMsg)
        {
            if (!ArrayPool.TryTake(out var array))
                array = new byte[16];

            lidgrenMsg.ReadBytes(array, 0, 16);
            var guid = new Guid(array);

            ArrayPool.Add(array);

            return guid;
        }
    }
}
