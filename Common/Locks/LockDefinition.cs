using Lidgren.Network;
using LunaCommon.Message.Base;
using System;
using System.Collections.Generic;

namespace LunaCommon.Locks
{
    public class LockDefinition : IEquatable<LockDefinition>
    {
        /// <summary>
        /// Player who owns the lock. It should never be null
        /// </summary>
        public string PlayerName { get; internal set; } = string.Empty;

        /// <summary>
        /// Kerbal name assigned to the lock. Can be null unless lock type is kerbal
        /// </summary>
        public string KerbalName { get; internal set; } = string.Empty;

        /// <summary>
        /// Vessel id assigned to the lock. Can be Guid.Empty for an asteroid lock
        /// </summary>
        public Guid VesselId { get; internal set; } = Guid.Empty;

        /// <summary>
        /// Vessel persistent id assigned to the lock. Can be Guid.Empty for an asteroid lock
        /// </summary>
        public uint VesselPersistentId { get; internal set; } = 0;

        /// <summary>
        /// The type of the lock. It should never be null
        /// </summary>
        public LockType Type { get; internal set; }

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
        public LockDefinition(LockType type, string playerName, Guid vesselId, uint persistentId)
        {
            Type = type;
            PlayerName = playerName;
            VesselId = vesselId;
            VesselPersistentId = persistentId;
        }

        /// <summary>
        /// Kerbal constructor
        /// </summary>
        public LockDefinition(LockType type, string playerName, string kerbalName)
        {
            if (type != LockType.Kerbal) throw new Exception("This constructor is only for kerbal type!");

            Type = type;
            PlayerName = playerName;
            KerbalName = kerbalName;
        }

        public override string ToString()
        {
            return VesselId != Guid.Empty ? $"{Type} - {VesselId}" : 
                !string.IsNullOrEmpty(KerbalName) ? $"{Type} - {KerbalName}" :
                $"{Type}";
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

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + KerbalName.GetByteCount() + GuidUtil.ByteSize + sizeof(LockType);
        }

        public void CopyFrom(LockDefinition lockDefinition)
        {
            PlayerName = lockDefinition.PlayerName.Clone() as string;
            Type = lockDefinition.Type;
            KerbalName = lockDefinition.KerbalName;
            VesselId = lockDefinition.VesselId;
        }

        #region Equatable

        public bool Equals(LockDefinition other)
        {
            if (other == null)
                return false;

            return PlayerName == other.PlayerName && VesselId == other.VesselId && Type == other.Type && KerbalName == other.KerbalName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var lockObj = obj as LockDefinition;
            return lockObj != null && Equals(lockObj);
        }

        public override int GetHashCode()
        {
            var hashCode = -423896247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PlayerName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KerbalName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(VesselId);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        public static bool operator == (LockDefinition lock1, LockDefinition lock2)
        {
            if ((object)lock1 == null || (object)lock2 == null)
                return Equals(lock1, lock2);

            return lock1.Equals(lock2);
        }

        public static bool operator !=(LockDefinition lock1, LockDefinition lock2)
        {
            if ((object)lock1 == null || (object)lock2 == null)
                return !Equals(lock1, lock2);

            return !lock1.Equals(lock2);
        }

        #endregion
    }
}
