using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselPositionSys;
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
            if (!VesselCommon.IsSpectating && data.vessel.id == FlightGlobals.ActiveVessel.id)
            {
                var msgData = new VesselChangeMsgData
                {
                    ChangeType = (int)VesselChangeType.Explode,
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
            if (!VesselCommon.IsSpectating)
            {
                var debrisVessel = FlightGlobals.FindVessel(data.origin.vessel.id);

                SystemsContainer.Get<LockSystem>().AcquireUpdateLock(debrisVessel.id, true);
                SystemsContainer.Get<VesselPositionSystem>().MessageSender.SendVesselPositionUpdate(new VesselPositionUpdate(debrisVessel));
            }
        }
    }
}