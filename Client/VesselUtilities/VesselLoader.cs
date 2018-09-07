using KSP.UI.Screens;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Flag;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.VesselPositionSys;
using System;
using System.Reflection;
using UniLinq;
using Object = UnityEngine.Object;

namespace LunaClient.VesselUtilities
{
    public class VesselLoader
    {
        public static Guid CurrentlyLoadingVesselId;

        /// <summary>
        /// Invoke this private method to rebuild the vessel lists that appear on the tracking station
        /// </summary>
        private static MethodInfo BuildSpaceTrackingVesselList { get; } = typeof(SpaceTracking).GetMethod("buildVesselsList", BindingFlags.NonPublic | BindingFlags.Instance);

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
                    RegisterServerAsteriodIfVesselIsAsteroid(vesselProto);
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
        /// Registers an asteroid
        /// </summary>
        private static void RegisterServerAsteriodIfVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            //Register asteroids from other players
            if (ProtoVesselIsAsteroid(possibleAsteroid))
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID);
        }

        /// <summary>
        /// Checks if vessel is an asteroid
        /// </summary>
        private static bool ProtoVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            return possibleAsteroid.vesselType == VesselType.SpaceObject &&
                   possibleAsteroid.protoPartSnapshots?.Count == 1 &&
                   possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid";
        }

        /// <summary>
        /// Loads the vessel proto into the current game
        /// </summary>
        private static bool LoadVesselIntoGame(ProtoVessel vesselProto)
        {
            if (HighLogic.CurrentGame?.flightState == null)
                return false;

            var reloadingOwnVessel = vesselProto.vesselID == FlightGlobals.ActiveVessel?.id;

            //In case the vessel exists, silently remove them from unity and recreate it again
            var existingVessel = FlightGlobals.FindVessel(vesselProto.vesselID);
            if (existingVessel != null)
            {
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
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                //When in trackstation rebuild the vessels left panel as otherwise the new vessel won't be listed
                var spaceTracking = Object.FindObjectOfType<SpaceTracking>();
                if (spaceTracking != null)
                    BuildSpaceTrackingVesselList?.Invoke(spaceTracking, null);
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                KSCVesselMarkers.fetch?.RefreshMarkers();

            if (reloadingOwnVessel)
            {
                vesselProto.vesselRef.Load();
                FlightGlobals.fetch.activeVessel = vesselProto.vesselRef;
                OrbitPhysicsManager.CheckReferenceFrame();
                OrbitPhysicsManager.HoldVesselUnpack();
                FlightCamera.SetTarget(vesselProto.vesselRef);
                vesselProto.vesselRef.MakeActive();
            }

            return true;
        }

        #endregion
    }
}
