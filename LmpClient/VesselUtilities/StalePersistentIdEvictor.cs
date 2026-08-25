using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LmpClient.Utilities;

namespace LmpClient.VesselUtilities
{
    public static class StalePersistentIdEvictor
    {
        private sealed class ClaimedPartIdentity
        {
            public uint Id;
            public ProtoPartSnapshot PreviousUnloadedOwner;
            public Part PreviousLoadedOwner;
        }

        private sealed class VesselEvictionRecord
        {
            public readonly List<ClaimedPartIdentity> Claims = new List<ClaimedPartIdentity>();

            public ClaimedPartIdentity FindOrCreateClaim(uint id)
            {
                for (var i = 0; i < Claims.Count; i++)
                    if (Claims[i].Id == id)
                        return Claims[i];

                var claim = new ClaimedPartIdentity { Id = id };
                Claims.Add(claim);
                return claim;
            }
        }

        private static readonly ConcurrentDictionary<Guid, VesselEvictionRecord> EvictionsByVessel
            = new ConcurrentDictionary<Guid, VesselEvictionRecord>();

        private struct ForeignCollisionInfo
        {
            public int Count;
            public string FirstOwnerDescription;
        }

        public static Dictionary<uint, uint> PreflightWireIdentityAndEvict(ConfigNode vesselNode, Guid vesselId)
        {
            var claimedIds = new Dictionary<uint, uint>();
            try
            {
                var partNodes = vesselNode?.GetNodes("PART");
                if (partNodes == null) return claimedIds;

                var foreignInfo = new ForeignCollisionInfo();
                foreach (var partNode in partNodes)
                {
                    var pidText = partNode?.GetValue("persistentId");
                    if (string.IsNullOrEmpty(pidText) || !uint.TryParse(pidText, out var claimedId) || claimedId == 0)
                        continue;

                    uint flightId = 0;
                    var uidText = partNode?.GetValue("uid");
                    if (!string.IsNullOrEmpty(uidText)) uint.TryParse(uidText, out flightId);
                    claimedIds[flightId] = claimedId;

                    //Snapshot owners before evicting so rollback can restore them.
                    var claim = GetRecord(vesselId).FindOrCreateClaim(claimedId);
                    if (claim.PreviousUnloadedOwner == null
                        && FlightGlobals.PersistentUnloadedPartIds.TryGetValue(claimedId, out var prevUnloaded))
                        claim.PreviousUnloadedOwner = prevUnloaded;
                    if (claim.PreviousLoadedOwner == null
                        && FlightGlobals.PersistentLoadedPartIds.TryGetValue(claimedId, out var prevLoaded))
                        claim.PreviousLoadedOwner = prevLoaded;

                    ResolvePartIdCollision(claimedId, vesselId, ref foreignInfo);
                }

                if (foreignInfo.Count > 0)
                    LunaLog.LogWarning($"[LMP]: PersistentId collisions: vessel {vesselId} has {foreignInfo.Count} part ID(s) owned by foreign vessel(s) ({foreignInfo.FirstOwnerDescription}); stock may rename");
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: Wire identity preflight failed for {vesselId} (falling back to stock collision handling): {e.Message}");
            }
            return claimedIds;
        }

        public static void LogWireConstructionResult(ProtoVessel constructed, Dictionary<uint, uint> claimedIds, Guid vesselId)
        {
            try
            {
                var snapshots = constructed?.protoPartSnapshots;
                if (claimedIds == null || snapshots == null) return;

                var renamed = 0;
                foreach (var snapshot in snapshots)
                {
                    if (snapshot == null || !claimedIds.TryGetValue(snapshot.flightID, out var claimedId))
                        continue;

                    if (snapshot.persistentId != claimedId)
                        renamed++;
                }

                if (renamed > 0)
                    LunaLog.LogWarning($"[LMP]: PersistentId renamed: vessel {vesselId} {renamed} part persistentId(s) changed during construction");
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: Wire identity post-log failed for {vesselId}: {e.Message}");
            }
        }

        public static int EvictStalePersistentIds(ProtoVessel vesselProto)
        {
            var evicted = 0;
            try
            {
                var snapshots = vesselProto?.protoPartSnapshots;
                if (snapshots == null) return 0;

                var foreignInfo = new ForeignCollisionInfo();
                for (var i = 0; i < snapshots.Count; i++)
                {
                    var snapshot = snapshots[i];
                    if (snapshot == null || snapshot.persistentId == 0) continue;

                    if (ResolvePartIdCollision(snapshot.persistentId, vesselProto.vesselID, ref foreignInfo)
                        == PersistentIdEvictionDecision.EvictStaleOwner)
                        evicted++;
                }

                if (foreignInfo.Count > 0)
                    LunaLog.LogWarning($"[LMP]: PersistentId collisions: vessel {vesselProto.vesselID} has {foreignInfo.Count} part ID(s) owned by foreign vessel(s) ({foreignInfo.FirstOwnerDescription}); stock may rename");
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: PersistentId eviction failed for {SafeProtoVesselId(vesselProto)} (falling back to stock collision handling): {e.Message}");
            }
            return evicted;
        }

        public static void CommitEvictions(Guid vesselId)
        {
            EvictionsByVessel.TryRemove(vesselId, out _);
        }

        public static void ClearAll()
        {
            EvictionsByVessel.Clear();
        }

        public static void RollbackEvictions(Guid vesselId, ProtoVessel incomingCopy = null)
        {
            if (!EvictionsByVessel.TryRemove(vesselId, out var record))
                return;
            if (HighLogic.CurrentGame == null) return;

            foreach (var claim in record.Claims)
            {
                try
                {
                    var unloaded = FlightGlobals.PersistentUnloadedPartIds;
                    if (unloaded.TryGetValue(claim.Id, out var current)
                        && !ReferenceEquals(current, claim.PreviousUnloadedOwner))
                    {
                        //Non-previous-owner entry is the incoming copy's registration — free the id.
                        unloaded.Remove(claim.Id);
                    }

                    if (claim.PreviousUnloadedOwner != null)
                    {
                        var snapshot = claim.PreviousUnloadedOwner;
                        //Restore only if the owning vessel is still in flightState — a re-placement
                        //may have removed it since the eviction was recorded.
                        if (OwnerStillPresent(snapshot) && !unloaded.ContainsKey(claim.Id))
                            unloaded.Add(claim.Id, snapshot);
                    }

                    if (claim.PreviousLoadedOwner != null)
                    {
                        var part = claim.PreviousLoadedOwner;
                        //Reference search, NOT part.vessel != null: Unity destruction is deferred,
                        //so a doomed part keeps a non-null vessel until frame end.
                        if (PersistentIdEvictionPolicy.ShouldRestoreLoadedOwner(
                                ownerVesselStillPresent: LoadedOwnerVesselStillPresent(part, vesselId),
                                idAlreadyRegistered: FlightGlobals.PersistentLoadedPartIds.ContainsKey(claim.Id)))
                        {
                            FlightGlobals.PersistentLoadedPartIds.Add(claim.Id, part);
                        }
                    }
                }
                catch (Exception e)
                {
                    LunaLog.LogWarning($"[LMP]: PersistentId rollback failed for vessel {vesselId} claimed id {claim.Id}: {e.Message}");
                }
            }

            if (incomingCopy != null)
                RemoveIncomingRenamedRegistrations(incomingCopy);
            else
                RemoveDanglingIncomingRegistrations(vesselId);
        }

        public static void ResolveSwapEvictions(Guid vesselId)
        {
            if (!EvictionsByVessel.TryRemove(vesselId, out var record))
                return;
            if (HighLogic.CurrentGame == null) return;

            foreach (var claim in record.Claims)
            {
                try
                {
                    if (claim.PreviousLoadedOwner == null) continue;

                    var part = claim.PreviousLoadedOwner;
                    if (part != null && part.vessel != null && part.vessel.id == vesselId
                        && !FlightGlobals.PersistentLoadedPartIds.ContainsKey(claim.Id))
                    {
                        FlightGlobals.PersistentLoadedPartIds.Add(claim.Id, part);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.LogWarning($"[LMP]: PersistentId swap-resolve failed for vessel {vesselId} claimed id {claim.Id}: {e.Message}");
                }
            }
        }

        private static bool OwnerStillPresent(ProtoPartSnapshot snapshot)
        {
            if (snapshot?.pVesselRef == null) return false;

            var protoVessels = HighLogic.CurrentGame?.flightState?.protoVessels;
            if (protoVessels == null) return false;

            for (var i = 0; i < protoVessels.Count; i++)
            {
                if (ReferenceEquals(protoVessels[i], snapshot.pVesselRef))
                    return true;
            }
            return false;
        }

        private static bool LoadedOwnerVesselStillPresent(Part part, Guid vesselId)
        {
            if (part == null || part.vessel == null || part.vessel.id != vesselId) return false;

            var vessels = FlightGlobals.Vessels;
            if (vessels == null) return false;

            for (var i = 0; i < vessels.Count; i++)
            {
                if (ReferenceEquals(vessels[i], part.vessel))
                    return true;
            }
            return false;
        }

        private static void RemoveDanglingIncomingRegistrations(Guid vesselId)
        {
            try
            {
                var unloaded = FlightGlobals.PersistentUnloadedPartIds;
                if (unloaded == null || HighLogic.CurrentGame?.flightState?.protoVessels == null) return;

                var idsToRemove = new List<uint>();
                foreach (var id in unloaded.Keys)
                {
                    if (unloaded.TryGetValue(id, out var snapshot)
                        && PersistentIdEvictionPolicy.IsDanglingVesselRegistration(
                            snapshot,
                            vesselId,
                            owner => SafeProtoVesselId(((ProtoPartSnapshot)owner).pVesselRef),
                            owner => OwnerStillPresent((ProtoPartSnapshot)owner)))
                    {
                        idsToRemove.Add(id);
                    }
                }
                for (var i = 0; i < idsToRemove.Count; i++)
                    unloaded.Remove(idsToRemove[i]);
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: PersistentId dangling-owner sweep failed: {e.Message}");
            }
        }

        private static void RemoveIncomingRenamedRegistrations(ProtoVessel incomingCopy)
        {
            try
            {
                var unloaded = FlightGlobals.PersistentUnloadedPartIds;
                var idsToRemove = new List<uint>();
                foreach (var id in unloaded.Keys)
                {
                    if (unloaded.TryGetValue(id, out var snapshot)
                        && ReferenceEquals(snapshot?.pVesselRef, incomingCopy))
                    {
                        idsToRemove.Add(id);
                    }
                }
                for (var i = 0; i < idsToRemove.Count; i++)
                    unloaded.Remove(idsToRemove[i]);
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: PersistentId renamed-id sweep failed: {e.Message}");
            }
        }

        private static PersistentIdEvictionDecision ResolvePartIdCollision(uint id, Guid incomingVesselId, ref ForeignCollisionInfo foreignInfo)
        {
            var overall = PersistentIdEvictionDecision.NotRegistered;

            if (FlightGlobals.PersistentUnloadedPartIds.TryGetValue(id, out var unloadedSnapshot))
            {
                var decision = PersistentIdEvictionPolicy.DecideUnloaded(
                    registered: true,
                    ownerSnapshotNull: unloadedSnapshot == null,
                    ownerVesselNull: unloadedSnapshot != null && unloadedSnapshot.pVesselRef == null,
                    ownerIsIncomingVessel: unloadedSnapshot != null && unloadedSnapshot.pVesselRef != null && unloadedSnapshot.pVesselRef.vesselID == incomingVesselId);

                if (decision == PersistentIdEvictionDecision.EvictStaleOwner)
                {
                    FlightGlobals.PersistentUnloadedPartIds.Remove(id);
                }
                else if (decision == PersistentIdEvictionDecision.KeepForeignOwner)
                {
                    foreignInfo.Count++;
                    if (foreignInfo.FirstOwnerDescription == null)
                        foreignInfo.FirstOwnerDescription = $"proto {SafeProtoVesselId(unloadedSnapshot.pVesselRef)} '{SafeProtoVesselName(unloadedSnapshot.pVesselRef)}'";
                }

                if (decision > overall) overall = decision;
            }

            if (FlightGlobals.PersistentLoadedPartIds.TryGetValue(id, out var loadedPart))
            {
                var decision = PersistentIdEvictionPolicy.DecideLoaded(
                    registered: true,
                    ownerPartNull: loadedPart == null,
                    ownerVesselNull: loadedPart != null && loadedPart.vessel == null,
                    ownerIsIncomingVessel: loadedPart != null && loadedPart.vessel != null && loadedPart.vessel.id == incomingVesselId);

                if (decision == PersistentIdEvictionDecision.EvictStaleOwner)
                {
                    FlightGlobals.PersistentLoadedPartIds.Remove(id);
                }
                else if (decision == PersistentIdEvictionDecision.KeepForeignOwner)
                {
                    foreignInfo.Count++;
                    if (foreignInfo.FirstOwnerDescription == null)
                        foreignInfo.FirstOwnerDescription = $"vessel {SafeVesselId(loadedPart.vessel)} '{SafeVesselName(loadedPart.vessel)}'";
                }

                if (decision > overall) overall = decision;
            }

            return overall;
        }

        private static VesselEvictionRecord GetRecord(Guid vesselId)
        {
            return EvictionsByVessel.GetOrAdd(vesselId, _ => new VesselEvictionRecord());
        }

        private static Guid SafeVesselId(Vessel vessel)
        {
            try { return vessel != null ? vessel.id : Guid.Empty; }
            catch { return Guid.Empty; }
        }

        private static string SafeVesselName(Vessel vessel)
        {
            try { return vessel != null ? vessel.vesselName : "<null>"; }
            catch { return "<error>"; }
        }

        private static Guid SafeProtoVesselId(ProtoVessel vessel)
        {
            try { return vessel != null ? vessel.vesselID : Guid.Empty; }
            catch { return Guid.Empty; }
        }

        private static string SafeProtoVesselName(ProtoVessel vessel)
        {
            try { return vessel != null ? vessel.vesselName : "<null>"; }
            catch { return "<error>"; }
        }
    }
}
