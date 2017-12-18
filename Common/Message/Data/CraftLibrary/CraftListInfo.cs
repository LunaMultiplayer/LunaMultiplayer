using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftListInfo
    {
        public bool VabExists;
        public bool SphExists;
        public bool SubassemblyExists;

        public int VabCraftCount;
        public string[] VabCraftNames = new string[0];

        public int SphCraftCount;
        public string[] SphCraftNames = new string[0];

        public int SubassemblyCraftCount;
        public string[] SubassemblyCraftNames = new string[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(VabExists);
            lidgrenMsg.Write(SphExists);
            lidgrenMsg.Write(SubassemblyExists);

            lidgrenMsg.Write(VabCraftCount);
            for (var i = 0; i < VabCraftCount; i++)
                lidgrenMsg.Write(VabCraftNames[i]);

            lidgrenMsg.Write(SphCraftCount);
            for (var i = 0; i < SphCraftCount; i++)
                lidgrenMsg.Write(SphCraftNames[i]);

            lidgrenMsg.Write(SubassemblyCraftCount);
            for (var i = 0; i < SubassemblyCraftCount; i++)
                lidgrenMsg.Write(SubassemblyCraftNames[i]);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            VabExists = lidgrenMsg.ReadBoolean();
            SphExists = lidgrenMsg.ReadBoolean();
            SubassemblyExists = lidgrenMsg.ReadBoolean();

            VabCraftCount = lidgrenMsg.ReadInt32();
            VabCraftNames = ArrayPool<string>.Claim(VabCraftCount);
            for (var i = 0; i < VabCraftCount; i++)
                VabCraftNames[i] = lidgrenMsg.ReadString();

            SphCraftCount = lidgrenMsg.ReadInt32();
            SphCraftNames = ArrayPool<string>.Claim(SphCraftCount);
            for (var i = 0; i < SphCraftCount; i++)
                SphCraftNames[i] = lidgrenMsg.ReadString();

            SubassemblyCraftCount = lidgrenMsg.ReadInt32();
            SubassemblyCraftNames = ArrayPool<string>.Claim(SubassemblyCraftCount);
            for (var i = 0; i < SubassemblyCraftCount; i++)
                SubassemblyCraftNames[i] = lidgrenMsg.ReadString();
        }

        public void Recycle()
        {
            ArrayPool<string>.Release(ref VabCraftNames);
            ArrayPool<string>.Release(ref SphCraftNames);
            ArrayPool<string>.Release(ref SubassemblyCraftNames);
        }

        public int GetByteCount()
        {
            return sizeof(bool) * 3 + sizeof(int) * 3
                   + VabCraftNames.GetByteCount(VabCraftCount)
                   + SphCraftNames.GetByteCount(SphCraftCount)
                   + SubassemblyCraftNames.GetByteCount(SubassemblyCraftCount);
        }
    }
}