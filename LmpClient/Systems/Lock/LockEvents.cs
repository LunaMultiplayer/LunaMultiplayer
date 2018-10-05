using LmpClient.Base;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.Lock
{
    public class LockEvents : SubSystem<LockSystem>
    {
        /// <summary>
        /// Triggered when requesting a scene change.
        /// </summary>
        public void OnSceneRequested(GameScenes requestedScene)
        {
            //We must release ALL locks EVEN if we go to tracking station as otherwise we will still keep the control lock!
            if (requestedScene == GameScenes.FLIGHT) return;

            //Always release the Update/UnloadedUpdate lock and the spectate lock
            System.ReleaseAllPlayerVesselLocks();
            VesselCommon.IsSpectating = false;
        }
    }
}
