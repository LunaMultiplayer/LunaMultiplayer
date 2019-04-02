using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselPositionMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselPositionMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Position;

        //Avoid using reference types in this message as it can generate allocations and is sent VERY often.
        public int BodyIndex;
        public int SubspaceId;
        public float PingSec;
        public float HeightFromTerrain;
        public bool Landed;
        public bool Splashed;
        public bool HackingGravity;
        public double[] LatLonAlt = new double[3];
        public double[] VelocityVector = new double[3];
        public double[] NormalVector = new double[3];
        public float[] SrfRelRotation = new float[4];
        public double[] Orbit = new double[8];

        public override string ClassName { get; } = nameof(VesselPositionMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(BodyIndex);
            lidgrenMsg.Write(SubspaceId);
            lidgrenMsg.Write(PingSec);
            lidgrenMsg.Write(HeightFromTerrain);
            lidgrenMsg.Write(Landed);
            lidgrenMsg.Write(Splashed);
            lidgrenMsg.Write(HackingGravity);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(LatLonAlt[i]);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(VelocityVector[i]);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(NormalVector[i]);

            for (var i = 0; i < 4; i++)
                lidgrenMsg.Write(SrfRelRotation[i]);

            for (var i = 0; i < 8; i++)
                lidgrenMsg.Write(Orbit[i]);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            BodyIndex = lidgrenMsg.ReadInt32();
            SubspaceId = lidgrenMsg.ReadInt32();
            PingSec = lidgrenMsg.ReadFloat();
            HeightFromTerrain = lidgrenMsg.ReadFloat();
            Landed = lidgrenMsg.ReadBoolean();
            Splashed = lidgrenMsg.ReadBoolean();
            HackingGravity = lidgrenMsg.ReadBoolean();

            for (var i = 0; i < 3; i++)
                LatLonAlt[i] = lidgrenMsg.ReadDouble();

            for (var i = 0; i < 3; i++)
                VelocityVector[i] = lidgrenMsg.ReadDouble();

            for (var i = 0; i < 3; i++)
                NormalVector[i] = lidgrenMsg.ReadDouble();

            for (var i = 0; i < 4; i++)
                SrfRelRotation[i] = lidgrenMsg.ReadFloat();

            for (var i = 0; i < 8; i++)
                Orbit[i] = lidgrenMsg.ReadDouble();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) * 2 + sizeof(float) * 2 + sizeof(bool) * 3 + sizeof(double) * 3 * 3 +
                sizeof(float) * 4 * 1 + sizeof(double) * 8;
        }
    }
}
