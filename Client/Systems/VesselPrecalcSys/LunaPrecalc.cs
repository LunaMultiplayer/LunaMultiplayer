using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.VesselPrecalcSys
{
    public class LunaPrecalc : VesselPrecalculate
    {
        /// <summary>
        /// Do only the kill checks if we OWN the unloaded update lock!
        /// </summary>
        public override void MainPhysics(bool doKillChecks)
        {
            if (Vessel != null)
            {
                base.MainPhysics(LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(Vessel.id, SettingsSystem.CurrentSettings.PlayerName) && doKillChecks);
            }
            else
            {
                base.MainPhysics(doKillChecks);
            }
        }
    }
}
