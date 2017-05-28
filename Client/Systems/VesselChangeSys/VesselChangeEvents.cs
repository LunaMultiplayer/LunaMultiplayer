using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselPositionSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Types;
using System;
using UniLinq;

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
            if (!VesselCommon.IsSpectating && !VesselCommon.ActiveVesselIsInSafetyBubble() && data.vessel.id == FlightGlobals.ActiveVessel.id)
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
            if (!VesselCommon.IsSpectating && !VesselCommon.ActiveVesselIsInSafetyBubble())
            {
                var debrisVessel = FlightGlobals.FindVessel(data.origin.vessel.id);
                var missionId = data.origin.missionID;

                if (!LockSystem.Singleton.LockWithPrefixExists($"debris-{missionId}"))
                {
                    LockSystem.Singleton.AcquireLock($"debris-{missionId}_{debrisVessel.id}");
                    VesselPositionSystem.Singleton.MessageSender.SendVesselPositionUpdate(new VesselPositionUpdate(debrisVessel));
                }
                else
                {
                    var debrisLocks = LockSystem.Singleton.ServerLocks.Where(l => l.Key.StartsWith($"debris-{missionId}"))
                            .Select(l => l.Key.Substring(l.Key.IndexOf('_') + 1)).ToArray();

                    var otherVesselsWIthSameMissionId = FlightGlobals.Vessels
                            .Where(v => v.Parts.Any() && v.Parts.First().missionID == missionId && v.id != debrisVessel.id)
                            .Select(v => v.id.ToString()).ToArray();

                    if (debrisLocks.Length == otherVesselsWIthSameMissionId.Length)
                    {
                        debrisVessel.id = new Guid(debrisLocks.Except(otherVesselsWIthSameMissionId).First());
                    }
                    else
                    {
                        LockSystem.Singleton.AcquireLock($"debris-{missionId}_{debrisVessel.id}");
                        VesselPositionSystem.Singleton.MessageSender.SendVesselPositionUpdate(new VesselPositionUpdate(debrisVessel));
                    }
                }
            }
        }
    }
}