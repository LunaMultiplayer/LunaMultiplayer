using KSP.UI.Screens;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UniLinq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaClient.VesselUtilities
{
    public class VesselLoader
    {
        public static Guid ReloadingVesselId { get; set; }

        /// <summary>
        /// Here we hold all the messages of "Target: xxx" message created by SetVesselTarget to remove them when we reload a vessel and set
        /// back the target to it
        /// </summary>
        private static List<ScreenMessage> MessagesToRemove { get; } = new List<ScreenMessage>();

        /// <summary>
        /// Invoke this private method to rebuild the vessel lists that appear on the tracking station
        /// </summary>
        private static MethodInfo BuildSpaceTrackingVesselList { get; } = typeof(SpaceTracking).GetMethod("buildVesselsList", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Load a vessel into the game
        /// </summary>
        public static bool LoadVessel(ProtoVessel vesselProto)
        {
            try
            {
                return LoadVesselImpl(vesselProto);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error loading vessel: {e}");
            }

            return false;
        }

        /// <summary>
        /// Reloads an existing vessel into the game. Try to avoid this method as possible. It has really bad performance and
        /// works really bad if you are reloading your own current vessel...
        /// </summary>
        public static bool ReloadVessel(ProtoVessel vesselProto)
        {
            LunaLog.Log($"Reloading vessel {vesselProto.vesselID}");
            ReloadingVesselId = vesselProto.vesselID;
            try
            {
                //Are we realoading our current active vessel?
                var reloadingCurrentVessel = FlightGlobals.ActiveVessel?.id == vesselProto.vesselID;

                //Load the existing target, if any.  We will use this to reset the target to the newly loaded vessel, if the vessel we're reloading is the one that is targeted.
                var currentTargetId = FlightGlobals.fetch.VesselTarget?.GetVessel()?.id;

                //If targeted, unloading the vessel will cause the target to be lost.  We'll have to reset it later.
                //Bear in mind that UnloadVessel will trigger VesselRemoveEvents.OnVesselWillDestroy!! So be sure to set ReloadingVesselId correctly
                VesselRemoveSystem.Singleton.UnloadVessel(vesselProto.vesselID);
                if (LoadVesselImpl(vesselProto))
                {
                    //Case when the target is the vessel we are changing
                    if (currentTargetId == vesselProto.vesselID)
                    {
                        //Record the time immediately before calling SetVesselTarget to remove it's message
                        var currentTime = Time.realtimeSinceStartup;
                        FlightGlobals.fetch.SetVesselTarget(vesselProto.vesselRef, true);

                        RemoveSetTargetMessages(currentTime);
                    }

                    if (reloadingCurrentVessel)
                    {
                        OrbitPhysicsManager.HoldVesselUnpack();
                        FlightGlobals.fetch.activeVessel.GoOnRails();
                        VesselSwitcherSystem.Singleton.SwitchToVessel(vesselProto.vesselID);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error reloading vessel: {e}");
                return false;
            }
            finally
            {
                ReloadingVesselId = Guid.Empty;
            }
        }

        #region Private methods

        /// <summary>
        /// Performs the operation of actually loading the vessel into the game.  Does not handle errors.
        /// </summary>
        /// <param name="vesselProto"></param>
        private static bool LoadVesselImpl(ProtoVessel vesselProto)
        {
            if (ProtoVesselValidationsPassed(vesselProto))
            {
                if (FlightGlobals.FindVessel(vesselProto.vesselID) == null)
                {
                    RegisterServerAsteriodIfVesselIsAsteroid(vesselProto);
                    FixProtoVesselFlags(vesselProto);
                    GetLatestProtoVesselPosition(vesselProto);
                    return LoadVesselIntoGame(vesselProto);
                }

                LunaLog.LogError($"A vessel with id '{vesselProto.vesselID}' already exists. Cannot load the same vessel again");
            }

            return false;
        }

        /// <summary>
        /// Asks the vessel position system for the last known position of a vessel
        /// </summary>
        private static void GetLatestProtoVesselPosition(ProtoVessel vesselProto)
        {
            var latLonAlt = VesselPositionSystem.Singleton.GetLatestVesselPosition(vesselProto.vesselID);
            if (latLonAlt != null)
            {
                vesselProto.latitude = latLonAlt[0];
                vesselProto.longitude = latLonAlt[1];
                vesselProto.altitude = latLonAlt[2];
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
                if (!File.Exists(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", $"{part.flagURL}.png")))
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
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
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
        private static bool LoadVesselIntoGame(ProtoVessel currentProto)
        {
            if (HighLogic.CurrentGame?.flightState == null)
                return false;

            LunaLog.Log($"[LMP]: Loading {currentProto.vesselID}, Name: {currentProto.vesselName}, type: {currentProto.vesselType}");
            currentProto.Load(HighLogic.CurrentGame.flightState);

            if (currentProto.vesselRef == null)
            {
                LunaLog.Log($"[LMP]: Protovessel {currentProto.vesselID} failed to create a vessel!");
                return false;
            }

            if (currentProto.vesselRef.isEVA)
            {
                var evaModule = currentProto.vesselRef.FindPartModuleImplementing<KerbalEVA>();
                if (evaModule != null && evaModule.fsm != null && !evaModule.fsm.Started)
                {
                    evaModule.fsm?.StartFSM("Idle (Grounded)");
                }
                currentProto.vesselRef.GoOnRails();
            }
            currentProto.vesselRef.orbitDriver?.updateFromParameters();

            PlayerColorSystem.Singleton.SetVesselOrbitColor(currentProto.vesselRef);
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                //When in trackstation rebuild the vessels left panel as otherwise the new vessel won't be listed
                var spaceTracking = Object.FindObjectOfType<SpaceTracking>();
                if (spaceTracking != null)
                    BuildSpaceTrackingVesselList?.Invoke(spaceTracking, null);
            }

            KSCVesselMarkers.fetch?.RefreshMarkers();

            return true;
        }

        /// <summary>
        /// This method removes the "Target: xxx" message created by SetVesselTarget
        /// </summary>
        private static void RemoveSetTargetMessages(float currentTime)
        {
            MessagesToRemove.Clear();

            //If the message started on or after the SetVesselTarget call time, remove it
            MessagesToRemove.AddRange(ScreenMessages.Instance.ActiveMessages.Where(m => m.startTime >= currentTime));

            foreach (var message in MessagesToRemove)
            {
                ScreenMessages.RemoveMessage(message);
            }
        }

        #endregion
    }
}
