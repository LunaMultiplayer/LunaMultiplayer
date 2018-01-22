using Lidgren.Network;
using LunaCommon.Message.Base;
using System;

namespace LunaCommon.Locks
{
    public class LockDefinition
    {
        /// <summary>
        /// Player who owns the lock. It should never be null
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Vessel id assigned to the lock. Can be null for an asteroid lock
        /// </summary>
        public Guid VesselId { get; set; }

        /// <summary>
        /// The type of the lock. It should never be null
        /// </summary>
        public LockType Type { get; set; }

        /// <summary>
        /// Parameterless constructor should not be used except for deserialization
        /// </summary>
        internal LockDefinition()
        {
        }

        /// <summary>
        /// Most basic constructor
        /// </summary>
        public LockDefinition(LockType type, string playerName)
        {
            Type = type;
            PlayerName = playerName;
        }

        /// <summary>
        /// Standard constructor
        /// </summary>
        public LockDefinition(LockType type, string playerName, Guid vesselId)
        {
            Type = type;
            PlayerName = playerName;
            VesselId = vesselId;
        }

        public override string ToString()
        {
            return VesselId != Guid.Empty ? $"{Type} - {VesselId}" : $"{Type}";
        }

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(PlayerName);
            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write((int)Type);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            PlayerName = lidgrenMsg.ReadString();
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            Type = (LockType) lidgrenMsg.ReadInt32();
        }

        public int GetByteSize(bool dataCompressed)
        {
            return PlayerName.GetByteCount() + GuidUtil.GetByteSize() + sizeof(LockType);
        }
        
        public void CopyFrom(LockDefinition lockDefinition)
        {
            PlayerName = lockDefinition.PlayerName.Clone() as string;
            Type = lockDefinition.Type;
            VesselId = lockDefinition.VesselId;
        }
    }
}
