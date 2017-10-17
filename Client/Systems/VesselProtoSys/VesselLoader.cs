using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionAltSys;
using LunaClient.Systems.VesselRemoveSys;
using System;
using System.Collections.Generic;
using System.IO;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselLoader : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Load all the received vessels from the server into the game
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
                vessel.Value.Loaded = true;
            }

            LunaLog.Log($"[LMP]: {numberOfLoads} Vessels loaded into game");
        }

        /// <summary>
        /// Load a vessel into the game
        /// </summary>
        /// 
        public void LoadVessel(VesselProtoUpdate vesselProto)
        {
            try
            {
                LoadVesselImpl(vesselProto);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error loading vessel: {e}");
            }
        }

        /// <summary>
        /// Performs the operation of actually loading the vessel into the game.  Does not handle errors.
        /// </summary>
        /// <param name="vesselProto"></param>
        private static void LoadVesselImpl(VesselProtoUpdate vesselProto)
        {
            if (ProtoVesselValidationsPassed(vesselProto.ProtoVessel))
            {
                RegisterServerAsteriodIfVesselIsAsteroid(vesselProto.ProtoVessel);

                if (FlightGlobals.FindVessel(vesselProto.VesselId) == null)
                {
                    FixProtoVesselFlags(vesselProto.ProtoVessel);
                    GetLatestProtoVesselPosition(vesselProto);
                    vesselProto.Loaded = LoadVesselIntoGame(vesselProto.ProtoVessel);
                }
            }
        }

        private static void GetLatestProtoVesselPosition(VesselProtoUpdate vesselProto)
        {
            if (SettingsSystem.CurrentSettings.UseAlternativePositionSystem)
            {
                var latLonAlt = SystemsContainer.Get<VesselPositionAltSystem>().GetLatestVesselPosition(vesselProto.VesselId);
                if (latLonAlt.Length == 3)
                {
                    vesselProto.ProtoVessel.latitude = latLonAlt[0];
                    vesselProto.ProtoVessel.longitude = latLonAlt[1];
                    vesselProto.ProtoVessel.altitude = latLonAlt[2];
                }
            }
        }

        /// <summary>
        /// Reloads an existing vessel into the game. We assume that the vessel exists!
        /// We are sure that the vessel has changed as we handle that on the msg receive which runs in another thread
        /// </summary>
        public bool ReloadVessel(VesselProtoUpdate vesselProto)
        {
            try
            {
                //TODO: Handle when it's the active vessel in case we are spectating. If you delete the active vessel, it ends badly.

                //Load the existing target, if any.  We will use this to reset the target to the newly loaded vessel, if the vessel we're reloading is the one that is targeted.
                var currentTargetId = FlightGlobals.fetch.VesselTarget?.GetVessel()?.id;

                //If targeted, unloading the vessel will cause the target to be lost.  We'll have to reset it later.
                SystemsContainer.Get<VesselRemoveSystem>().UnloadVessel(vesselProto.Vessel);
                LoadVesselImpl(vesselProto);

                //Case when the target is the vessel we are changing
                if (currentTargetId == vesselProto.VesselId)
                {
                    //Fetch the new vessel information for the same vessel ID, as the unload/load creates a new game object, 
                    //and we need to refer to the new one for the target
                    var newVessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselProto.VesselId);

                    //Record the time immediately before calling SetVesselTarget to remove it's message
                    var currentTime = Time.realtimeSinceStartup;
                    FlightGlobals.fetch.SetVesselTarget(newVessel, true);

                    RemoveSetTargetMessages(currentTime);
                }
                return true;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error reloading vessel: {e}");
                return false;
            }
        }

        #region Private methods

        /// <summary>
        /// Check if we were spectating the vessel
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static bool SpectatingProtoVessel(ProtoVessel currentProto)
        {
            return FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == currentProto.vesselID;
        }

        /// <summary>
        /// Checks if the protovessel is a target we have locked
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static bool ProtoVesselIsTarget(ProtoVessel currentProto)
        {
            return FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != null &&
                   FlightGlobals.fetch.VesselTarget.GetVessel().id == currentProto.vesselID;
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
                var updateBody = FlightGlobals.Bodies[vesselProto.orbitSnapShot.ReferenceBodyIndex];
                if (updateBody == null)
                {
                    LunaLog.Log("[LMP]: Skipping flying vessel load - Could not find celestial body index {currentProto.orbitSnapShot.ReferenceBodyIndex}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Fixes the flags urls in the vessel
        /// </summary>
        private static void FixProtoVesselFlags(ProtoVessel vesselProto)
        {
            foreach (var part in vesselProto.protoPartSnapshots)
            {
                //Fix up flag URLS.
                if (!string.IsNullOrEmpty(part.flagURL))
                {
                    var flagFile = Path.Combine(Path.Combine(Client.KspPath, "GameData"), $"{part.flagURL}.png");
                    if (!File.Exists(flagFile))
                    {
                        LunaLog.Log($"[LMP]: Flag '{part.flagURL}' doesn't exist, setting to default!");
                        part.flagURL = "Squad/Flags/default";
                    }
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
            return true;
        }

        /// <summary>
        /// This method removes the "Target: xxx" message created by SetVesselTarget
        /// </summary>
        /// <param name="currentTime"></param>
        private static void RemoveSetTargetMessages(float currentTime)
        {
            var messagesToRemove = new List<ScreenMessage>();

            foreach (var message in ScreenMessages.Instance.ActiveMessages.Where(m => m.startTime >= currentTime))
            {
                //If the message started on or after the SetVesselTarget call time, remove it
                messagesToRemove.Add(message);
            }

            foreach (var message in messagesToRemove)
            {
                ScreenMessages.RemoveMessage(message);
            }
        }

        #endregion
    }
}