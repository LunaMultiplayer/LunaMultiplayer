using System.Collections.Concurrent;
using System.Collections.Generic;
using Harmony;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.ShareExperimentalParts
{
    public class ShareExperimentalPartsMessageHandler : SubSystem<ShareExperimentalPartsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ExperimentalPart) return;

            if (msgData is ShareProgressExperimentalPartMsgData data)
            {
                var partName = string.Copy(data.PartName);
                var count = data.Count;
                LunaLog.Log($"Queue ExperimentalPart: part {partName} count {count}");
                System.QueueAction(() =>
                {
                    ExperimentalPart(partName, count);
                });
            }
        }

        private static void ExperimentalPart(string partName, int count)
        {
            System.StartIgnoringEvents();

            var partInfo = PartLoader.getPartInfoByName(partName);
            if (partInfo != null)
            {
                var currentExperimentalParts = Traverse.Create(ResearchAndDevelopment.Instance).Field<Dictionary<AvailablePart, int>>("experimentalPartsStock").Value;

                if (currentExperimentalParts.TryGetValue(partInfo, out var currentCount))
                {
                    if (currentCount > count)
                        ResearchAndDevelopment.RemoveExperimentalPart(partInfo);
                    else if (currentCount < count)
                        ResearchAndDevelopment.AddExperimentalPart(partInfo);
                }
                else
                {
                    ResearchAndDevelopment.AddExperimentalPart(partInfo);
                }
            }

            //Refresh RD nodes in case we are in the RD screen
            if (RDController.Instance && RDController.Instance.partList)
            {
                RDController.Instance.partList.Refresh();
                RDController.Instance.UpdatePanel();
            }

            //Refresh the part list in case we are in the VAB/SPH
            if (EditorPartList.Instance) EditorPartList.Instance.Refresh();
            
            System.StopIgnoringEvents();
            LunaLog.Log($"Experimental part received part: {partName} count {count}");
        }
    }
}
