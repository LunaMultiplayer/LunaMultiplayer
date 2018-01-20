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
            lidgrenMsg.WritePadBits();

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
            lidgrenMsg.SkipPadBits();

            VabCraftCount = lidgrenMsg.ReadInt32();
            if (VabCraftNames.Length < VabCraftCount)
                VabCraftNames = new string[VabCraftCount];

            for (var i = 0; i < VabCraftCount; i++)
                VabCraftNames[i] = lidgrenMsg.ReadString();

            SphCraftCount = lidgrenMsg.ReadInt32();
            if (SphCraftNames.Length < SphCraftCount)
                SphCraftNames = new string[SphCraftCount];

            for (var i = 0; i < SphCraftCount; i++)
                SphCraftNames[i] = lidgrenMsg.ReadString();

            SubassemblyCraftCount = lidgrenMsg.ReadInt32();
            if (SubassemblyCraftNames.Length < SubassemblyCraftCount)
                SubassemblyCraftNames = new string[SubassemblyCraftCount];

            for (var i = 0; i < SubassemblyCraftCount; i++)
                SubassemblyCraftNames[i] = lidgrenMsg.ReadString();
        }
        
        public int GetByteCount()
        {            
            //We use sizeof(byte) instead of sizeof(bool) * 3 because we use the WritePadBits()
            return sizeof(byte) + sizeof(int) * 3
                   + VabCraftNames.GetByteCount(VabCraftCount)
                   + SphCraftNames.GetByteCount(SphCraftCount)
                   + SubassemblyCraftNames.GetByteCount(SubassemblyCraftCount);
        }
    }
}