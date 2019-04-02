using LmpClient.Base;
using System;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsEvents : SubSystem<ShareFundsSystem>
    {
        public void FundsChanged(double funds, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Funds changed to: {funds} reason: {reason}");
            System.MessageSender.SendFundsMessage(funds, reason.ToString());
        }

        public void RevertingDetected()
        {
            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void RevertingToEditorDetected(EditorFacility data)
        {
            System.Reverting = true;

            if (System.CurrentShipCost != null)
            {
                Funding.Instance.AddFunds(System.CurrentShipCost.Item2, TransactionReasons.VesselRecovery);
                System.CurrentShipCost = null;
            }
            System.StartIgnoringEvents();
        }

        public void LevelLoaded(GameScenes data)
        {
            if (System.Reverting)
            {
                System.Reverting = false;
                System.StopIgnoringEvents(true);
            }
        }

        public void VesselSwitching(Vessel data0, Vessel data1)
        {
            System.CurrentShipCost = null;
        }

        public void VesselAssembled(Vessel vessel, ShipConstruct construct)
        {
            System.CurrentShipCost = new Tuple<Guid, float>(vessel.id, construct.GetShipCosts(out _, out _));
        }
    }
}
