using System;

namespace LmpClient.VesselUtilities
{
    public sealed class VesselLoadTransaction
    {
        public enum Phase
        {
            AwaitingLoad,
            LoadSucceeded,
            Committed,
            Failed
        }

        public enum Trigger
        {
            UnchangedEarlyOut,
            ValidationFailed,
            LoadRefNull,
            LoadSucceeded,
            PostLoadFailure,
            FinalSuccess,
            ProtoSwapResolved
        }

        public enum Resolution
        {
            None,
            Continue,
            RollbackOnly,
            RollbackAndCleanUp,
            Commit,
            SwapResolve
        }

        private Phase _phase = Phase.AwaitingLoad;

        public Phase CurrentPhase => _phase;

        public Resolution Resolve(Trigger trigger)
        {
            switch (trigger)
            {
                case Trigger.UnchangedEarlyOut:
                case Trigger.ValidationFailed:
                    if (_phase != Phase.AwaitingLoad) return Resolution.None;
                    _phase = Phase.Failed;
                    return Resolution.RollbackOnly;

                case Trigger.LoadRefNull:
                    if (_phase != Phase.AwaitingLoad) return Resolution.None;
                    _phase = Phase.Failed;
                    return Resolution.RollbackAndCleanUp;

                case Trigger.LoadSucceeded:
                    if (_phase != Phase.AwaitingLoad) return Resolution.None;
                    _phase = Phase.LoadSucceeded;
                    return Resolution.Continue;

                case Trigger.PostLoadFailure:
                    //Valid from before Load (exception inside Load) and after it (post-load failures).
                    if (_phase != Phase.AwaitingLoad && _phase != Phase.LoadSucceeded) return Resolution.None;
                    _phase = Phase.Failed;
                    return Resolution.RollbackAndCleanUp;

                case Trigger.FinalSuccess:
                    //Commit only after Load produced a live vessel AND every post-load step completed.
                    if (_phase != Phase.LoadSucceeded) return Resolution.None;
                    _phase = Phase.Committed;
                    return Resolution.Commit;

                case Trigger.ProtoSwapResolved:
                    if (_phase != Phase.AwaitingLoad) return Resolution.None;
                    _phase = Phase.Committed;
                    return Resolution.SwapResolve;

                default:
                    return Resolution.None;
            }
        }
    }
}
