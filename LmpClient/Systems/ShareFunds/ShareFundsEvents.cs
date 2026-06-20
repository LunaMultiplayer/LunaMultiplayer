using LmpClient.Base;
using System;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsEvents : SubSystem<ShareFundsSystem>
    {
        public void FundsChanged(double funds, TransactionReasons reason)
        {
            //Track the running delta of funds. When KSP charges for a launch it fires OnFundsChanged
            //with reason = VesselRollout *before* ShipConstruction.AssembleForLaunch runs, so this is
            //the only place we can reliably observe the exact amount debited. Snapshot that amount on
            //CurrentShipCost so RevertingToEditorDetected can refund precisely what KSP took.
            //
            //Do the delta bookkeeping regardless of IgnoreEvents, otherwise incoming server-pushed
            //SetFunds (routed through SetFundsWithoutTriggeringEvent) would leave LastKnownFunds stale
            //and the *next* real event's delta would be wrong.
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
            //Re-seed LastKnownFunds from whatever the scene just loaded so the very next
            //OnFundsChanged computes a correct delta. Stock KSP scenario loads set funds
            //directly without firing OnFundsChanged, which would otherwise leave our
            //tracker stale and miss the first VesselRollout after a scene transition.
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
            //Don't clear CurrentShipCost while a revert is in progress. Revert-to-launch reloads
            //the launchpad save and fires onVesselSwitching as part of that reload, but the
            //vessel on the pad still has its launch debit outstanding - a subsequent
            //revert-to-editor still needs to refund it. Clearing here would cause the refund
            //to be silently skipped and RestoreState() to then overwrite KSP's save-reloaded
            //pre-launch funds with a stale post-launch value.
            //
            //Outside of a revert, the existing guard stands: switching to an unrelated vessel
            //(e.g. from the tracking station) should drop the pending refund so a later revert
            //doesn't inappropriately credit the player.
            if (System.Reverting) return;
            System.CurrentShipCost = null;
        }
    }
}
