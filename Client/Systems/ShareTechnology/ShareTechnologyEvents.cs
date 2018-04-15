using KSP.UI.Screens;
using LunaClient.Base;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologyEvents : SubSystem<ShareTechnologySystem>
    {
        public void TechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (System.IgnoreEvents || data.target != RDTech.OperationResult.Successful) return;

            LunaLog.Log($"Relaying unlocked tech: {data.host.techID}");
            System.MessageSender.SendTechnologyMessage(data.host);
        }

        public void PartPurchased(AvailablePart part)
        {
            if (System.IgnoreEvents) return;

            //var node = AssetBase.RnDTechTree.GetTreeTechs().ToList().Find(n => n.partsPurchased.Contains(part));  //not performant
            //var rdTech = RDController.Instance.nodes.Find(rd => rd.tech.techID == node.techID).tech;  //not performant
            var rdTech = RDController.Instance.node_selected.tech;   //KSP way of getting it...

            LunaLog.Log($"Relaying part purchased on tech: {rdTech.techID}; part: {part.name}");
            System.MessageSender.SendTechnologyMessage(rdTech);
        }
    }
}
