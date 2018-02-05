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
                    System.DisplayMessage(SettingsSystem.ServerSettings.WarpMode == WarpMode.None ? 
                        "Cannot warp, warping is disabled on this server" : 
                        "Cannot warp, you are not the warp master!", 5f);

                    TimeWarp.SetRate(0, true);
                    return;
                }

                if (System.WaitingSubspaceIdFromServer)
                {
                    System.DisplayMessage("Cannot warp, waiting subspace id from the server", 5f);

                    TimeWarp.SetRate(0, true);
                    return;
                }

                if (System.CurrentSubspace != -1)
                {
                    //We are warping so set the subspace to -1
                    System.CurrentSubspace = -1;
                    System.MessageSender.SendChangeSubspaceMsg(-1);
                }
            }
            //Detecting here if warp has stopped (TimeWarp.CurrentRateIndex == 0 && System.CurrentSubspace == -1) is not reliable so we use a routine to check it
        }

        /// <summary>
        /// Event triggered when scene has changed
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (System.Enabled && !System.SyncedToLastSubspace && HighLogic.LoadedSceneIsGame)
            {
                System.CurrentSubspace = System.LatestSubspace;
                System.SyncedToLastSubspace = true;
            }
        }
    }
}
