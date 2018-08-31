using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselUpdate
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public string Name;
        public string Type;
        public double DistanceTraveled;
        public string Situation;
        public bool Landed;
        public bool Splashed;
        public bool Persistent;
        public string LandedAt;
        public string DisplayLandedAt;
        public double MissionTime;
        public double LaunchTime;
        public double LastUt;
        public uint RefTransformId;
        public bool AutoClean;
        public string AutoCleanReason;
        public bool WasControllable;
        public int Stage;
        public float[] Com = new float[3];
        public ActionGroup[] ActionGroups = new ActionGroup[17];

        #endregion
        
        public void ProcessVesselUpdate()
        {
            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoValues(this);

            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            UpdateVesselFields(vessel);
            UpdateActionGroups(vessel);
            UpdateProtoVesselValues(vessel.protoVessel);
        }

        private void UpdateVesselFields(Vessel vessel)
        {
            vessel.protoVessel.vesselName = vessel.vesselName = Name;
            vessel.protoVessel.vesselType = vessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), Type);
            vessel.protoVessel.distanceTraveled = vessel.distanceTraveled = DistanceTraveled;

            vessel.protoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), Situation);
            //Only change this value if vessel is loaded. When vessel is not loaded we reload it if the situation changes
            if (vessel.loaded)
                vessel.situation = vessel.protoVessel.situation;

            vessel.protoVessel.landed = Landed;
            //Only change this value if vessel takes off and is loaded. 
            //When the vessel lands the vessel must be reloaded as a whole by the vessel proto system if it's not loaded
            if (vessel.loaded && vessel.Landed && !Landed)
                vessel.Landed = vessel.protoVessel.landed;

            vessel.protoVessel.landedAt = vessel.landedAt = LandedAt;
            vessel.protoVessel.displaylandedAt = vessel.displaylandedAt = DisplayLandedAt;

            vessel.protoVessel.splashed = Splashed;
            //Only change this value if vessel splashes. When the vessel splashes the vessel must be reloaded as a whole by the vessel proto system
            if (vessel.Splashed && !Splashed)
                vessel.Splashed = vessel.protoVessel.splashed;

            vessel.protoVessel.missionTime = vessel.missionTime = MissionTime;
            vessel.protoVessel.launchTime = vessel.launchTime = LaunchTime;
            vessel.protoVessel.lastUT = vessel.lastUT = LastUt;
            vessel.protoVessel.persistent = vessel.isPersistent = Persistent;
            vessel.protoVessel.refTransform = vessel.referenceTransformId = RefTransformId;

            if (AutoClean)
            {
                vessel.SetAutoClean(AutoCleanReason);
            }

            //vessel.IsControllable = msgData.WasControllable;

            vessel.currentStage = Stage;

            vessel.localCoM.x = Com[0];
            vessel.localCoM.y = Com[1];
            vessel.localCoM.z = Com[2];
        }

        private void UpdateActionGroups(Vessel vessel)
        {
            for (var i = 0; i < 17; i++)
            {
                var kspActGrp = (KSPActionGroup)(1 << (i & 31));

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

                if (vessel.ActionGroups.groups[i] != ActionGroups[i].State)
                {
                    vessel.ActionGroups.ToggleGroup(kspActGrp);
                    vessel.ActionGroups.groups[i] = ActionGroups[i].State;
                    vessel.ActionGroups.cooldownTimes[i] = ActionGroups[i].Time;
                }
            }
        }

        private void UpdateProtoVesselValues(ProtoVessel protoVessel)
        {
            if (protoVessel != null)
            {
                protoVessel.vesselName = Name;
                protoVessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), Type);
                protoVessel.distanceTraveled = DistanceTraveled;
                protoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), Situation);
                protoVessel.landed = Landed;
                protoVessel.landedAt = LandedAt;
                protoVessel.displaylandedAt = DisplayLandedAt;
                protoVessel.splashed = Splashed;
                protoVessel.missionTime = MissionTime;
                protoVessel.launchTime = LaunchTime;
                protoVessel.lastUT = LastUt;
                protoVessel.persistent = Persistent;
                protoVessel.refTransform = RefTransformId;
                protoVessel.autoClean = AutoClean;
                protoVessel.autoCleanReason = AutoCleanReason;
                protoVessel.wasControllable = WasControllable;
                protoVessel.stage = Stage;
                protoVessel.CoM.x = Com[0];
                protoVessel.CoM.y = Com[1];
                protoVessel.CoM.z = Com[2];

                for (var i = 0; i < 17; i++)
                {
                    protoVessel.actionGroups.SetValue(ActionGroups[i].ActionGroupName, $"{ActionGroups[i].State}, {ActionGroups[i].Time}");
                }
            }
        }
    }
}
