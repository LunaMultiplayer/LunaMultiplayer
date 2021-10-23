using HarmonyLib;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.ShareCareer;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
                ShareCareerSystem.Singleton.QueueAction(() => ExperimentalPart(partName, count));
            }
        }

        private static void ExperimentalPart(string partName, int count)
        {
            System.StartIgnoringEvents();

            var partInfo = PartLoader.getPartInfoByName(partName);
            if (partInfo != null)
            {
                var currentExperimentalParts = Traverse.Create(ResearchAndDevelopment.Instance).Field<Dictionary<AvailablePart, int>>("experimentalPartsStock").Value;

                //Silently add/remove experimental parts without triggering the event
                if (currentExperimentalParts.TryGetValue(partInfo, out var currentCount))
                {
                    if (count == 0)
                        currentExperimentalParts.Remove(partInfo);
                    else if (currentCount != count)
                        currentExperimentalParts[partInfo] = count;
                }
                else if (count > 0)
                    currentExperimentalParts.Add(partInfo, count);
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
