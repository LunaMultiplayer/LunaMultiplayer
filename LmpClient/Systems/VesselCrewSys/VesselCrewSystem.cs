namespace LmpClient.Systems.VesselCrewSys
{
    /// <summary>
    /// System for handling when a kerbal boards/unboards a vessel or gets transfered
    /// </summary>
    public class VesselCrewSystem : Base.System<VesselCrewSystem>
    {
        #region Fields & properties

        private VesselCrewEvents VesselCrewEvents { get; } = new VesselCrewEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselCrewSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onCrewBoardVessel.Add(VesselCrewEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Add(VesselCrewEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Add(VesselCrewEvents.OnCrewTransfered);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onCrewBoardVessel.Remove(VesselCrewEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Remove(VesselCrewEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Remove(VesselCrewEvents.OnCrewTransfered);
        }

        #endregion
    }
}
