using LunaClient.Extensions;
using LunaClient.VesselUtilities;
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
        public uint VesselPersistentId;
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

        #endregion

        public void ProcessVesselUpdate()
        {
            var vessel = FlightGlobals.fetch.FindVessel(VesselPersistentId, VesselId);
            if (vessel == null) return;

            var previousSituation = vessel.situation;
            var previousStage = vessel.currentStage;

            UpdateVesselFields(vessel);
            UpdateProtoVesselValues(vessel.protoVessel);

            //Reload the vessel when the situation changes and vessel is unloaded.
            //This will also fix the Landed and Splashed fields
            if (!vessel.loaded && previousSituation != (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), Situation))
            {
                VesselLoader.LoadVessel(vessel.protoVessel);
            }

            //Trigger a reload when staging!
            if (previousStage != Stage)
                VesselLoader.LoadVessel(vessel.protoVessel);
        }

        private void UpdateVesselFields(Vessel vessel)
        {
            vessel.vesselName = Name;
            vessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), Type);
            vessel.distanceTraveled = DistanceTraveled;

            vessel.protoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), Situation);

            //Only change this value if vessel is loaded. When vessel is not loaded we reload it
            if (vessel.loaded)
            {
                vessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), Situation);
                vessel.Landed = Landed;
                vessel.Splashed = Splashed;
            }

            vessel.landedAt = LandedAt;
            vessel.displaylandedAt = DisplayLandedAt;

            vessel.missionTime = MissionTime;
            vessel.launchTime = LaunchTime;
            vessel.lastUT = LastUt;
            vessel.isPersistent = Persistent;
            vessel.referenceTransformId = RefTransformId;

            if (AutoClean)
            {
                vessel.SetAutoClean(AutoCleanReason);
            }

            //vessel.IsControllable = msgData.WasControllable;

            vessel.currentStage = Stage;

            vessel.localCoM.x = Com[0];
            vessel.localCoM.y = Com[1];
            vessel.localCoM.z = Com[2];

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
            }
        }

        private void UpdateProtoVesselValues(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return;

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
        }
    }
}
