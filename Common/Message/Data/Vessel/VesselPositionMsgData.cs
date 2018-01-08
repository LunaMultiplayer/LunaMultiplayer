using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPositionMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselPositionMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Position;

        //Avoid using reference types in this message as it can generate allocations and is sent VERY often.
        public Guid VesselId;
        public int BodyIndex;
        public double[] LatLonAlt = new double[3];
        public double[] NormalVector = new double[3];
        public double[] Com = new double[3];
        public double[] TransformPosition = new double[3];
        public double[] Velocity = new double[3];
        public double[] Orbit = new double[8];
        public bool Landed;
        public bool Splashed;
        public float[] SrfRelRotation = new float[4];
        public long TimeStamp;

        public override string ClassName { get; } = nameof(VesselPositionMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(BodyIndex);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(LatLonAlt[i]);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(NormalVector[i]);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(Com[i]);
            
            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(TransformPosition[i]);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(Velocity[i]);

            for (var i = 0; i < 8; i++)
                lidgrenMsg.Write(Orbit[i]);

            lidgrenMsg.Write(Landed);
            lidgrenMsg.Write(Splashed);

            for (var i = 0; i < 4; i++)
                lidgrenMsg.Write(SrfRelRotation[i]);

            lidgrenMsg.Write(TimeStamp);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            BodyIndex = lidgrenMsg.ReadInt32();
            
            for (var i = 0; i < 3; i++)
                LatLonAlt[i] = lidgrenMsg.ReadDouble();
            
            for (var i = 0; i < 3; i++)
                NormalVector[i] = lidgrenMsg.ReadDouble();
            
            for (var i = 0; i < 3; i++)
                Com[i] = lidgrenMsg.ReadDouble();
            
            for (var i = 0; i < 3; i++)
                TransformPosition[i] = lidgrenMsg.ReadDouble();
            
            for (var i = 0; i < 3; i++)
                Velocity[i] = lidgrenMsg.ReadDouble();
            
            for (var i = 0; i < 8; i++)
                Orbit[i] = lidgrenMsg.ReadDouble();

            Landed = lidgrenMsg.ReadBoolean();
            Splashed = lidgrenMsg.ReadBoolean();
            
            for (var i = 0; i < 4; i++)
                SrfRelRotation[i] = lidgrenMsg.ReadFloat();

            TimeStamp = lidgrenMsg.ReadInt64();
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + GuidUtil.GetByteSize() + sizeof(int) + sizeof(double) * 3 * 6 + 
                sizeof(bool) * 2 + sizeof(float) * 4 * 1 + sizeof(long);
        }
    }
}