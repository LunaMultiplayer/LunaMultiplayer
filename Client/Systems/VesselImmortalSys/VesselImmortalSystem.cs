using System;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselImmortalSys
{
    /// <summary>
    /// This class makes the other vessels immortal, this way if we crash against them they are not destroyed but we do.
    /// In the other player screens they will be destroyed and they will send their new vessel definition.
    /// </summary>
    public class VesselImmortalSystem : System<VesselImmortalSystem>
    {
        #region Constructors
        public VesselImmortalSystem() : base()
        {
            setupTimer(VESSEL_IMMORTAL_TIMER_NAME, MakeOtherPlayerVesselsImmortalMSInterval);
        }
        #endregion

        #region Fields & properties

        private bool VesselImmortalSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f;

        private const String VESSEL_IMMORTAL_TIMER_NAME = "IMMORTAL";
        private const int MakeOtherPlayerVesselsImmortalMSInterval = 2000;

        #endregion

        #region base overrides
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!Enabled || !VesselImmortalSystemReady)
            {
                return;
            }

            if ( IsTimeForNextSend(VESSEL_IMMORTAL_TIMER_NAME))
            {
                Profiler.BeginSample("VesselImmortalSystem");
                MakeOtherPlayerVesselsImmortal();
                Profiler.EndSample();
            }
        }
        #endregion

        #region Public methods
        #endregion

        #region Private methods
        /// <summary>
        /// Make the other player vessels inmortal
        /// </summary>
        private void MakeOtherPlayerVesselsImmortal()
        {
            try
            {
                if (VesselImmortalSystemReady)
                {
                    var ownedVessels = LockSystem.Singleton.GetOwnedLocksPrefix("control-").Select(LockSystem.TrimLock)
                        .Union(LockSystem.Singleton.GetLocksWithPrefix("update-").Select(LockSystem.TrimLock))
                        .Select(i => FlightGlobals.FindVessel(new Guid(i)))
                        .Where(v => v != null)
                        .ToArray();

                    var othersPeopleVessels = LockSystem.Singleton.GetLocksWithPrefix("control-").Select(LockSystem.TrimLock)
                        .Union(LockSystem.Singleton.GetLocksWithPrefix("update-").Select(LockSystem.TrimLock))
                        .Except(ownedVessels.Select(v => v.id.ToString()))
                        .Select(i => FlightGlobals.FindVessel(new Guid(i)))
                        //Select the vessels and filter out the nulls
                        .Where(v => v != null).ToArray();

                    foreach (var vessel in ownedVessels)
                    {
                        SetVesselImmortalState(vessel, false);
                    }

                    foreach (var vessel in othersPeopleVessels)
                    {
                        SetVesselImmortalState(vessel, true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Error in coroutine MakeOtherPlayerVesselsInmortal {e}");
            }
        }

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        private static void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            vessel.Parts.Where(p => p.attachJoint != null).ToList()
                .ForEach(p => p.attachJoint.SetUnbreakable(immortal, immortal));
        }

        #endregion
    }
}
