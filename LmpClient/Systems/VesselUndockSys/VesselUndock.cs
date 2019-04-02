using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselUndockSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselUndock
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public Guid NewVesselId;

        public DockedVesselInfo DockedInfo;

        #endregion

        public void ProcessUndock()
        {
            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            var protoPart = vessel.protoVessel.GetProtoPart(PartFlightId);
            if (protoPart != null)
            {
                if (protoPart.partRef)
                {
                    VesselUndockSystem.Singleton.ManuallyUndockingVesselId = protoPart.partRef.vessel.id;
                    VesselUndockSystem.Singleton.IgnoreEvents = true;

                    protoPart.partRef.Undock(DockedInfo);
                    protoPart.partRef.vessel.id = NewVesselId;

                    LockSystem.Singleton.FireVesselLocksEvents(NewVesselId);

                    //Forcefully set the vessel as immortal
                    protoPart.partRef.vessel.SetImmortal(true);

                    VesselPositionSystem.Singleton.ForceUpdateVesselPosition(NewVesselId);

                    VesselUndockSystem.Singleton.IgnoreEvents = false;
                    VesselUndockSystem.Singleton.ManuallyUndockingVesselId = Guid.Empty;
                }
            }
        }
    }
}
