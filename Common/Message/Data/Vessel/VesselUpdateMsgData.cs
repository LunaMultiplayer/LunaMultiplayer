using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselUpdateMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselUpdateMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Update;
        
        public Guid VesselId;
        public string Name;
        public string Type;
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

        public ActionGroup[] ActionGroups = new ActionGroup[17];

        public override string ClassName { get; } = nameof(VesselUpdateMsgData);


        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(Name);
            lidgrenMsg.Write(Type);
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

            for (var i = 0; i < 17; i++)
                ActionGroups[i].Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            Name = lidgrenMsg.ReadString();
            Type = lidgrenMsg.ReadString();
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

            for (var i = 0; i < 17; i++)
            {
                if (ActionGroups[i] == null)
                    ActionGroups[i] = new ActionGroup();

                ActionGroups[i].Deserialize(lidgrenMsg);
            }
        }
        
        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < 17; i++)
            {
                arraySize += ActionGroups[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + GuidUtil.GetByteSize() 
                + sizeof(double) * 3 + sizeof(bool) * 5 + sizeof(uint) + sizeof(int)
                + Name.GetByteCount() + Type.GetByteCount() + Situation.GetByteCount() 
                + LandedAt.GetByteCount() + DisplayLandedAt.GetByteCount() + AutoCleanReason.GetByteCount()
                + arraySize;
        }
    }
}