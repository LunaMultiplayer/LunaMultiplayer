using System;
using System.Collections;
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
        #region Fields & properties
        
        private bool VesselImmortalSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f;

        private const float MakeOtherPlayerVesselsImmortalSInterval = 2f;

        #endregion

        #region base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(MakeOtherPlayerVesselsImmortal());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Makes a vessel immortal or not for the given amount of seconds.
        /// </summary>
        public void MakeVesselMortalOrImmortal(Vessel vessel, bool immortal, float seconds)
        {
            Client.Singleton.StartCoroutine(MakeVesselImmortalRoutine(vessel, immortal, seconds));
        }

        #endregion

        #region Private methods

        #region Coroutines

        /// <summary>
        /// Coroutine that makes a vessel immortal or mortal for the given amount of seconds
        /// </summary>
        private IEnumerator MakeVesselImmortalRoutine(Vessel vessel, bool immortal, float durationInSeconds)
        {
            var start = Time.time;
            while (true)
            {
                try
                {
                    if (!Enabled || !VesselImmortalSystemReady || Time.time - start > durationInSeconds)
                    {
                        SetVesselImmortalState(vessel, !immortal);
                        break;
                    }

                    SetVesselImmortalState(vessel, immortal);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error in coroutine MakeVesselImmortalRoutine {e}");
                }

                yield return null;
            }
        }

        /// <summary>
        /// Make the other player vessels inmortal
        /// </summary>
        private IEnumerator MakeOtherPlayerVesselsImmortal()
        {
            var seconds = new WaitForSeconds(MakeOtherPlayerVesselsImmortalSInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;
                    if (VesselImmortalSystemReady)
                    {
                        var ownedVessels = LockSystem.Singleton.GetOwnedLocksPrefix("control-").Select(LockSystem.TrimLock)
                            .Union(LockSystem.Singleton.GetLocksWithPrefix("update-").Select(LockSystem.TrimLock))
                            .Select(i => FlightGlobals.FindVessel(new Guid(i)))
                            .Where(v => v != null)
                            .ToArray();

                        var othersPeopleVessels = LockSystem.Singleton.GetLocksWithPrefix("control-").Select(LockSystem.TrimLock)
                            .Union(LockSystem.Singleton.GetLocksWithPrefix("update-").Select(LockSystem.TrimLock))
                            .Except(ownedVessels.Select(v=> v.id.ToString()))
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

                yield return seconds;
            }
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        private static void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            vessel.Parts.Where(p => p.attachJoint != null).ToList()
                .ForEach(p => p.attachJoint.SetUnbreakable(immortal, immortal));
        }

        #endregion

        #endregion
    }
}
