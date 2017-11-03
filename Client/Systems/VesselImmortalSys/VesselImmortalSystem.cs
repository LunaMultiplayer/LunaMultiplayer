using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
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

        private static List<Vessel> OwnedVessels { get; set; } = new List<Vessel>();
        private static List<Vessel> OtherPeopleVessels { get; set; } = new List<Vessel>();

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
                var ownedVesselIds = LockSystem.LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName)
                    .Select(l => l.VesselId)
                    .Union(LockSystem.LockQuery.GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                    .Select(l => l.VesselId))
                    .Where(v=> !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(v))
                    .ToList();

                OwnedVessels = ownedVesselIds
                    .Select(FlightGlobals.FindVessel)
                    .Where(v => v != null)
                    .ToList();

                OtherPeopleVessels = LockSystem.LockQuery.GetAllControlLocks()
                    .Union(LockSystem.LockQuery.GetAllUpdateLocks())
                    .Select(l => l.VesselId)
                    .Except(ownedVesselIds)
                    .Where(v => !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(v))
                    .Select(FlightGlobals.FindVessel)
                    .Where(v => v != null)
                    .ToList();
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
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        private static void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            vessel.Parts.Where(p => p.attachJoint != null).ToList()
            .ForEach(p => p.attachJoint.SetUnbreakable(immortal, p.rigidAttachment));
        }

        #endregion
    }
}
