using System;

namespace LmpClient.VesselUtilities
{
    public enum PersistentIdEvictionDecision
    {
        NotRegistered,
        EvictStaleOwner,
        KeepForeignOwner,
    }

    public static class PersistentIdEvictionPolicy
    {
        public static PersistentIdEvictionDecision DecideUnloaded(
            bool registered, bool ownerSnapshotNull, bool ownerVesselNull, bool ownerIsIncomingVessel)
        {
            if (!registered) return PersistentIdEvictionDecision.NotRegistered;
            if (ownerSnapshotNull || ownerVesselNull || ownerIsIncomingVessel) return PersistentIdEvictionDecision.EvictStaleOwner;
            return PersistentIdEvictionDecision.KeepForeignOwner;
        }

        public static PersistentIdEvictionDecision DecideLoaded(
            bool registered, bool ownerPartNull, bool ownerVesselNull, bool ownerIsIncomingVessel)
        {
            if (!registered) return PersistentIdEvictionDecision.NotRegistered;
            if (ownerPartNull || ownerVesselNull || ownerIsIncomingVessel) return PersistentIdEvictionDecision.EvictStaleOwner;
            return PersistentIdEvictionDecision.KeepForeignOwner;
        }

        public static bool ShouldRestoreLoadedOwner(bool ownerVesselStillPresent, bool idAlreadyRegistered)
        {
            return ownerVesselStillPresent && !idAlreadyRegistered;
        }

        public static bool IsDanglingVesselRegistration(object owner, Guid vesselId,
            Func<object, Guid> ownerVesselId, Func<object, bool> ownerIsPresent)
        {
            if (owner == null) return false;
            if (ownerVesselId(owner) != vesselId) return false;
            return !ownerIsPresent(owner);
        }
    }
}
