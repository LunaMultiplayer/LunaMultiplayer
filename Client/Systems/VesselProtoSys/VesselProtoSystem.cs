using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// </summary>
    public class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        public List<VesselProtoUpdate> AllPlayerVessels { get; } = new List<VesselProtoUpdate>();

        public float UpdateScreenMessageInterval = 1f;
        public ScreenMessage BannedPartsMessage { get; set; }
        public float LastBannedPartsMessageUpdate { get; set; }
        public VesselLoader VesselLoader { get; } = new VesselLoader();
        private VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public bool CurrentVesselSent { get; set; }
        public bool VesselReady { get; set; } = false;

        private static bool VesselProtoSystemReady => HighLogic.LoadedScene == GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready;


        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFlightReady.Add(VesselProtoEvents.OnFlightReady);
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.OnVesselWasModified);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onFlightReady.Remove(VesselProtoEvents.OnFlightReady);
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.OnVesselWasModified);
        }

        /// <summary>
        /// Here we send our vessel and load other people vessels if they are in our subspace and have updates for them.
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!VesselProtoSystemReady)
                return;

            //Never send the proto vessel if we are in safety bubble!
            if (!CurrentVesselSent && VesselReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
            {
                CurrentVesselSent = true;
                MessageSender.SendVesselProtoMessageApplyPosition(FlightGlobals.ActiveVessel.protoVessel);
            }

            //Load vessels when we have at least 5 updates for them and are in our subspace
            var vesselsToLoad = AllPlayerVessels
                .Where(v => v.HasUpdates && !v.Loaded && VesselWarpSystem.Singleton.GetVesselSubspace(v.VesselId) == WarpSystem.Singleton.CurrentSubspace)
                .ToArray();

            foreach (var vesselProto in vesselsToLoad)
            {
                Client.Singleton.StartCoroutine(VesselLoader.LoadVessel(vesselProto.VesselNode, vesselProto.VesselId));
                vesselProto.Loaded = true;
            }
        }
        
        /// <summary>
        /// Checks the vessel for invalid parts
        /// </summary>
        public bool CheckVessel()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT || 
                FlightGlobals.ActiveVessel == null || 
                !FlightGlobals.ActiveVessel.loaded || 
                VesselLockSystem.Singleton.IsSpectating)
                return false;

            if (ModSystem.Singleton.ModControl != ModControlMode.DISABLED)
            {
                var bannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);

                if (!string.IsNullOrEmpty(bannedPartsStr))
                {
                    if (Time.realtimeSinceStartup - LastBannedPartsMessageUpdate > UpdateScreenMessageInterval)
                    {
                        LastBannedPartsMessageUpdate = Time.realtimeSinceStartup;
                        if (BannedPartsMessage != null)
                            BannedPartsMessage.duration = 0;
                        if (ModSystem.Singleton.ModControl == ModControlMode.ENABLED_STOP_INVALID_PART_SYNC)
                            BannedPartsMessage = ScreenMessages.PostScreenMessage(
                                    "Active vessel contains the following banned parts, it will not be saved to the server:\n" +
                                    bannedPartsStr, 2f, ScreenMessageStyle.UPPER_CENTER);
                        if (ModSystem.Singleton.ModControl == ModControlMode.ENABLED_STOP_INVALID_PART_LAUNCH)
                            BannedPartsMessage = ScreenMessages.PostScreenMessage(
                                    "Active vessel contains the following banned parts, you will be unable to launch on this server:\n" +
                                    bannedPartsStr, 2f, ScreenMessageStyle.UPPER_CENTER);
                    }
                    return false;
                }
            }
            return true;
        }

        private static string GetInvalidVesselParts(Vessel checkVessel)
        {
            var bannedParts = checkVessel.BackupVessel().protoPartSnapshots
                .Where(p => !ModSystem.Singleton.AllowedParts.Contains(p.partName.ToLower())).Distinct().ToArray();

            var bannedPartsStr = bannedParts.Aggregate("", (current, bannedPart) => current + (bannedPart + "\n"));

            Debug.Log($"Checked vessel {checkVessel.id } for banned parts, is ok: {bannedParts.Length == 0}");

            return bannedPartsStr;
        }

        private void RegisterServerAsteriodIfVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            //Register asteroids from other players
            if ((possibleAsteroid.vesselType == VesselType.SpaceObject) &&
                (possibleAsteroid.protoPartSnapshots?.Count == 1) &&
                (possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid"))
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }
    }
}
