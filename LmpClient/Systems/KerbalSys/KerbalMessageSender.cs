using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.KerbalSys
{
    public class KerbalMessageSender : SubSystem<KerbalSystem>, IMessageSender
    {
        private static ConfigNode ConfigNode { get; } = new ConfigNode();

        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<KerbalCliMsg>(msg)));
        }

        public void SendKerbalRemove(string kerbalName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<KerbalRemoveMsgData>();
            msgData.KerbalName = kerbalName;

            SendMessage(msgData);
        }

        public void SendKerbal(ProtoCrewMember pcm)
        {
            if (pcm == null) return;

            if (VesselCommon.IsSpectating) return;

            ConfigNode.ClearData();
            pcm.Save(ConfigNode);

            var kerbalBytes = ConfigNodeSerializer.Serialize(ConfigNode);
            if (kerbalBytes == null || kerbalBytes.Length == 0)
            {
                LunaLog.LogError("[LMP]: Error sending kerbal - bytes are null or 0");
                return;
            }
            
            SendKerbalProtoMessage(pcm.name, kerbalBytes);
        }

        private void SendKerbalProtoMessage(string kerbalName, byte[] kerbalBytes)
        {
            if (kerbalBytes != null && kerbalBytes.Length > 0)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<KerbalProtoMsgData>();
                msgData.Kerbal.KerbalName = kerbalName;
                msgData.Kerbal.KerbalData = kerbalBytes;
                msgData.Kerbal.NumBytes = kerbalBytes.Length;

                SendMessage(msgData);
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to create byte[] data for kerbal {kerbalName}");
            }
        }
    }
}
