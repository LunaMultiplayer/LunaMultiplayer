using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalMessageSender : SubSystem<KerbalSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<KerbalCliMsg>(msg));
        }
        
        public void SendKerbalIfDifferent(ProtoCrewMember pcm)
        {
            if (pcm.type == ProtoCrewMember.KerbalType.Tourist)
            {
                //Don't send tourists
                Debug.Log("Skipping sending of tourist: " + pcm.name);
                return;
            }

            var kerbalNode = new ConfigNode();
            pcm.Save(kerbalNode);
            var kerbalBytes = ConfigNodeSerializer.Singleton.Serialize(kerbalNode);
            if ((kerbalBytes == null) || (kerbalBytes.Length == 0))
            {
                Debug.Log("VesselWorker: Error sending kerbal - bytes are null or 0");
                return;
            }
            var kerbalHash = Common.CalculateSha256Hash(kerbalBytes);
            var kerbalDifferent = false;
            if (!System.ServerKerbals.ContainsKey(pcm.name))
            {
                //New kerbal
                Debug.Log("Found new kerbal, sending...");
                kerbalDifferent = true;
            }
            else if (System.ServerKerbals[pcm.name] != kerbalHash)
            {
                Debug.Log("Found changed kerbal (" + pcm.name + "), sending...");
                kerbalDifferent = true;
            }
            if (kerbalDifferent)
            {
                System.ServerKerbals[pcm.name] = kerbalHash;
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
            if ((kerbalBytes != null) && (kerbalBytes.Length > 0))
            {
                Debug.Log("Sending kerbal " + kerbalName);
                var msgData = new KerbalProtoMsgData
                {
                    KerbalName = kerbalName,
                    KerbalData = kerbalBytes,
                    SendTime = Planetarium.GetUniversalTime()
                };
                SendMessage(msgData);
            }
            else
            {
                Debug.Log("Failed to create byte[] data for kerbal " + kerbalName);
            }
        }
    }
}
