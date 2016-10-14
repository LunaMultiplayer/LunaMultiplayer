using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.Warp;
using UniLinq;

namespace LunaClient.Systems.VesselWarpSys
{
    /// <summary>
    /// This class joins the vessel with the warp system.
    /// When a player warps we move his controlled vessels to the subspace he currently is
    /// When he disconnects, we move his vessels back to subspace 0
    /// </summary>
    public class VesselWarpSystem : System<VesselWarpSystem>
    {
        public Dictionary<Guid, int> VesselSubspaceList { get; } = new Dictionary<Guid, int>();
        private long LastVesselStrandedCheck { get; set; }

        public override void OnDisabled()
        {
            VesselSubspaceList.Clear();
        }

        public override void Update()
        {
            base.Update();
            if (!Enabled) return;
            
            CheckAbandonedVessels();
        }

        public int GetVesselSubspace(Guid vesselId)
        {
            return VesselSubspaceList.ContainsKey(vesselId) ? VesselSubspaceList[vesselId] : 0;
        }

        public void AddUpdateVesselSubspace(Guid vesselId, int subspace)
        {
            if (!VesselSubspaceList.ContainsKey(vesselId))
                VesselSubspaceList.Add(vesselId, subspace);
            else
                VesselSubspaceList[vesselId] = subspace;
        }

        /// <summary>
        /// Check for vessels that are stranded in a subspace with no players and moves them to subspace 0
        /// </summary>
        private void CheckAbandonedVessels()
        {
            if (DateTime.UtcNow.Ticks - LastVesselStrandedCheck > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.StrandedVesselsCheckMsInterval).Ticks)
            {
                LastVesselStrandedCheck = DateTime.UtcNow.Ticks;
                
                var strandedVesselIds = new List<Guid>();
                foreach (var vessel in VesselSubspaceList)
                {
                    var clientsInSubspace = WarpSystem.Singleton.ClientSubspaceList.Count(v => v.Value == vessel.Value);
                    if (clientsInSubspace < 1)
                        strandedVesselIds.Add(vessel.Key);
                }

                foreach (var strandedVessel in strandedVesselIds)
                {
                    VesselSubspaceList[strandedVessel] = 0;
                }
            }
        }

        /// <summary>
        /// Moves all the vessels in control of the player to the new subspace specified
        /// </summary>
        public void MovePlayerVesselsToServerSubspace()
        {
            var controlledVesselIds = LockSystem.Singleton.GetLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => l.StartsWith("control-"))
                .Select(l => new Guid(l.Substring(8)));

            foreach (var controlledVesselId in controlledVesselIds)
            {
                VesselSubspaceList[controlledVesselId] = 0;
            }
        }

        /// <summary>
        /// Moves all the vessels in control of the player to the new subspace specified
        /// </summary>
        public void MovePlayerVesselsToNewSubspace(string playerName, int newSubspace)
        {
            var controlledVesselIds = LockSystem.Singleton.GetLocks(playerName)
                .Where(l => l.StartsWith("control-"))
                .Select(l => new Guid(l.Substring(8)));

            foreach (var controlledVesselId in controlledVesselIds)
            {
                VesselSubspaceList[controlledVesselId] = newSubspace;
            }
        }

        /// <summary>
        /// After syncing the locks and the subspaces we call this method to join both
        /// Bear in mind that control locks are more important than update locks
        /// </summary>
        public void SyncVesselLocksAndSubspaces()
        {
            var controlLocks = LockSystem.Singleton.ServerLocks
                .Where(l => l.Key.StartsWith("control-"))
                .Select(l=> new KeyValuePair<Guid,string>(new Guid(l.Key.Substring(8)), l.Value))
                .ToArray();

            var updateLocks = LockSystem.Singleton.ServerLocks
                .Where(l => l.Key.StartsWith("update-"))
                .Select(l => new KeyValuePair<Guid, string>(new Guid(l.Key.Substring(7)), l.Value))
                .Where(l=> !controlLocks.Any(cl=> cl.Key == l.Key));

            var allLocks = new List<KeyValuePair<Guid, string>>();
            allLocks.AddRange(controlLocks);
            allLocks.AddRange(updateLocks);

            foreach (var playerVesselLock in allLocks)
            {
                var subspace = WarpSystem.Singleton.GetPlayerSubspace(playerVesselLock.Value);
                AddUpdateVesselSubspace(playerVesselLock.Key, subspace);
            }

            //Now set the vessels that don't have any lock to subspace 0
            var abandonedVesselIds = VesselProtoSystem.Singleton.AllPlayerVessels
                .Where(v => !allLocks.Any(l => l.Key == v.VesselId))
                .Select(v => v.VesselId);

            foreach (var abandonedVesselid in abandonedVesselIds)
            {
                AddUpdateVesselSubspace(abandonedVesselid, 0);
            }
        }
    }
}
