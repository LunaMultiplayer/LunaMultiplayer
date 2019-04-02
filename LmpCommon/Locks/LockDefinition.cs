using Lidgren.Network;
using LmpCommon.Message.Base;
using System;
using System.Collections.Generic;

namespace LmpCommon.Locks
{
    public class LockDefinition : IEquatable<LockDefinition>, ICloneable
    {
        /// <summary>
        /// Player who owns the lock. It should never be null
        /// </summary>
        public string PlayerName { get; internal set; } = string.Empty;

        /// <summary>
        /// Kerbal name assigned to the lock. Can be null unless lock type is kerbal
        /// </summary>
        public string KerbalName { get; private set; } = string.Empty;

        /// <summary>
        /// Vessel id assigned to the lock. Can be Guid.Empty for an asteroid lock
        /// </summary>
        public Guid VesselId { get; private set; } = Guid.Empty;

        /// <summary>
        /// The type of the lock. It should never be null
        /// </summary>
        public LockType Type { get; private set; }

        /// <summary>
        /// Parameterless constructor should not be used except for deserialization
        /// </summary>
        internal LockDefinition()
        {
        }

        /// <summary>
        /// Contract/Asteroid/Spectator constructor
        /// </summary>
        public LockDefinition(LockType type, string playerName)
        {
            if (type != LockType.Contract && type != LockType.Asteroid && type != LockType.Spectator)
                throw new Exception("This constructor is only for Contract/Asteroid/Spectator type!");

            Type = type;
            PlayerName = playerName;
        }

        /// <summary>
        /// Control/Update/UnlUpdate constructor
        /// </summary>
        public LockDefinition(LockType type, string playerName, Guid vesselId)
        {
            if (type != LockType.Control && type != LockType.Update && type != LockType.UnloadedUpdate)
                throw new Exception("This constructor is only for Control/Update/UnlUpdate type!");

            Type = type;
            PlayerName = playerName;
            VesselId = vesselId;
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
            return VesselId != Guid.Empty ? $"{Type} - {VesselId} - {PlayerName}" :
                !string.IsNullOrEmpty(KerbalName) ? $"{Type} - {KerbalName} - {PlayerName}" :
                $"{Type} - {PlayerName}";
        }

        public object Clone()
        {
            return new LockDefinition
            {
                KerbalName = KerbalName.Clone() as string,
                PlayerName = PlayerName.Clone() as string,
                VesselId = VesselId,
                Type = Type
            };
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

        #region Equatable

        public bool Equals(LockDefinition other)
        {
            if (other == null)
                return false;

            //Do not check equality in the VesselPersistentId field as it can change
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

        public static bool operator ==(LockDefinition lock1, LockDefinition lock2)
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
