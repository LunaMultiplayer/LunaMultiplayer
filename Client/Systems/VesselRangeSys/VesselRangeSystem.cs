using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using System.Linq;

namespace LunaClient.Systems.VesselRangeSys
{
    /// <summary>
    /// This system packs the other player vessels. 
    /// Theorically this should make the movement better as we won't fight with the flight integrator system,
    /// </summary>
    public class VesselRangeSystem : Base.System
    {
        #region Fields & properties

        private static VesselRanges LmpRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            }
        };

        public bool VesselRangeSystemReady => SystemsContainer.Get<VesselProtoSystem>().ProtoSystemReady;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(200, RoutineExecution.Update, PackUnpackVessels));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            RemoveRoutines();

            //In case we disable this system, set all the vessels to the normal pack range...
            foreach (var vessel in FlightGlobals.Vessels.Where(v => v.id != FlightGlobals.ActiveVessel.id))
            {
                UnPackVessel(vessel);
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Set all other controlled vessels as packed so the movement is better.
        /// Our vessel must be always unpacked
        /// </summary>
        private void PackUnpackVessels()
        {
            if (Enabled && VesselRangeSystemReady)
            {
                var controlledVessels = VesselCommon.GetControlledVessels();

                foreach (var vessel in FlightGlobals.Vessels.Where(v => v.id != FlightGlobals.ActiveVessel.id))
                {
                    if (controlledVessels.Contains(vessel))
                    {
                        if (!SettingsSystem.CurrentSettings.PackOtherControlledVessels)
                        {
                            UnPackVessel(vessel);
                            continue;
                        }

                        if (!vessel.packed)
                            PackVessel(vessel);
                    }
                    else
                    {
                        UnPackVessel(vessel);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private static void UnPackVessel(Vessel vessel)
        {
            vessel.vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
            vessel.maxControlLevel = Vessel.ControlLevel.FULL;
            //vessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
        }

        private static void PackVessel(Vessel vessel)
        {
            vessel.vesselRanges = LmpRanges;
            vessel.maxControlLevel = Vessel.ControlLevel.NONE;
            vessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.IDLE;
        }

        #endregion
    }
}
