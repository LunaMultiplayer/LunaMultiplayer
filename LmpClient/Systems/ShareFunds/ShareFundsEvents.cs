using LmpClient.Base;
using System;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsEvents : SubSystem<ShareFundsSystem>
    {
        public void FundsChanged(double funds, TransactionReasons reason)
        {
            //Capture rollout debit here so revert-to-editor can refund the exact launch cost.
            //Always update LastKnownFunds, even when events are ignored, to keep deltas correct.
            if (System.LastKnownFunds.HasValue)
            {
                var delta = System.LastKnownFunds.Value - funds;
                if (reason == TransactionReasons.VesselRollout && delta > 0)
                {
                    System.CurrentShipCost = new Tuple<Guid, float>(Guid.Empty, (float)delta);
                }
            }
            System.LastKnownFunds = funds;

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
            //Re-seed tracker because scene loads can change funds without firing OnFundsChanged.
            if (Funding.Instance != null)
                System.LastKnownFunds = Funding.Instance.Funds;

            if (System.Reverting)
            {
                System.Reverting = false;
                System.StopIgnoringEvents(true);
            }
        }

        public void VesselSwitching(Vessel data0, Vessel data1)
        {
            //Keep pending launch refund during revert flows; clear it on normal vessel switches.
            if (System.Reverting) return;
            System.CurrentShipCost = null;
        }
    }
}
