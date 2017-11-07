using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Utilities;
using System;
using System.IO;
using System.Reflection;
using UniLinq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselLoader : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Invoke this private method to rebuild the vessel lists that appear on the tracking station
        /// </summary>
        private static MethodInfo BuildSpaceTrackingVesselList { get; } = typeof(SpaceTracking).GetMethod("buildVesselsList", BindingFlags.NonPublic | BindingFlags.Instance);
        
        /// <summary>
        /// Load all the received vessels from the server into the game
        /// This should be called before the game starts as it only loads them in the scenario
        /// </summary>
        public void LoadVesselsIntoGame()
        {
            LunaLog.Log("[LMP]: Loading vessels in subspace 0 into game");
            var numberOfLoads = 0;

            foreach (var vessel in System.AllPlayerVessels)
            {
                if (vessel.Value.ProtoVessel != null && vessel.Value.ProtoVessel.vesselID == vessel.Key)
                {
                    RegisterServerAsteriodIfVesselIsAsteroid(vessel.Value.ProtoVessel);
                    HighLogic.CurrentGame.flightState.protoVessels.Add(vessel.Value.ProtoVessel);
                    numberOfLoads++;
                }
                else
                {
                    LunaLog.LogWarning($"[LMP]: Protovessel {vessel.Key} is DAMAGED!. Skipping load.");
                    SystemsContainer.Get<ChatSystem>().PmMessageServer($"WARNING: Protovessel {vessel.Key} is DAMAGED!. Skipping load.");
                }
            }

            LunaLog.Log($"[LMP]: {numberOfLoads} Vessels loaded into game");
        }

        /// <summary>
        /// Load a vessel into the game
        /// </summary>
        public bool LoadVessel(ProtoVessel vesselProto)
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
        /// Reloads an existing vessel into the game.
        /// </summary>
        public bool ReloadVessel(ProtoVessel vesselProto)
        {
            try
            {
                //Are we realoading our current active vessel?
                var reloadingCurrentVessel = FlightGlobals.ActiveVessel?.id == vesselProto.vesselID;

                //Load the existing target, if any.  We will use this to reset the target to the newly loaded vessel, if the vessel we're reloading is the one that is targeted.
                var currentTargetId = FlightGlobals.fetch.VesselTarget?.GetVessel()?.id;

                //If targeted, unloading the vessel will cause the target to be lost.  We'll have to reset it later.
                SystemsContainer.Get<VesselRemoveSystem>().UnloadVessel(vesselProto.vesselID);
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
                        SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(vesselProto.vesselID);
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
            var latLonAlt = SystemsContainer.Get<VesselPositionSystem>().GetLatestVesselPosition(vesselProto.vesselID);
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

            if (vesselProto.situation == Vessel.Situations.FLYING)
            {
                if (vesselProto.orbitSnapShot == null)
                {
                    LunaLog.Log("[LMP]: Skipping flying vessel load - Protovessel does not have an orbit snapshot");
                    return false;
                }
                if (FlightGlobals.Bodies == null || FlightGlobals.Bodies.Count < vesselProto.orbitSnapShot.ReferenceBodyIndex)
                {
                    LunaLog.Log("[LMP]: Skipping flying vessel load - Could not find celestial body index " +
                                $"{vesselProto.orbitSnapShot.ReferenceBodyIndex}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Fixes the flags urls in the vessel. The flag have the value as: "Squad/Flags/default" or "LunaMultiPlayer/Flags/mycoolflag" 
        /// </summary>
        private static void FixProtoVesselFlags(ProtoVessel vesselProto)
        {
            foreach (var part in vesselProto.protoPartSnapshots.Where(p=> !string.IsNullOrEmpty(p.flagURL)))
            {
                if (!File.Exists(CommonUtil.CombinePaths(Client.KspPath, "GameData", $"{part.flagURL}.png")))
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
                SystemsContainer.Get<AsteroidSystem>().RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
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
            LunaLog.Log($"[LMP]: Loading {currentProto.vesselID}, Name: {currentProto.vesselName}, type: {currentProto.vesselType}");
            currentProto.Load(HighLogic.CurrentGame.flightState);

            if (currentProto.vesselRef == null)
            {
                LunaLog.Log($"[LMP]: Protovessel {currentProto.vesselID} failed to create a vessel!");
                return false;
            }

            SystemsContainer.Get<PlayerColorSystem>().SetVesselOrbitColor(currentProto.vesselRef);
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                //When in trackstation rebuild the vessels left panel as otherwise the new vessel won't be listed
                var spaceTracking = Object.FindObjectOfType<SpaceTracking>();
                BuildSpaceTrackingVesselList?.Invoke(spaceTracking, null);
            }
            return true;
        }

        /// <summary>
        /// This method removes the "Target: xxx" message created by SetVesselTarget
        /// </summary>
        private static void RemoveSetTargetMessages(float currentTime)
        {                
            //If the message started on or after the SetVesselTarget call time, remove it
            var messagesToRemove = ScreenMessages.Instance.ActiveMessages.Where(m => m.startTime >= currentTime);
            foreach (var message in messagesToRemove)
            {
                ScreenMessages.RemoveMessage(message);
            }
        }

        #endregion
    }
}