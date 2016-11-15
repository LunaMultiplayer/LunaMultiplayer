using System;
using System.Linq;
using UnityEngine;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// This class handle the vessel updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselUpdate : VesselPositionUpdate
    {
        #region Fields

        public int Stage { get; set; }

        #region Vessel parts information fields

        public uint[] ActiveEngines { get; set; }
        public uint[] StoppedEngines { get; set; }
        public uint[] Decouplers { get; set; }
        public uint[] AnchoredDecouplers { get; set; }
        public uint[] Clamps { get; set; }
        public uint[] Docks { get; set; }

        #endregion

        #region Vessel position information fields

        public FlightCtrlState FlightState { get; set; }
        public bool[] ActionGrpControls { get; set; }

        #endregion

        #endregion

        #region Creation methods

        public new static VesselUpdate CreateFromVesselId(Guid vesselId)
        {
            var vessel = FlightGlobals.Vessels.FindLast(v => v.id == vesselId);
            return vessel != null ? CreateFromVessel(vessel) : null;
        }

        public new static VesselUpdate CreateFromVessel(Vessel vessel)
        {
            try
            {
                //TODO: This duplicates code in VesselPositionUpdate.cs
                //TODO: Use proper OOP and eliminate the duplicate code through appropriate constructors
                var engines = vessel.FindPartModulesImplementing<ModuleEngines>();
                var returnUpdate = new VesselUpdate
                {
                    VesselId = vessel.id,
                    PlanetTime = Planetarium.GetUniversalTime(),
                    ActiveEngines = engines.Where(e => e.EngineIgnited)
                        .Select(e => e.part.craftID).ToArray(),
                    StoppedEngines = engines.Where(e => !e.EngineIgnited)
                        .Select(e => e.part.craftID).ToArray(),
                    Decouplers = vessel.FindPartModulesImplementing<ModuleDecouple>()
                        .Where(e => !e.isDecoupled)
                        .Select(e => e.part.craftID).ToArray(),
                    AnchoredDecouplers = vessel.FindPartModulesImplementing<ModuleAnchoredDecoupler>()
                        .Where(e => !e.isDecoupled)
                        .Select(e => e.part.craftID).ToArray(),
                    Clamps = vessel.FindPartModulesImplementing<LaunchClamp>()
                        .Select(e => e.part.craftID).ToArray(),
                    Docks = vessel.FindPartModulesImplementing<ModuleDockingNode>()
                        .Where(e => !e.IsDisabled)
                        .Select(e => e.part.craftID).ToArray(),
                    Stage = vessel.currentStage,
                    FlightState = new FlightCtrlState(),
                    BodyName = vessel.mainBody.bodyName,
                    Rotation = new[]
                    {
                        vessel.srfRelRotation.x,
                        vessel.srfRelRotation.y,
                        vessel.srfRelRotation.z,
                        vessel.srfRelRotation.w
                    },
                    ActionGrpControls = new[]
                    {
                        vessel.ActionGroups[KSPActionGroup.Gear],
                        vessel.ActionGroups[KSPActionGroup.Light],
                        vessel.ActionGroups[KSPActionGroup.Brakes],
                        vessel.ActionGroups[KSPActionGroup.SAS],
                        vessel.ActionGroups[KSPActionGroup.RCS]
                    }
                };

                returnUpdate.FlightState.CopyFrom(vessel.ctrlState);

                if (vessel.altitude < 10000)
                {
                    //Use surface position under 10k
                    returnUpdate.IsSurfaceUpdate = true;

                    returnUpdate.Position = new double[]
                    {
                        vessel.latitude,
                        vessel.longitude,
                        vessel.altitude,
                        vessel.radarAltitude
                    };

                    Vector3d srfVel = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
                    returnUpdate.Velocity = new[]
                    {
                        srfVel.x,
                        srfVel.y,
                        srfVel.z
                    };
                }
                else
                {
                    //Use orbital positioning over 10k
                    returnUpdate.IsSurfaceUpdate = false;

                    returnUpdate.Orbit = new[]
                    {
                        vessel.orbit.inclination,
                        vessel.orbit.eccentricity,
                        vessel.orbit.semiMajorAxis,
                        vessel.orbit.LAN,
                        vessel.orbit.argumentOfPeriapsis,
                        vessel.orbit.meanAnomalyAtEpoch,
                        vessel.orbit.epoch
                    };
                }
                return returnUpdate;
            }
            catch (Exception e)
            {
                Debug.Log($"[LMP]: Failed to get vessel update, exception: {e}");
                return null;
            }
        }

        public override VesselPositionUpdate Clone()
        {
            return MemberwiseClone() as VesselUpdate;
        }

        #endregion

        #region Overridden VesselPositionUpdate methods

        /// <summary>
        /// Sets any custom fields defined for this class
        /// </summary>
        protected override void setFieldsDuringInterpolation(Vessel vessel)
        {
            Vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, ActionGrpControls[0]);
            Vessel.ActionGroups.SetGroup(KSPActionGroup.Light, ActionGrpControls[1]);
            Vessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, ActionGrpControls[2]);
            Vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, ActionGrpControls[3]);
            Vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, ActionGrpControls[4]);

            var stage = Vessel.currentStage;

            if (stage != Stage)
            {
                Vessel.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                Vessel.currentStage = Stage;
            }
            else
            {
                var engines = Vessel.FindPartModulesImplementing<ModuleEngines>();
                var enginesToActivate =
                    engines.Where(e => !e.EngineIgnited && ActiveEngines.Contains(e.part.craftID));
                var enginesToStop =
                    engines.Where(e => e.EngineIgnited && StoppedEngines.Contains(e.part.craftID));

                var decouplersToLaunch = Vessel.FindPartModulesImplementing<ModuleDecouple>()
                    .Where(d => !d.isDecoupled && !Decouplers.Contains(d.part.craftID));

                var anchoredDecouplersToLaunch = Vessel.FindPartModulesImplementing<ModuleAnchoredDecoupler>()
                    .Where(d => !d.isDecoupled && !Decouplers.Contains(d.part.craftID));

                var clamps =
                    Vessel.FindPartModulesImplementing<LaunchClamp>()
                        .Where(c => !Clamps.Contains(c.part.craftID));

                var docks =
                    Vessel.FindPartModulesImplementing<ModuleDockingNode>()
                        .Where(d => !d.IsDisabled && !Docks.Contains(d.part.craftID));

                foreach (var engine in enginesToActivate)
                {
                    engine.Activate();
                }

                foreach (var engine in enginesToStop)
                {
                    engine.Shutdown();
                }

                foreach (var decoupler in decouplersToLaunch)
                {
                    decoupler.Decouple();
                }

                foreach (var anchoredDecoupler in anchoredDecouplersToLaunch)
                {
                    anchoredDecoupler.Decouple();
                }

                foreach (var clamp in clamps)
                {
                    clamp.Release();
                }

                foreach (var dock in docks)
                {
                    dock.Decouple();
                }
            }
        }

        /// <summary>
        /// Applies the control state
        /// </summary>
        protected override void ApplyControlState(float percentage)
        {
            var targetFlightState = ((VesselUpdate)Target).FlightState;
            var currentFlightState = Lerp(FlightState, targetFlightState, percentage);

            //Don't interpolate the vessel's control input state.  Apply those immediately so future updates match as closely as possible
            Vessel?.ctrlState.CopyFrom(targetFlightState);
            //Vessel?.FeedInputFeed();

            if (VesselCommon.IsSpectating)
            {
                VesselUpdateSystem.Singleton.FlightState = currentFlightState;
            } else
            {
                VesselUpdateSystem.Singleton.FlightState = null;
            }
        }

        public override VesselPositionUpdateMsgData createVesselUpdateMessage()
        {
            //TODO: This duplicates code in VesselPositionUpdate.cs
            //TODO: Use proper OOP and eliminate the duplicate code through appropriate constructors
            return new VesselUpdateMsgData
            {
                GameSentTime = Time.fixedTime,
                Stage = this.Stage,
                PlanetTime = this.PlanetTime,
                ActiveEngines = this.ActiveEngines,
                StoppedEngines = this.StoppedEngines,
                Decouplers = this.Decouplers,
                AnchoredDecouplers = this.AnchoredDecouplers,
                Clamps = this.Clamps,
                Docks = this.Docks,
                VesselId = this.VesselId,
                ActiongroupControls = this.ActionGrpControls,
                BodyName = this.BodyName,
                GearDown = this.FlightState.gearDown,
                GearUp = this.FlightState.gearUp,
                Headlight = this.FlightState.headlight,
                IsSurfaceUpdate = this.IsSurfaceUpdate,
                KillRot = this.FlightState.killRot,
                MainThrottle = this.FlightState.mainThrottle,
                Orbit = this.Orbit,
                Pitch = this.FlightState.pitch,
                PitchTrim = this.FlightState.pitchTrim,
                Position = this.Position,
                Roll = this.FlightState.roll,
                RollTrim = this.FlightState.rollTrim,
                Rotation = this.Rotation,
                Velocity = this.Velocity,
                WheelSteer = this.FlightState.wheelSteer,
                WheelSteerTrim = this.FlightState.wheelSteerTrim,
                WheelThrottle = this.FlightState.wheelThrottle,
                WheelThrottleTrim = this.FlightState.wheelThrottleTrim,
                X = this.FlightState.X,
                Y = this.FlightState.Y,
                Yaw = this.FlightState.yaw,
                YawTrim = this.FlightState.yawTrim,
                Z = this.FlightState.Z
            };
        }

        #endregion

        #region Private interpolation methods
        /// <summary>
        /// Custom lerp for a flight control state
        /// </summary>
        private static FlightCtrlState Lerp(FlightCtrlState from, FlightCtrlState to, float t)
        {
            return new FlightCtrlState
            {
                X = Mathf.Lerp(from.X, to.X, t),
                Y = Mathf.Lerp(from.Y, to.Y, t),
                Z = Mathf.Lerp(from.Z, to.Z, t),
                gearDown = t < 0.5 ? from.gearDown : to.gearDown,
                gearUp = t < 0.5 ? from.gearUp : to.gearUp,
                headlight = t < 0.5 ? from.headlight : to.headlight,
                killRot = t < 0.5 ? from.killRot : to.killRot,
                mainThrottle = Mathf.Lerp(from.mainThrottle, to.mainThrottle, t),
                pitch = Mathf.Lerp(from.pitch, to.pitch, t),
                roll = Mathf.Lerp(from.roll, to.roll, t),
                yaw = Mathf.Lerp(from.yaw, to.yaw, t),
                pitchTrim = Mathf.Lerp(from.pitchTrim, to.pitchTrim, t),
                rollTrim = Mathf.Lerp(from.rollTrim, to.rollTrim, t),
                yawTrim = Mathf.Lerp(from.yawTrim, to.yawTrim, t),
                wheelSteer = Mathf.Lerp(from.wheelSteer, to.wheelSteer, t),
                wheelSteerTrim = Mathf.Lerp(from.wheelSteerTrim, to.wheelSteerTrim, t),
                wheelThrottle = Mathf.Lerp(from.wheelThrottle, to.wheelThrottle, t),
                wheelThrottleTrim = Mathf.Lerp(from.wheelThrottleTrim, to.wheelThrottleTrim, t),
            };
        }

        #endregion
    }
}