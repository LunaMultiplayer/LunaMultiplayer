using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using LmpClient.Extensions;
using LmpClient.Systems.Flag;
using LmpClient.Systems.KscScene;
using LmpClient.Systems.PlayerColorSys;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.Utilities;
using System;
using UniLinq;
using Object = UnityEngine.Object;

namespace LmpClient.VesselUtilities
{
    public class VesselLoader
    {
        public static Guid CurrentlyLoadingVesselId { get; private set; }
        
        /// <summary>
        /// Loads/reloads a vessel into game
        /// </summary>
        public static bool LoadVessel(ProtoVessel vesselProto)
        {
            try
            {
                if (ProtoVesselValidationsPassed(vesselProto))
                {
                    CurrentlyLoadingVesselId = vesselProto.vesselID;
                    FixProtoVesselFlags(vesselProto);

                    return LoadVesselIntoGame(vesselProto);
                }

                return false;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error loading vessel: {e}");
                return false;
            }
            finally
            {
                CurrentlyLoadingVesselId = Guid.Empty;
            }
        }

        #region Private methods

        /// <summary>
        /// Do some basic validations over the protovessel
        /// </summary>
        private static bool ProtoVesselValidationsPassed(ProtoVessel vesselProto)
        {
            if (vesselProto == null)
            {
                LunaLog.LogError("[LMP]: protoVessel is null!");
                return false;
            }

            if (vesselProto.vesselID == Guid.Empty)
            {
                LunaLog.LogError("[LMP]: protoVessel id is null!");
                return false;
            }

            if (vesselProto.situation == Vessel.Situations.FLYING)
            {
                if (vesselProto.orbitSnapShot == null)
                {
                    LunaLog.LogWarning("[LMP]: Skipping flying vessel load - Protovessel does not have an orbit snapshot");
                    return false;
                }
                if (FlightGlobals.Bodies == null || FlightGlobals.Bodies.Count < vesselProto.orbitSnapShot.ReferenceBodyIndex)
                {
                    LunaLog.LogWarning($"[LMP]: Skipping flying vessel load - Could not find celestial body index {vesselProto.orbitSnapShot.ReferenceBodyIndex}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Fixes the flags urls in the vessel. The flag have the value as: "Squad/Flags/default" or "LunaMultiplayer/Flags/mycoolflag" 
        /// </summary>
        private static void FixProtoVesselFlags(ProtoVessel vesselProto)
        {
            foreach (var part in vesselProto.protoPartSnapshots.Where(p => !string.IsNullOrEmpty(p.flagURL)))
            {
                if (!FlagSystem.Singleton.FlagExists(part.flagURL))
                {
                    LunaLog.Log($"[LMP]: Flag '{part.flagURL}' doesn't exist, setting to default!");
                    part.flagURL = "Squad/Flags/default";
                }
            }
        }

        /// <summary>
        /// Loads the vessel proto into the current game
        /// </summary>
        private static bool LoadVesselIntoGame(ProtoVessel vesselProto)
        {
            if (HighLogic.CurrentGame?.flightState == null)
                return false;

            var reloadingOwnVessel = FlightGlobals.ActiveVessel && vesselProto.vesselID == FlightGlobals.ActiveVessel.id;

            //In case the vessel exists, silently remove them from unity and recreate it again
            var existingVessel = FlightGlobals.FindVessel(vesselProto.vesselID);
            if (existingVessel != null)
            {
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

            PlayerColorSystem.Singleton.SetVesselOrbitColor(vesselProto.vesselRef);
            KscSceneSystem.Singleton.RefreshTrackingStationVessels();
            if (KSCVesselMarkers.fetch) KSCVesselMarkers.fetch.RefreshMarkers();

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
