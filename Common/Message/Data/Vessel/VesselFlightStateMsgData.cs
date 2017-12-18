using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselFlightStateMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselFlightStateMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Flightstate;

        //Avoid using reference types in this message as it can generate allocations and is sent VERY often.
        public Guid VesselId;
        public float MainThrottle;
        public float WheelThrottleTrim;
        public float X;
        public float Y;
        public float Z;
        public bool KillRot;
        public bool GearUp;
        public bool GearDown;
        public bool Headlight;
        public float WheelThrottle;
        public float Pitch;
        public float Roll;
        public float Yaw;
        public float PitchTrim;
        public float RollTrim;
        public float YawTrim;
        public float WheelSteer;
        public float WheelSteerTrim;
        public long TimeStamp;

        public override string ClassName { get; } = nameof(VesselFlightStateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(MainThrottle);
            lidgrenMsg.Write(WheelThrottle);
            lidgrenMsg.Write(WheelThrottleTrim);
            lidgrenMsg.Write(X);
            lidgrenMsg.Write(Y);
            lidgrenMsg.Write(Z);
            lidgrenMsg.Write(KillRot);
            lidgrenMsg.Write(GearUp);
            lidgrenMsg.Write(GearDown);
            lidgrenMsg.Write(Headlight);
            lidgrenMsg.Write(Pitch);
            lidgrenMsg.Write(Roll);
            lidgrenMsg.Write(Yaw);
            lidgrenMsg.Write(PitchTrim);
            lidgrenMsg.Write(RollTrim);
            lidgrenMsg.Write(YawTrim);
            lidgrenMsg.Write(WheelSteer);
            lidgrenMsg.Write(WheelSteerTrim);
            lidgrenMsg.Write(TimeStamp);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            MainThrottle = lidgrenMsg.ReadFloat();
            WheelThrottle = lidgrenMsg.ReadFloat();
            WheelThrottleTrim = lidgrenMsg.ReadFloat();
            X = lidgrenMsg.ReadFloat();
            Y = lidgrenMsg.ReadFloat();
            Z = lidgrenMsg.ReadFloat();
            KillRot = lidgrenMsg.ReadBoolean();
            GearUp = lidgrenMsg.ReadBoolean();
            GearDown = lidgrenMsg.ReadBoolean();
            Headlight = lidgrenMsg.ReadBoolean();
            Pitch = lidgrenMsg.ReadFloat();
            Roll = lidgrenMsg.ReadFloat();
            Yaw = lidgrenMsg.ReadFloat();
            PitchTrim = lidgrenMsg.ReadFloat();
            RollTrim = lidgrenMsg.ReadFloat();
            YawTrim = lidgrenMsg.ReadFloat();
            WheelSteer = lidgrenMsg.ReadFloat();
            WheelSteerTrim = lidgrenMsg.ReadFloat();
            TimeStamp = lidgrenMsg.ReadInt64();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) +
                   GuidUtil.GetByteSize() + sizeof(float) * 14 + sizeof(bool) * 4 + sizeof(long);
        }
    }
}