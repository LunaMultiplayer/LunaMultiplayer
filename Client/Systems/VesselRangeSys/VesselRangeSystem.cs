using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;

namespace LunaClient.Systems.VesselRangeSys
{
    /// <summary>
    /// This system packs the other player vessels. 
    /// Theorically this should make the movement better as we won't fight with the flight integrator system,
    /// </summary>
    public class VesselRangeSystem : System<VesselRangeSystem>
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

        public bool VesselRangeSystemReady => VesselProtoSystem.Singleton.ProtoSystemReady;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, PackUnpackVessels));
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Set all other vessels as packed so the movement is better
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
        }

        private static void PackVessel(Vessel vessel)
        {
            vessel.vesselRanges = LmpRanges;
        }

        #endregion
    }
}
