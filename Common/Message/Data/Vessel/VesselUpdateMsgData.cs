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
        public string LandedAt;
        public string DisplayLandedAt;
        public bool Splashed;
        public double MissionTime;
        public double LaunchTime;
        public double LastUt;
        public bool Persistent;
        public uint RefTransformId;
        public bool Controllable;
        public ActionGroup[] ActionGroups = new ActionGroup[17];

        public override string ClassName { get; } = nameof(VesselUpdateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(Name);
            lidgrenMsg.Write(Type);
            lidgrenMsg.Write(Situation);
            lidgrenMsg.Write(Landed);
            lidgrenMsg.Write(LandedAt);
            lidgrenMsg.Write(DisplayLandedAt);
            lidgrenMsg.Write(Splashed);
            lidgrenMsg.Write(MissionTime);
            lidgrenMsg.Write(LaunchTime);
            lidgrenMsg.Write(LastUt);
            lidgrenMsg.Write(Persistent);
            lidgrenMsg.Write(RefTransformId);
            lidgrenMsg.Write(Controllable);

            for (var i = 0; i < 17; i++)
                ActionGroups[i].Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            Name = lidgrenMsg.ReadString();
            Type = lidgrenMsg.ReadString();
            Situation = lidgrenMsg.ReadString();
            Landed = lidgrenMsg.ReadBoolean();
            LandedAt = lidgrenMsg.ReadString();
            DisplayLandedAt = lidgrenMsg.ReadString();
            Splashed = lidgrenMsg.ReadBoolean();
            MissionTime = lidgrenMsg.ReadDouble();
            LaunchTime = lidgrenMsg.ReadDouble();
            LastUt = lidgrenMsg.ReadDouble();
            Persistent = lidgrenMsg.ReadBoolean();
            RefTransformId = lidgrenMsg.ReadUInt32();
            Controllable = lidgrenMsg.ReadBoolean();

            for (var i = 0; i < 17; i++)
            {
                if (ActionGroups[i] == null)
                    ActionGroups[i] = new ActionGroup();

                ActionGroups[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < 17; i++)
            {
                arraySize += ActionGroups[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + GuidUtil.GetByteSize() 
                + sizeof(double) * 3 + sizeof(bool) * 4 + sizeof(uint) 
                + Name.GetByteCount() + Type.GetByteCount() + Situation.GetByteCount() + LandedAt.GetByteCount() + DisplayLandedAt.GetByteCount() +
                + arraySize;
        }
    }
}