using KSP.UI.Screens.Flight;
using LmpClient.Extensions;
using LmpClient.Utilities;
using System;
using Object = UnityEngine.Object;

namespace LmpClient.VesselUtilities
{
    public class VesselLoader
    {
        /// <summary>
        /// Loads/Reloads a vessel into game
        /// </summary>
        public static bool LoadVessel(ProtoVessel vesselProto, bool forceReload)
        {
            try
            {
                if (vesselProto.Validate())
                {
                    return LoadVesselIntoGame(vesselProto, forceReload);
                }

                return false;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error loading vessel: {e}");
                return false;
            }
        }

        #region Private methods
        
        /// <summary>
        /// Loads the vessel proto into the current game
        /// </summary>
        private static bool LoadVesselIntoGame(ProtoVessel vesselProto, bool forceReload)
        {
            if (HighLogic.CurrentGame?.flightState == null)
                return false;

            var reloadingOwnVessel = FlightGlobals.ActiveVessel && vesselProto.vesselID == FlightGlobals.ActiveVessel.id;

            //In case the vessel exists, silently remove them from unity and recreate it again
            var existingVessel = FlightGlobals.FindVessel(vesselProto.vesselID);
            if (existingVessel != null)
            {
                if (existingVessel.Parts.Count == vesselProto.protoPartSnapshots.Count && !forceReload)
                    return true;

                if(reloadingOwnVessel)
                    existingVessel.RemoveAllCrew();

                FlightGlobals.RemoveVessel(existingVessel);
                foreach (var part in existingVessel.parts)
                {
                    Object.Destroy(part.gameObject);
                }
                Object.Destroy(existingVessel.gameObject);
            }

            vesselProto.Load(HighLogic.CurrentGame.flightState);
            if (vesselProto.vesselRef == null)
            {
                LunaLog.Log($"[LMP]: Protovessel {vesselProto.vesselID} failed to create a vessel!");
                return false;
            }

            vesselProto.vesselRef.protoVessel = vesselProto;
            if (vesselProto.vesselRef.isEVA)
            {
                var evaModule = vesselProto.vesselRef.FindPartModuleImplementing<KerbalEVA>();
                if (evaModule != null && evaModule.fsm != null && !evaModule.fsm.Started)
                {
                    evaModule.fsm?.StartFSM("Idle (Grounded)");
                }
                vesselProto.vesselRef.GoOnRails();
            }

            if (vesselProto.vesselRef.situation > Vessel.Situations.PRELAUNCH)
            {
                vesselProto.vesselRef.orbitDriver.updateFromParameters();
            }

            if (double.IsNaN(vesselProto.vesselRef.orbitDriver.pos.x))
            {
                LunaLog.Log($"[LMP]: Protovessel {vesselProto.vesselID} has an invalid orbit");
                return false;
            }

            if (reloadingOwnVessel)
            {
                vesselProto.vesselRef.Load();
                vesselProto.vesselRef.RebuildCrewList();

                //Do not do the setting of the active vessel manually, too many systems are dependant of the events triggered by KSP
                FlightGlobals.ForceSetActiveVessel(vesselProto.vesselRef);

                vesselProto.vesselRef.SpawnCrew();
                foreach (var crew in vesselProto.vesselRef.GetVesselCrew())
                {
                    if (crew.KerbalRef)
                        crew.KerbalRef.state = Kerbal.States.ALIVE;
                }

                CoroutineUtil.StartDelayedRoutine("ReloadOwnVessel", () =>
                {
                    if (KerbalPortraitGallery.Instance.ActiveCrew.Count == 0)
                    {
                        FlightGlobals.ActiveVessel.SpawnCrew();
                        foreach (var kerbal in KerbalPortraitGallery.Instance.ActiveCrew)
                        {
                            kerbal.state = Kerbal.States.ALIVE;
                        }
                        KerbalPortraitGallery.Instance.StartRefresh(FlightGlobals.ActiveVessel);
                    }
                }, 0.5f);
            }

            return true;
        }

        #endregion
    }
}
