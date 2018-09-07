using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselEvaSys
{
    public class VesselEvaMessageHandler : SubSystem<VesselEvaSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselEvaMsgData msgData) || !System.EvaSystemReady) return;

            //We received a msg for our own controlled vessel so ignore it
            if (LockSystem.LockQuery.ControlLockBelongsToPlayer(msgData.VesselId, SettingsSystem.CurrentSettings.PlayerName))
                return;

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null || !vessel.isEVA) return;

            System.UpdateFsmStateInProtoVessel(vessel.protoVessel, msgData.NewState, msgData.LastBoundStep);

            try
            {
                System.RunEvent(vessel, msgData.NewState, msgData.EventToRun);
            }
            catch (Exception)
            {
                //Ignore the eva animation errors
            }
        }
    }
}
