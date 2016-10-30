using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.Warp
{
    public class WarpEvents: SubSystem<WarpSystem>
    {
        /// <summary>
        /// Event triggered when time warp is changed
        /// </summary>
        public void OnTimeWarpChanged()
        {
            var resetWarp = SettingsSystem.ServerSettings.WarpMode == WarpMode.NONE ||
                            SettingsSystem.ServerSettings.WarpMode == WarpMode.MASTER && SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName;

            if (TimeWarp.CurrentRateIndex > 0)
            {
                if (resetWarp)
                {
                    if (SettingsSystem.ServerSettings.WarpMode == WarpMode.NONE)
                        System.DisplayMessage("Cannot warp, warping is disabled on this server", 5f);

                    TimeWarp.SetRate(0, true);
                    return;
                }

                //We are warping so set the subspace to -1
                System.CurrentSubspace = -1;
                System.SendChangeSubspaceMsg(-1);
            }
            else if (TimeWarp.CurrentRateIndex == 0 && System.CurrentSubspace == -1)
            {
                //We stopped warping so send our new subspace
                System.NewSubspaceSent = false;
                System.WaitingSubspaceIdFromServer = true;
                System.SendNewSubspace();
            }
        }
    }
}
