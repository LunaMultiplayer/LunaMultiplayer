using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselImmortalSys
{
    /// <summary>
    /// This class makes the other vessels immortal, this way if we crash against them they are not destroyed but we do.
    /// In the other player screens they will be destroyed and they will send their new vessel definition.
    /// </summary>
    public class VesselImmortalSystem : Base.System<VesselImmortalSystem>
    {
        #region Fields & properties

        private bool VesselImmortalSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f;

        private static List<Vessel> OwnedVessels { get; } = new List<Vessel>();
        private static List<Vessel> OtherPeopleVessels { get; } = new List<Vessel>();

        private static List<Guid> OwnedVesselIds { get; } = new List<Guid>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselImmortalSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, MakeOtherPlayerVesselsImmortal));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, UpdateOwnedAndOtherPeopleVesselList));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            OwnedVessels.Clear();
            OtherPeopleVessels.Clear();

            //In case we disable this system, set all the vessels back as mortal...
            foreach (var vessel in FlightGlobals.Vessels)
            {
                SetVesselImmortalState(vessel, false);
            }
        }

        #endregion

        #region Update methods
        
        /// <summary>
        /// Updates the list of our vessels and other peoples vessel.
        /// We do this in another routine to improve performance
        /// </summary>
        private void UpdateOwnedAndOtherPeopleVesselList()
        {
            if (Enabled && VesselImmortalSystemReady)
            {
                OwnedVesselIds.Clear();
                OwnedVesselIds.AddRange(LockSystem.LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName)
                    .Select(l => l.VesselId)
                    .Union(LockSystem.LockQuery.GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                    .Select(l => l.VesselId))
                    .Where(v=> !VesselRemoveSystem.Singleton.VesselWillBeKilled(v)));

                OwnedVessels.Clear();
                OwnedVessels.AddRange(OwnedVesselIds.Select(FlightGlobals.FindVessel));

                OtherPeopleVessels.Clear();
                OtherPeopleVessels.AddRange(LockSystem.LockQuery.GetAllControlLocks()
                    .Union(LockSystem.LockQuery.GetAllUpdateLocks())
                    .Select(l => l.VesselId)
                    .Except(OwnedVesselIds)
                    .Where(v => !VesselRemoveSystem.Singleton.VesselWillBeKilled(v))
                    .Select(FlightGlobals.FindVessel));
            }
        }

        /// <summary>
        /// Make the other player vessels inmortal
        /// </summary>
        private void MakeOtherPlayerVesselsImmortal()
        {
            if (Enabled && VesselImmortalSystemReady)
            {
                foreach (var vessel in OwnedVessels.Where(v => v != null))
                {
                    SetVesselImmortalState(vessel, false);
                }

                foreach (var vessel in OtherPeopleVessels.Where(v => v != null))
                {
                    SetVesselImmortalState(vessel, true);
                }

                //If we are spectating set our own vessel as immortal
                SetVesselImmortalState(FlightGlobals.ActiveVessel, VesselCommon.IsSpectating);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        private static void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            if (vessel == null) return;

            foreach (var part in vessel.Parts)
            {
                if (part.attachJoint != null)
                    part.attachJoint?.SetUnbreakable(immortal, part.rigidAttachment);

                if(part.collider != null)
                    part.collider.enabled = SettingsSystem.CurrentSettings.CollidersEnabled || !immortal;

                part.gTolerance = immortal ? double.MaxValue : 50;
                part.maxPressure = immortal ? double.MaxValue : 4000;
                part.SetDetectCollisions(!immortal);
            }
        }

        #endregion

        /// <summary>
        /// Call this method when changing the collider settings. It will reset the colliders if we are already in a running game
        /// </summary>
        public void ChangedColliderSettings()
        {
            if (Enabled)
            {
                Enabled = false;
                Enabled = true;
            }
        }
    }
}
