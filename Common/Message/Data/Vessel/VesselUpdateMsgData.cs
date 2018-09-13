using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselUpdateMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselUpdateMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Update;
        
        public string Name;
        public string Type;
        public double DistanceTraveled;
        public string Situation;
        public bool Landed;
        public bool Splashed;
        public bool Persistent;
        public string LandedAt;
        public string DisplayLandedAt;
        public double MissionTime;
        public double LaunchTime;
        public double LastUt;
        public uint RefTransformId;
        public bool AutoClean;
        public string AutoCleanReason;
        public bool WasControllable;
        public int Stage;
        public float[] Com = new float[3];

        public override string ClassName { get; } = nameof(VesselUpdateMsgData);


        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Name);
            lidgrenMsg.Write(Type);
            lidgrenMsg.Write(DistanceTraveled);
            lidgrenMsg.Write(Situation);
            lidgrenMsg.Write(Landed);
            lidgrenMsg.Write(Splashed);
            lidgrenMsg.Write(Persistent);
            lidgrenMsg.Write(LandedAt);
            lidgrenMsg.Write(DisplayLandedAt);
            lidgrenMsg.Write(MissionTime);
            lidgrenMsg.Write(LaunchTime);
            lidgrenMsg.Write(LastUt);
            lidgrenMsg.Write(RefTransformId);
            lidgrenMsg.Write(AutoClean);
            lidgrenMsg.Write(AutoCleanReason);
            lidgrenMsg.Write(WasControllable);
            lidgrenMsg.Write(Stage);

            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(Com[i]);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Name = lidgrenMsg.ReadString();
            Type = lidgrenMsg.ReadString();
            DistanceTraveled = lidgrenMsg.ReadDouble();
            Situation = lidgrenMsg.ReadString();
            Landed = lidgrenMsg.ReadBoolean();
            Splashed = lidgrenMsg.ReadBoolean();
            Persistent = lidgrenMsg.ReadBoolean();
            LandedAt = lidgrenMsg.ReadString();
            DisplayLandedAt = lidgrenMsg.ReadString();
            MissionTime = lidgrenMsg.ReadDouble();
            LaunchTime = lidgrenMsg.ReadDouble();
            LastUt = lidgrenMsg.ReadDouble();
            RefTransformId = lidgrenMsg.ReadUInt32();
            AutoClean = lidgrenMsg.ReadBoolean();
            AutoCleanReason = lidgrenMsg.ReadString();
            WasControllable = lidgrenMsg.ReadBoolean();
            Stage = lidgrenMsg.ReadInt32();

            for (var i = 0; i < 3; i++)
                Com[i] = lidgrenMsg.ReadFloat();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() 
                + sizeof(double) * 4 + sizeof(bool) * 5 + sizeof(uint) + sizeof(int)
                + Name.GetByteCount() + Type.GetByteCount() + Situation.GetByteCount() 
                + LandedAt.GetByteCount() + DisplayLandedAt.GetByteCount() + AutoCleanReason.GetByteCount();
        }
    }
}
