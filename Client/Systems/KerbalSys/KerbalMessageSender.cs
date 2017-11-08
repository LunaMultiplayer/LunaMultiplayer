using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using UniLinq;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalMessageSender : SubSystem<KerbalSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<KerbalCliMsg>(msg)));
        }

        public void SendKerbalIfDifferent(ProtoCrewMember pcm)
        {
            if (pcm.type == ProtoCrewMember.KerbalType.Tourist)
            {
                //Don't send tourists
                LunaLog.Log($"[LMP]: Skipping sending of tourist: {pcm.name}");
                return;
            }

            //TODO: Check if this can be improved as it probably creates a lot of garbage in memory. TIP: VesselNodes can be cleared!
            var kerbalNode = new ConfigNode();
            pcm.Save(kerbalNode);

            var kerbalBytes = ConfigNodeSerializer.Serialize(kerbalNode);
            if (kerbalBytes == null || kerbalBytes.Length == 0)
            {
                LunaLog.LogError("[LMP]: Error sending kerbal - bytes are null or 0");
                return;
            }

            var kerbalHash = Common.CalculateSha256Hash(kerbalBytes);;
            if (!System.Kerbals.TryGetValue(pcm.name, out var existingValue) || existingValue.Hash != kerbalHash)
            {
                LunaLog.Log($"[LMP]: Found new/changed kerbal ({pcm.name}), sending...");
                var structure = new KerbalStructure(kerbalNode);
                System.Kerbals.AddOrUpdate(pcm.name, new KerbalStructure(kerbalNode), (key, existingVal) => structure);
                SendKerbalProtoMessage(pcm.name, kerbalBytes);
            }
        }

        public void SendKerbalsInVessel(ProtoVessel vessel)
        {
            if (vessel?.protoPartSnapshots == null)
                return;

            foreach (var pcm in vessel.protoPartSnapshots.Where(p => p != null).SelectMany(p => p.protoModuleCrew))
                SendKerbalIfDifferent(pcm);
        }

        public void SendKerbalsInVessel(Vessel vessel)
        {
            if (vessel == null)
                return;
            if (vessel.parts == null)
                return;
            foreach (var part in vessel.parts)
            {
                if (part == null)
                    continue;
                foreach (var pcm in part.protoModuleCrew)
                    SendKerbalIfDifferent(pcm);
            }
        }

        private void SendKerbalProtoMessage(string kerbalName, byte[] kerbalBytes)
        {
            if (kerbalBytes != null && kerbalBytes.Length > 0)
            {
                LunaLog.Log("[LMP]: Sending kerbal {kerbalName}");

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<KerbalProtoMsgData>();
                msgData.KerbalName = kerbalName;
                msgData.KerbalData = kerbalBytes;
                msgData.SendTime = Planetarium.GetUniversalTime();

                SendMessage(msgData);
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to create byte[] data for kerbal {kerbalName}");
            }
        }
    }
}
