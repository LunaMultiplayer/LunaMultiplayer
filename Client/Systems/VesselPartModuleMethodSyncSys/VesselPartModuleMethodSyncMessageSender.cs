using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPartModuleMethodSyncSys
{
    public class VesselPartModuleSyncMessageSender : SubSystem<VesselPartModuleMethodSyncSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncMsg(Guid vesselId, uint partFlightId, string moduleName, string methodName, FieldNameValue[] fieldVals)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartMethodSyncMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.PartFlightId = partFlightId;
            msgData.ModuleName = moduleName;
            msgData.MethodName = methodName;
            msgData.FieldCount = fieldVals.Length;

            if (msgData.FieldValues.Length < fieldVals.Length)
            {
                msgData.FieldValues = new FieldNameValue[fieldVals.Length];
            }

            for (var i = 0; i < fieldVals.Length; i++)
            {
                if (msgData.FieldValues[i] == null)
                    msgData.FieldValues[i] = new FieldNameValue();

                msgData.FieldValues[i].FieldName = fieldVals[i].FieldName;
                msgData.FieldValues[i].Value = fieldVals[i].Value;
            }

            SendMessage(msgData);
        }
    }
}
