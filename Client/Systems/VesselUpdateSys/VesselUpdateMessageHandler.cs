using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageHandler : SubSystem<VesselUpdateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselUpdateMsgData msgData) || !System.UpdateSystemReady) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoValues(msgData);

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;
            
            UpdateVesselFields(vessel, msgData);
            UpdateActionGroups(vessel, msgData);
            UpdateProtoVesselValues(vessel.protoVessel, msgData);
        }

        private static void UpdateVesselFields(Vessel vessel, VesselUpdateMsgData msgData)
        {
            vessel.protoVessel.vesselName = vessel.vesselName = msgData.Name;
            vessel.protoVessel.vesselType = vessel.vesselType = (VesselType) Enum.Parse(typeof(VesselType), msgData.Type);

            vessel.protoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), msgData.Situation);
            //Only change this value if vessel is loaded. When vessel is not loaded we reload it if the situation changes
            if (vessel.loaded) 
                vessel.situation = vessel.protoVessel.situation;

            vessel.protoVessel.landed = msgData.Landed;
            //Only change this value if vessel takes off and is loaded. 
            //When the vessel lands the vessel must be reloaded as a whole by the vessel proto system if it's not loaded
            if (vessel.loaded && vessel.Landed && !msgData.Landed)
                vessel.Landed = vessel.protoVessel.landed;

            vessel.protoVessel.landedAt = vessel.landedAt = msgData.LandedAt;
            vessel.protoVessel.displaylandedAt = vessel.displaylandedAt = msgData.DisplayLandedAt;

            vessel.protoVessel.splashed = msgData.Splashed;
            //Only change this value if vessel splashes. When the vessel splashes the vessel must be reloaded as a whole by the vessel proto system
            if (vessel.Splashed && !msgData.Splashed)
                vessel.Splashed = vessel.protoVessel.splashed;
            
            vessel.protoVessel.missionTime = vessel.missionTime = msgData.MissionTime;
            vessel.protoVessel.launchTime = vessel.launchTime = msgData.LaunchTime;
            vessel.protoVessel.lastUT = vessel.lastUT = msgData.LastUt;
            vessel.protoVessel.persistent = vessel.isPersistent = msgData.Persistent;
            vessel.protoVessel.refTransform = vessel.referenceTransformId = msgData.RefTransformId;

            if (msgData.AutoClean)
            {
                vessel.SetAutoClean(msgData.AutoCleanReason);
            }

            //vessel.IsControllable = msgData.WasControllable;

            vessel.currentStage = msgData.Stage;

            vessel.localCoM.x = msgData.Com[0];
            vessel.localCoM.y = msgData.Com[1];
            vessel.localCoM.z = msgData.Com[2];
        }

        private static void UpdateActionGroups(Vessel vessel, VesselUpdateMsgData msgData)
        {
            for (var i = 0; i < 17; i++)
            {
                var kspActGrp = (KSPActionGroup) (1 << (i & 31));

                //Ignore SAS if we are spectating as it will fight with the FI
                if (kspActGrp == KSPActionGroup.SAS && VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == vessel.id)
                {
                    if (vessel.ActionGroups[KSPActionGroup.SAS])
                    {
                        vessel.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
                        vessel.ActionGroups.groups[i] = false;
                    }
                    continue;
                }

                if (vessel.ActionGroups.groups[i] != msgData.ActionGroups[i].State)
                {
                    vessel.ActionGroups.ToggleGroup(kspActGrp);
                    vessel.ActionGroups.groups[i] = msgData.ActionGroups[i].State;
                    vessel.ActionGroups.cooldownTimes[i] = msgData.ActionGroups[i].Time;
                }
            }
        }

        private static void UpdateProtoVesselValues(ProtoVessel protoVessel, VesselUpdateMsgData msgData)
        {
            if (protoVessel != null)
            {
                protoVessel.vesselName = msgData.Name;
                protoVessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), msgData.Type);
                protoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), msgData.Situation);
                protoVessel.landed = msgData.Landed;
                protoVessel.landedAt = msgData.LandedAt;
                protoVessel.displaylandedAt = msgData.DisplayLandedAt;
                protoVessel.splashed = msgData.Splashed;
                protoVessel.missionTime = msgData.MissionTime;
                protoVessel.launchTime = msgData.LaunchTime;
                protoVessel.lastUT = msgData.LastUt;
                protoVessel.persistent = msgData.Persistent;
                protoVessel.refTransform = msgData.RefTransformId;
                protoVessel.autoClean = msgData.AutoClean;
                protoVessel.autoCleanReason = msgData.AutoCleanReason;
                protoVessel.wasControllable = msgData.WasControllable;
                protoVessel.stage = msgData.Stage;
                protoVessel.CoM.x = msgData.Com[0];
                protoVessel.CoM.y = msgData.Com[1];
                protoVessel.CoM.z = msgData.Com[2];
                for (var i = 0; i < 17; i++)
                {
                    protoVessel.actionGroups.SetValue(msgData.ActionGroups[i].ActionGroupName, $"{msgData.ActionGroups[i].State}, {msgData.ActionGroups[i].Time}");
                }
            }
        }
    }
}
