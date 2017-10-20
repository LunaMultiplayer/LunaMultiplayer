namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockSystem : Base.System
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartUndock.Add(VesselDockEvents.OnPartUndock);
            GameEvents.onPartCouple.Add(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Add(VesselDockEvents.OnVesselWasModified);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartUndock.Remove(VesselDockEvents.OnPartUndock);
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Remove(VesselDockEvents.OnVesselWasModified);
            GameEvents.onCrewBoardVessel.Remove(VesselDockEvents.OnCrewBoard);
        }
    }
}
