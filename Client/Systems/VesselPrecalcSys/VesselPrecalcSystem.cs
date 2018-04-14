namespace LunaClient.Systems.VesselPrecalcSys
{
    /// <summary>
    /// This system packs applis a custom precalc to all vessels so they don't do the kill check if they are not yours
    /// </summary>
    public class VesselPrecalcSystem : Base.System<VesselPrecalcSystem>
    {
        #region Fields & properties

        private VesselPrecalcEvents VesselPrecalcEvents { get; } = new VesselPrecalcEvents();

        //Eva vessels must be always packed otherwise we get punchtrough errors.
        public VesselRanges LmpEvaRanges { get; } = new VesselRanges
        {
            //Based on Physics.cfg file
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                pack = 1,//default is 350
                unpack = 0.5f,//default is 200
            },
        };

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPrecalcSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselPrecalcAssign.Add(VesselPrecalcEvents.OnVesselPrecalcAssign);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselPrecalcAssign.Remove(VesselPrecalcEvents.OnVesselPrecalcAssign);
        }

        #endregion
    }
}
