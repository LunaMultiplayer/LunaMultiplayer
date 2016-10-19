using LunaClient.Base;
using LunaClient.Systems.VesselLockSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChangeEvents : SubSystem<VesselChangeSystem>
    {
        /// <summary>
        /// Triggered when a part that is in our vessel is about to die
        /// </summary>
        /// <param name="data"></param>
        public void OnPartDie(Part data)
        {
            if (!VesselLockSystem.Singleton.IsSpectating && !VesselCommon.ActiveVesselIsInSafetyBubble() && data.vessel.id == FlightGlobals.ActiveVessel.id)
            {
                var msgData = new VesselChangeMsgData
                {
                    ChangeType = (int)VesselChangeType.EXPLODE,
                    PartCraftId = data.craftID,
                    PartFlightId = data.flightID,
                    VesselId = data.vessel.id
                };

                System.MessageSender.SendMessage(msgData);
            }
        }
        
        /// <summary>
        /// When a stage of ANY vessel is separated, try to get the update lock of that debris
        /// </summary>
        /// <param name="data"></param>
        public void OnStageSeparation(EventReport data)
        {
            if (!VesselLockSystem.Singleton.IsSpectating && !VesselCommon.ActiveVesselIsInSafetyBubble())
            {
                //data.origin.vessel.id = new Guid();
                //This is not going to work as the vessel id's are generated here and not transfered between clients...
                //LockSystem.Singleton.AcquireLock("update-" + data.origin.vessel.id);
            }
        }
    }
}