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
        private static ConfigNode ConfigNode { get; } = new ConfigNode();

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

            ConfigNode.ClearData();
            pcm.Save(ConfigNode);

            var kerbalBytes = ConfigNodeSerializer.Serialize(ConfigNode);
            if (kerbalBytes == null || kerbalBytes.Length == 0)
            {
                LunaLog.LogError("[LMP]: Error sending kerbal - bytes are null or 0");
                return;
            }

            var kerbalHash = Common.CalculateSha256Hash(kerbalBytes);;
            if (!System.Kerbals.TryGetValue(pcm.name, out var existingValue) || existingValue.Hash != kerbalHash)
            {
                LunaLog.Log($"[LMP]: Found new/changed kerbal ({pcm.name}), sending...");
                var structure = new KerbalStructure(ConfigNode);
                System.Kerbals.AddOrUpdate(pcm.name, new KerbalStructure(ConfigNode), (key, existingVal) => structure);
                SendKerbalProtoMessage(pcm.name, kerbalBytes);
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
