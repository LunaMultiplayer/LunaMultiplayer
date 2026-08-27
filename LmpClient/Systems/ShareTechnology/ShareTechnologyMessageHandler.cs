using System.Collections.Concurrent;
using System.Linq;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.ShareTechnology
{
    public class ShareTechnologyMessageHandler : SubSystem<ShareTechnologySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.TechnologyUpdate) return;

            if (msgData is ShareProgressTechnologyMsgData data)
            {
                var tech = new TechNodeInfo(data.TechNode); //create a copy of the tech value so it will not change in the future.
                LunaLog.Log($"Queue TechnologyResearch with: {tech.Id}");
                System.QueueAction(() =>
                {
                    TechnologyResearch(tech);
                });
            }
        }

        private static void TechnologyResearch(TechNodeInfo tech)
        {
            System.StartIgnoringEvents();
            var node = AssetBase.RnDTechTree.GetTreeTechs().ToList().Find(n => n.techID == tech.Id);

            //Check (before we respawn the tree below) whether the R&D screen currently has this exact
            //node's detail panel open. If it does, its Research button will need to be refreshed afterwards.
            var panelWasShowingThisNode = RDController.Instance && RDController.Instance.node_selected != null
                && RDController.Instance.node_selected.tech != null
                && RDController.Instance.node_selected.tech.techID == tech.Id;

            //Unlock the technology
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(node);

            //Refresh the tech tree
            ResearchAndDevelopment.RefreshTechTreeUI();

            //RefreshTechTreeUI() respawns the tree nodes but does NOT refresh an already open node panel.
            //If another player had this node open, its Research button stayed active and let them buy the
            //node again, spending the science a second time (see issue #667). We can't just call UpdatePanel()
            //because the respawn orphans node_selected (its cached RDTech.state is never re-synced), so instead
            //we close the stale panel. The node correctly shows as researched once it is reopened.
            if (panelWasShowingThisNode && RDController.Instance)
            {
                RDController.Instance.node_selected = null;
                RDController.Instance.ShowNothingPanel();
            }

            //Refresh the part list in case we are in the VAB/SPH
            if (EditorPartList.Instance) EditorPartList.Instance.Refresh();

            System.StopIgnoringEvents();
            LunaLog.Log($"TechnologyResearch received - technology researched: {tech.Id}");
        }
    }
}
