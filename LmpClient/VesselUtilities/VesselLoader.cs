using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using LmpClient.Extensions;
using LmpClient.Systems.Flag;
using LmpClient.Systems.KscScene;
using LmpClient.Systems.PlayerColorSys;
using LmpClient.Systems.VesselPositionSys;
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
                    GetLatestProtoVesselPosition(vesselProto);

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
        /// Asks the vessel position system for the last known position of a vessel
        /// </summary>
        private static void GetLatestProtoVesselPosition(ProtoVessel vesselProto)
        {
            var fullPosData = VesselPositionSystem.Singleton.GetLatestVesselPosition(vesselProto.vesselID);
            if (fullPosData != null)
            {
                vesselProto.latitude = fullPosData[0];
                vesselProto.longitude = fullPosData[1];
                vesselProto.altitude = fullPosData[2];

                vesselProto.orbitSnapShot.inclination = fullPosData[3];
                vesselProto.orbitSnapShot.eccentricity = fullPosData[4];
                vesselProto.orbitSnapShot.semiMajorAxis = fullPosData[5];
                vesselProto.orbitSnapShot.LAN = fullPosData[6];
                vesselProto.orbitSnapShot.argOfPeriapsis = fullPosData[7];
                vesselProto.orbitSnapShot.meanAnomalyAtEpoch = fullPosData[8];
                vesselProto.orbitSnapShot.epoch = fullPosData[9];
                vesselProto.orbitSnapShot.ReferenceBodyIndex = (int)fullPosData[10];
            }
        }

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

            var reloadingOwnVessel = FlightGlobals.ActiveVessel && vesselProto.vesselID == FlightGlobals.ActiveVessel.id && HighLogic.LoadedSceneIsFlight;

            //In case the vessel exists, silently remove them from unity and recreate it again
            var existingVessel = FlightGlobals.fetch.LmpFindVessel(vesselProto.vesselID);
            if (existingVessel != null)
            {
                if (reloadingOwnVessel)
                {
                    foreach (var part in existingVessel.Parts)
                    {
                        if (part.protoModuleCrew.Any())
                        {
                            //Serialize to avoid modifying the collection
                            var crewMembers = part.protoModuleCrew.ToArray();
                            foreach (var crew in crewMembers)
                            {
                                part.RemoveCrew(crew);
                            }
                        }
                    }
                }

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

                if (vesselProto.vesselRef.GetCrewCount() > 0)
                {
                    foreach (var part in vesselProto.vesselRef.Parts)
                    {
                        if (part.protoModuleCrew.Any())
                        {
                            //Serialize to avoid modifying the collection
                            var crewMembers = part.protoModuleCrew.ToArray();
                            foreach (var crew in crewMembers)
                            {
                                part.RemoveCrew(crew);
                                part.AddCrew(crew);
                            }
                        }
                    }

                    vesselProto.vesselRef.SpawnCrew();
                    if (KerbalPortraitGallery.Instance)
                        KerbalPortraitGallery.Instance.SetActivePortraitsForVessel(FlightGlobals.ActiveVessel);
                }
            }

            return true;
        }

        #endregion
    }
}
