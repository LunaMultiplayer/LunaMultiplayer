using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.Warp
{
    public class WarpEvents : SubSystem<WarpSystem>
    {
        /// <summary>
        /// Event triggered when time warp is changed
        /// </summary>
        public void OnTimeWarpChanged()
        {
            var resetWarp = SettingsSystem.ServerSettings.WarpMode == WarpMode.None ||
                            SettingsSystem.ServerSettings.WarpMode == WarpMode.Master && SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName;

            if (TimeWarp.CurrentRateIndex > 0)
            {
                if (resetWarp)
                {
                    if (SettingsSystem.ServerSettings.WarpMode == WarpMode.None)
                        System.DisplayMessage("Cannot warp, warping is disabled on this server", 5f);

                    TimeWarp.SetRate(0, true);
                    return;
                }

                if (System.CurrentSubspace != -1)
                {
                    //We are warping so set the subspace to -1
                    System.CurrentSubspace = -1;
                    System.SendChangeSubspaceMsg(-1);
                }
            }
            else if (TimeWarp.CurrentRateIndex == 0 && System.CurrentSubspace == -1)
            {
                //We stopped warping so send our new subspace
                System.WaitingSubspaceIdFromServer = true;
                System.SendNewSubspace();
            }
        }

        /// <summary>
        /// Event triggered when scene has changed
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (!System.SyncedToLastSubspace && MainSystem.Singleton.GameRunning && HighLogic.LoadedSceneIsGame)
            {
                System.CurrentSubspace = System.LatestSubspace;
                System.SyncedToLastSubspace = true;
            }
        }
    }
}
