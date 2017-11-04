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
            //Based on Physics.cfg file
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping),
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying),
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed)
            {
                pack = 50,//default is 350
                unpack = 30,//default is 200
            },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit),
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit),
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit),
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
        };

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselPrecalcAssign.Add(OnVesselAwake);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselPrecalcAssign.Remove(OnVesselAwake);
            foreach (var vessel in FlightGlobals.Vessels)
                vessel.vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
        }

        #endregion
        
        private static void OnVesselAwake(Vessel data)
        {
            data.vesselRanges = LmpRanges;
        }
    }
}