using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.ShareFunds;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.SharePurchaseParts
{
    public class SharePurchasePartsMessageHandler : SubSystem<SharePurchasePartsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.PartPurchase) return;

            if (msgData is ShareProgressPartPurchaseMsgData data)
            {
                var techId = string.Copy(data.TechId);
                var partName = string.Copy(data.PartName);
                LunaLog.Log($"Queue PartPurchase with: {techId} part {partName}");
                System.QueueAction(() =>
                {
                    PartPurchase(techId, partName);
                });
            }
        }

        private static void PartPurchase(string techId, string partName)
        {
            System.StartIgnoringEvents();
            ShareFundsSystem.Singleton.StartIgnoringEvents();

            var techState = ResearchAndDevelopment.Instance.GetTechState(techId);
            var partInfo = PartLoader.getPartInfoByName(partName);

            if (techState != null && partInfo != null)
            {
                techState.partsPurchased.Add(partInfo);
                GameEvents.OnPartPurchased.Fire(partInfo);

                //Now buy the identical parts....
                var identicalParts = partInfo.identicalParts.Split(',');
                foreach (var part in identicalParts)
                {
                    if (string.IsNullOrEmpty(part)) continue;

                    var identicalPartInfo = PartLoader.getPartInfoByName(part.Replace('_', '.').Trim());
                    if (identicalPartInfo != null)
                    {
                        identicalPartInfo.costsFunds = false;
                        techState.partsPurchased.Add(identicalPartInfo);
                        GameEvents.OnPartPurchased.Fire(identicalPartInfo);
                        identicalPartInfo.costsFunds = true;
                    }
                }
            }

            //Refresh RD nodes in case we are in the RD screen
            RDController.Instance?.partList?.Refresh();
            RDController.Instance?.UpdatePanel();

            //Refresh the part list in case we are in the VAB/SPH
            EditorPartList.Instance?.Refresh();

            System.StopIgnoringEvents();
            ShareFundsSystem.Singleton.StopIgnoringEvents();
            LunaLog.Log($"Part purchase received tech: {techId} part: {partName}");
        }
    }
}
