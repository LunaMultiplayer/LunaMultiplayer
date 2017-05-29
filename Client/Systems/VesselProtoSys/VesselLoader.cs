using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Mod;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Enums;
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
            Debug.Log("[LMP]: Loading vessels in subspace 0 into game");
            var numberOfLoads = 0;

            foreach (var vessel in System.AllPlayerVessels)
            {
                var pv = CreateSafeProtoVesselFromConfigNode(vessel.Value.VesselNode, vessel.Key);
                if (pv != null && pv.vesselID == vessel.Key)
                {
                    RegisterServerAsteriodIfVesselIsAsteroid(pv);
                    HighLogic.CurrentGame.flightState.protoVessels.Add(pv);
                    numberOfLoads++;
                }
                else
                {
                    Debug.LogWarning($"[LMP]: Protovessel {vessel.Key} is DAMAGED!. Skipping load.");
                    SystemsContainer.Get<ChatSystem>().PmMessageServer($"WARNING: Protovessel {vessel.Key} is DAMAGED!. Skipping load.");
                }
                vessel.Value.Loaded = true;
            }

            Debug.Log($"[LMP]: {numberOfLoads} Vessels loaded into game");
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
                Debug.LogError($"[LMP]: Error loading vessel: {e}");
            }
        }

        /// <summary>
        /// Performs the operation of actually loading the vessel into the game.  Does not handle errors.
        /// </summary>
        /// <param name="vesselProto"></param>
        private static void LoadVesselImpl(VesselProtoUpdate vesselProto)
        {
            var currentProto = CreateSafeProtoVesselFromConfigNode(vesselProto.VesselNode, vesselProto.VesselId);

            if (ProtoVesselValidationsPassed(currentProto))
            {
                RegisterServerAsteriodIfVesselIsAsteroid(currentProto);

                if (FlightGlobals.FindVessel(vesselProto.VesselId) == null)
                {
                    FixProtoVesselFlags(currentProto);
                    vesselProto.Loaded = LoadVesselIntoGame(currentProto);
                }
            }
        }

        /// <summary>
        /// Reloads an existing vessel into the game
        /// Bear in mind that this method won't reload the vessel unless the part count has changed.
        /// </summary>
        public void ReloadVessel(VesselProtoUpdate vesselProto)
        {
            try
            {
                var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselProto.VesselId);
                if (vessel != null)
                {
                    //Load the existing target, if any.  We will use this to reset the target to the newly loaded vessel, if the vessel we're reloading is the one that is targeted.
                    var currentTarget = FlightGlobals.fetch.VesselTarget;

                    var vesselLoaded = false;
                    var currentProto = CreateSafeProtoVesselFromConfigNode(vesselProto.VesselNode, vesselProto.VesselId);
                    //TODO: Is BackupVessel() needed or can we just look at the protoVessel?
                    if (currentProto.protoPartSnapshots.Count != vessel.BackupVessel().protoPartSnapshots.Count)
                    {
                        //If targeted, unloading the vessel will cause the target to be lost.  We'll have to reset it later.
                        SystemsContainer.Get<VesselRemoveSystem>().UnloadVessel(vessel);
                        LoadVesselImpl(vesselProto);
                        vesselLoaded = true;
                    }

                    //TODO: Handle when it's the active vessel for the FlightGlobals as well.  If you delete the active vessel, it ends badly.  Very badly.

                    //We do want to actually compare by reference--we want to see if the vessel object we're unloading and reloading is the one that's targeted.  If so, we need to
                    //reset the target to the new instance of the vessel
                    if (vesselLoaded && currentTarget?.GetVessel() == vessel)
                    {
                        //Fetch the new vessel information for the same vessel ID, as the unload/load creates a new game object, and we need to refer to the new one for the target
                        var newVessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselProto.VesselId);

                        //Record the time immediately before calling SetVesselTarget
                        var currentTime = Time.realtimeSinceStartup;
                        FlightGlobals.fetch.SetVesselTarget(newVessel, true);

                        var messagesToRemove = new List<ScreenMessage>();
                        //Remove the "Target:" message created by SetVesselTarget
                        foreach (var message in ScreenMessages.Instance.ActiveMessages)
                        {
                            //If the message started on or after the SetVesselTarget call time, remove it, as it's the target message created by SetVesselTarget
                            if (message.startTime >= currentTime)
                            {
                                messagesToRemove.Add(message);
                            }
                        }

                        foreach (var message in messagesToRemove)
                        {
                            ScreenMessages.RemoveMessage(message);
                        }
                    }

                }
                else
                {
                    LoadVesselImpl(vesselProto);
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Error reloading vessel: {e}");
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
                Debug.LogError("[LMP]: protoVessel is null!");
                return false;
            }

            if (vesselProto.situation == Vessel.Situations.FLYING)
            {
                if (vesselProto.orbitSnapShot == null)
                {
                    Debug.Log("[LMP]: Skipping flying vessel load - Protovessel does not have an orbit snapshot");
                    return false;
                }
                var updateBody = FlightGlobals.Bodies[vesselProto.orbitSnapShot.ReferenceBodyIndex];
                if (updateBody == null)
                {
                    Debug.Log("[LMP]: Skipping flying vessel load - Could not find celestial body index {currentProto.orbitSnapShot.ReferenceBodyIndex}");
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
                        Debug.Log($"[LMP]: Flag '{part.flagURL}' doesn't exist, setting to default!");
                        part.flagURL = "Squad/Flags/default";
                    }
                }
            }
        }

        /// <summary>
        /// Creates a protovessel from a ConfigNode
        /// </summary>
        private static ProtoVessel CreateSafeProtoVesselFromConfigNode(ConfigNode inputNode, Guid protoVesselId)
        {
            try
            {
                var pv = new ProtoVessel(inputNode, HighLogic.CurrentGame);
                var cn = new ConfigNode();
                pv.Save(cn);

                foreach (var pps in pv.protoPartSnapshots)
                {
                    if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled &&
                        !SystemsContainer.Get<ModSystem>().AllowedParts.Contains(pps.partName.ToLower()))
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the banned " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        Debug.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        return null;
                    }
                    if (pps.partInfo == null)
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the missing " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        Debug.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing {pps.partName}", 10f,
                            ScreenMessageStyle.UPPER_CENTER);

                        return null;
                    }

                    var missingeResource = pps.resources
                        .FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));

                    if (missingeResource != null)
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) " +
                                  $"contains the missing resource '{missingeResource.resourceName}'!. Skipping load.";

                        Debug.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing the resource " +
                                                         $"{missingeResource.resourceName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                        return null;
                    }
                }
                return pv;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Damaged vessel {protoVesselId}, exception: {e}");
                return null;
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
            Debug.Log($"[LMP]: Loading {currentProto.vesselID}, Name: {currentProto.vesselName}, type: {currentProto.vesselType}");
            currentProto.Load(HighLogic.CurrentGame.flightState);

            if (currentProto.vesselRef == null)
            {
                Debug.Log($"[LMP]: Protovessel {currentProto.vesselID} failed to create a vessel!");
                return false;
            }
            return true;
        }

        #endregion
    }
}