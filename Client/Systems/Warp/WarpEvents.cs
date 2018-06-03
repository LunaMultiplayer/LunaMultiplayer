using LunaClient.Base;
using LunaClient.Events;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.Warp
{
    public class WarpEvents : SubSystem<WarpSystem>
    {
        /// <summary>
        /// Event triggered when time warp is changed
        /// </summary>
        public void OnTimeWarpChanged()
        {
            if (TimeWarp.CurrentRateIndex > 0)
            {
                if (!System.WarpValidation() || VesselCommon.IsSpectating)
                    TimeWarp.SetRate(0, true);

                if (System.CurrentSubspace != -1)
                {
                    //We are warping so set the subspace to -1
                    WarpEvent.onTimeWarpStarted.Fire();
                    System.CurrentSubspace = -1;
                }
            }
            //Detecting here if warp has stopped (TimeWarp.CurrentRateIndex == 0 && System.CurrentSubspace == -1) is not reliable so we use a routine to check it
        }

        /// <summary>
        /// Event triggered when scene has changed. We force the player when loading the game to go to the latest subspace
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (System.Enabled && !System.SyncedToLastSubspace && HighLogic.LoadedSceneIsGame)
            {
                System.CurrentSubspace = System.LatestSubspace;
                System.SyncedToLastSubspace = true;
                System.ProcessNewSubspace();
            }
        }
    }
}
