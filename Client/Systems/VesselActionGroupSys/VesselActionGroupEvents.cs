using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.VesselActionGroupSys
{
    public class VesselActionGroupEvents : SubSystem<VesselActionGroupSystem>
    {
        public void ActionGroupFired(Vessel vessel, KSPActionGroup actionGroup, bool value)
        {
            if (LockSystem.LockQuery.UpdateLockExists(vessel.id) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                return;

            System.MessageSender.SendVesselActionGroup(FlightGlobals.ActiveVessel, actionGroup, value);
        }
    }
}
