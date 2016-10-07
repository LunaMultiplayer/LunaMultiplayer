using System;
using System.IO;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Mod;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselLoader : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Load all the received vessels from the server into the game
        /// </summary>
        public void LoadVesselsIntoGame()
        {
            LunaLog.Debug("Loading vessels in subspace 0 into game");
            var numberOfLoads = 0;
            
            foreach (var vessel in System.AllPlayerVessels.Where(v=> VesselWarpSystem.Singleton.GetVesselSubspace(v.VesselId) == 0))
            {
                var pv = CreateSafeProtoVesselFromConfigNode(vessel.VesselNode, vessel.VesselId);
                if ((pv != null) && (pv.vesselID == vessel.VesselId))
                {
                    RegisterServerAsteriodIfVesselIsAsteroid(pv);
                    HighLogic.CurrentGame.flightState.protoVessels.Add(pv);
                    numberOfLoads++;
                }
                else
                {
                    LunaLog.Debug($"WARNING: Protovessel {vessel.VesselId} is DAMAGED!. Skipping load.");
                    ChatSystem.Singleton.PmMessageServer($"WARNING: Protovessel {vessel.VesselId} is DAMAGED!. Skipping load.");
                }
                vessel.Loaded = true;
            }
            
            LunaLog.Debug($"{numberOfLoads} Vessels loaded into game");
        }

        /// <summary>
        /// Load a vessel into the game
        /// </summary>
        public void LoadVessel(ConfigNode vesselNode, Guid protovesselId)
        {
            var currentProto = CreateSafeProtoVesselFromConfigNode(vesselNode, protovesselId);

            if (!ProtoVesselValidationsPassed(currentProto))
                return;

            RegisterServerAsteriodIfVesselIsAsteroid(currentProto);
            FixProtoVesselFlags(currentProto);
            DestroyOldVesselIfExists(currentProto);

            LunaLog.Debug("Loading " + currentProto.vesselID + ", Name: " + currentProto.vesselName + ", type: " + currentProto.vesselType);

            currentProto.Load(HighLogic.CurrentGame.flightState);
            if (currentProto.vesselRef == null)
            {
                LunaLog.Debug("Protovessel " + currentProto.vesselID + " failed to create a vessel!");
                return;
            }
            
            if (ProtoVesselIsTarget(currentProto))
            {
                LunaLog.Debug("ProtoVessel update for target vessel!");
                LunaLog.Debug("Set docking target");
                FlightGlobals.fetch.SetVesselTarget(currentProto.vesselRef);
            }

            //If we are spectating that vessel and it changed focus to the new vessel
            if (FlightGlobals.ActiveVessel.id == currentProto.vesselID)
                FlightGlobals.SetActiveVessel(currentProto.vesselRef);

            LunaLog.Debug("Protovessel Loaded");
        }

        #region Private methods

        /// <summary>
        /// As this protovessel is an update over an existing one, this method kills the old vessel
        /// </summary>
        private static void DestroyOldVesselIfExists(ProtoVessel currentProto)
        {
            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => !ProtoVesselIsAsteroid(v.protoVessel) && v.id == currentProto.vesselID);
            if (vessel != null)
            {
                VesselRemoveSystem.Singleton.KillVessel(vessel);
            }
        }

        /// <summary>
        /// Checks if the protovessel is a target we have locked
        /// </summary>
        private static bool ProtoVesselIsTarget(ProtoVessel currentProto)
        {
            if ((FlightGlobals.fetch.VesselTarget != null) && (FlightGlobals.fetch.VesselTarget.GetVessel() != null) &&
                FlightGlobals.fetch.VesselTarget.GetVessel().id == currentProto.vesselID)
                return true;
            return false;
        }

        /// <summary>
        /// Do some basic validations over the protovessel
        /// </summary>
        private static bool ProtoVesselValidationsPassed(ProtoVessel vesselProto)
        {
            if (vesselProto == null)
            {
                LunaLog.Debug("protoVessel is null!");
                return false;
            }

            if (vesselProto.situation == Vessel.Situations.FLYING)
            {
                if (vesselProto.orbitSnapShot == null)
                {
                    LunaLog.Debug("Skipping flying vessel load - Protovessel does not have an orbit snapshot");
                    return false;
                }
                var updateBody = FlightGlobals.Bodies[vesselProto.orbitSnapShot.ReferenceBodyIndex];
                if (updateBody == null)
                {
                    LunaLog.Debug("Skipping flying vessel load - Could not find celestial body index {currentProto.orbitSnapShot.ReferenceBodyIndex}");
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
                    string flagFile = Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"), part.flagURL + ".png");
                    if (!File.Exists(flagFile))
                    {
                        LunaLog.Debug("Flag '" + part.flagURL + "' doesn't exist, setting to default!");
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
                    if (ModSystem.Singleton.ModControl != ModControlMode.DISABLED &&
                        !ModSystem.Singleton.AllowedParts.Contains(pps.partName.ToLower()))
                    {
                        var msg = $"WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the banned " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        LunaLog.Debug(msg);
                        ChatSystem.Singleton.PmMessageServer(msg);

                        return null;
                    }
                    if (pps.partInfo == null)
                    {
                        var msg = $"WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the missing " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        LunaLog.Debug(msg);
                        ChatSystem.Singleton.PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing {pps.partName}", 10f,
                            ScreenMessageStyle.UPPER_CENTER);

                        return null;
                    }

                    var missingeResource = pps.resources
                        .FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));

                    if (missingeResource != null)
                    {
                        var msg = $"WARNING: Protovessel {protoVesselId} ({pv.vesselName}) " +
                                  $"contains the missing resource '{missingeResource.resourceName}'!. Skipping load.";

                        LunaLog.Debug(msg);
                        ChatSystem.Singleton.PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing the resource " +
                                                         $"{missingeResource.resourceName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                        return null;
                    }
                }
                return pv;
            }
            catch (Exception e)
            {
                LunaLog.Debug("Damaged vessel " + protoVesselId + ", exception: " + e);
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
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }

        /// <summary>
        /// Checks if vessel is an asteroid
        /// </summary>
        private static bool ProtoVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            return (possibleAsteroid.vesselType == VesselType.SpaceObject) &&
                   (possibleAsteroid.protoPartSnapshots?.Count == 1) &&
                   (possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid");
        }

        #endregion
    }
}