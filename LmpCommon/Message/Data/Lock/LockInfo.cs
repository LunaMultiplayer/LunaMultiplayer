using Lidgren.Network;
using LmpCommon.Locks;
using LmpCommon.Message.Base;
using System;

namespace LmpCommon.Message.Data.Lock
{
    public class LockInfo
    {
        public string PlayerName;
        public string KerbalName;
        public Guid VesselId;
        public LockType Type;

        public LockInfo() { }

        public LockInfo(LockDefinition definition)
        {
            PlayerName = definition.PlayerName.Clone() as string;
            KerbalName = definition.KerbalName.Clone() as string;
            VesselId = definition.VesselId;
            Type = definition.Type;
        }
        
        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(KerbalName);
            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write((int)Type);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
            KerbalName = lidgrenMsg.ReadString();
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            Type = (LockType)lidgrenMsg.ReadInt32();
        }

        public LockDefinition AsLockDefinition()
        {
            return new LockDefinition(this);
        }

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + KerbalName.GetByteCount() + GuidUtil.ByteSize + sizeof(LockType);
        }
    }
}
