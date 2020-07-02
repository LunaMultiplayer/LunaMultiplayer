using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Enums;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using UnityEngine;

namespace LmpClient.Systems.VesselPartSyncFieldSys
{
    public class TimeToSend
    {
        private readonly int _intervalInMs;
        private DateTime _lastSendTime;

        public TimeToSend(int interval)
        {
            _intervalInMs = interval;
            _lastSendTime = DateTime.MinValue;
        }

        public bool ReadyToSend()
        {
            if (_intervalInMs <= 0) return true;
            if (DateTime.UtcNow - _lastSendTime > TimeSpan.FromMilliseconds(_intervalInMs))
            {
                _lastSendTime = DateTime.UtcNow;
                return true;
            }

            return false;
        }
    }

    public class VesselPartSyncFieldMessageSender : SubSystem<VesselPartSyncFieldSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncFieldBoolMsg(Vessel vessel, Part part, string moduleName, string field, bool value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Boolean;
            msgData.BoolValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldShortMsg(Vessel vessel, Part part, string moduleName, string field, short value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Short;
            msgData.ShortValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldUshortMsg(Vessel vessel, Part part, string moduleName, string field, ushort value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.UShort;
            msgData.UShortValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldIntMsg(Vessel vessel, Part part, string moduleName, string field, int value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Integer;
            msgData.IntValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldUIntMsg(Vessel vessel, Part part, string moduleName, string field, uint value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.UInteger;
            msgData.UIntValue = value;

            SendMessage(msgData);
        }


        public void SendVesselPartSyncFieldFloatMsg(Vessel vessel, Part part, string moduleName, string field, float value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Float;
            msgData.FloatValue = value;

            SendMessage(msgData);
        }



        public void SendVesselPartSyncFieldLongMsg(Vessel vessel, Part part, string moduleName, string field, long value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Long;
            msgData.LongValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldULongMsg(Vessel vessel, Part part, string moduleName, string field, ulong value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.ULong;
            msgData.ULongValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldDoubleMsg(Vessel vessel, Part part, string moduleName, string field, double value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Double;
            msgData.DoubleValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldVectorMsg(Vessel vessel, Part part, string moduleName, string field, Vector3 value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Vector3;
            msgData.VectorValue[0] = value.x;
            msgData.VectorValue[1] = value.y;
            msgData.VectorValue[2] = value.z;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldQuaternionMsg(Vessel vessel, Part part, string moduleName, string field, Quaternion value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Quaternion;
            msgData.QuaternionValue[0] = value.x;
            msgData.QuaternionValue[1] = value.y;
            msgData.QuaternionValue[2] = value.z;
            msgData.QuaternionValue[3] = value.w;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldStringMsg(Vessel vessel, Part part, string moduleName, string field, string value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.String;
            msgData.StrValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldObjectMsg(Vessel vessel, Part part, string moduleName, string field, object value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.String;
            msgData.StrValue = value.ToString();

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldEnumMsg(Vessel vessel, Part part, string moduleName, string field, int value, string valueStr)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Enum;
            msgData.IntValue = value;
            msgData.StrValue = valueStr;

            SendMessage(msgData);
        }

        private static VesselPartSyncFieldMsgData GetBaseMsg(Vessel vessel, Part part, string moduleName, string field)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncFieldMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = part.flightID;
            msgData.ModuleName = moduleName;
            msgData.FieldName = field;

            return msgData;
        }
    }
}
