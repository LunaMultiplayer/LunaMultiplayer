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
    public class VesselImmortalSystem : Base.System
    {
        #region Fields & properties

        private bool VesselImmortalSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f;

        private static IEnumerable<Vessel> OwnedVessels { get; set; } = new List<Vessel>();
        private static IEnumerable<Vessel> OtherPeopleVessels { get; set; } = new List<Vessel>();

        private static List<Guid> OwnedVesselIds { get; set; } = new List<Guid>();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, UpdateOwnedAndOtherPeopleVesselList));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, MakeOtherPlayerVesselsImmortal));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            OwnedVessels = new Vessel[0];
            OtherPeopleVessels = new Vessel[0];

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
                    .Where(v=> !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(v)));

                OwnedVessels = OwnedVesselIds
                    .Select(FlightGlobals.FindVessel)
                    .Where(v => v != null);

                OtherPeopleVessels = LockSystem.LockQuery.GetAllControlLocks()
                    .Union(LockSystem.LockQuery.GetAllUpdateLocks())
                    .Select(l => l.VesselId)
                    .Except(OwnedVesselIds)
                    .Where(v => !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(v))
                    .Select(FlightGlobals.FindVessel)
                    .Where(v => v != null);
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

                if(FlightGlobals.ActiveVessel != null) //If we are spectating set our own vessel as immortal
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
            foreach (var part in vessel.Parts.Where(p => p.attachJoint != null))
                part.attachJoint.SetUnbreakable(immortal, part.rigidAttachment);
        }

        #endregion
    }
}
