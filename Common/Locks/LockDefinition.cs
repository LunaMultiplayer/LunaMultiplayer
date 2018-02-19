using Lidgren.Network;
using LunaCommon.Message.Base;
using System;

namespace LunaCommon.Locks
{
    public class LockDefinition : IEquatable<LockDefinition>
    {
        /// <summary>
        /// Player who owns the lock. It should never be null
        /// </summary>
        public string PlayerName { get; internal set; }

        /// <summary>
        /// Vessel id assigned to the lock. Can be null for an asteroid lock
        /// </summary>
        public Guid VesselId { get; internal set; }

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

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write((int)Type);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            Type = (LockType)lidgrenMsg.ReadInt32();
        }

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + GuidUtil.GetByteSize() + sizeof(LockType);
        }

        public void CopyFrom(LockDefinition lockDefinition)
        {
            PlayerName = lockDefinition.PlayerName.Clone() as string;
            Type = lockDefinition.Type;
            VesselId = lockDefinition.VesselId;
        }

        #region Equatable

        public bool Equals(LockDefinition other)
        {
            if (other == null)
                return false;

            return PlayerName == other.PlayerName && VesselId == other.VesselId && Type == other.Type;
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
            unchecked
            {
                var hashCode = (PlayerName != null ? StringComparer.InvariantCulture.GetHashCode(PlayerName) : 0);
                hashCode = (hashCode * 397) ^ VesselId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
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
