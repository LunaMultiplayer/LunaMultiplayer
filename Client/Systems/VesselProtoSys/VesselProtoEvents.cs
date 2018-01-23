using LunaClient.Base;
using LunaClient.Systems.Lock;
using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents: SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
        }

        /// <summary>
        /// Called when a vessel is modified. We use it to update our own proto dictionary 
        /// and reflect changes so we don't have to call the "backupvessel" so often
        /// We should not send out own vessel data using this event as this is handled in a routine
        /// </summary>
        public void VesselModified(Vessel data)
        {
            //Perhaps we are shooting stuff at other uncontrolled vessel...
            if (data.id != FlightGlobals.ActiveVessel?.id && !LockSystem.LockQuery.UpdateLockExists(data.id))
            {
                System.MessageSender.SendVesselMessage(data, false);

                if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.id))
                    VesselsProtoStore.AllPlayerVessels[data.id].ProtoVessel = data.BackupVessel();
            }
        }

        /// <summary>
        /// We use this method to detect when a flag has been planted and we are far away from it.
        /// We don't use the onflagplanted event as that is triggered too early and we need to set the id
        /// AFTER we filled the plaque in the flag
        /// </summary>
        /// <param name="data"></param>
        public void VesselGoOnRails(Vessel data)
        {
            if (data.vesselType == VesselType.Flag && data.id == Guid.Empty)
            {
                data.id = Guid.NewGuid();
                data.protoVessel.vesselID = data.id;
                System.MessageSender.SendVesselMessage(data, true);
            }
        }
    }
}
