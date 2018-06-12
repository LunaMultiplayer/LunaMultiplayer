using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionEvents : SubSystem<VesselPositionSystem>
    {
        /// <summary>
        /// Whenever we acquire a UnloadedUpdate/Update/Control lock of a vessel, remove it from the dictionaries
        /// </summary>
        public void OnLockAcquire(LockDefinition data)
        {
            if (data.PlayerName != SettingsSystem.CurrentSettings.PlayerName)
                return;

            switch (data.Type)
            {
                case LockType.UnloadedUpdate:
                case LockType.Update:
                case LockType.Control:
                    System.RemoveVessel(data.VesselId);
                    break;
            }
        }

        /// <summary>
        /// When stop warping adjust the interpolation times of long running packets
        /// </summary>
        public void WarpStopped()
        {
            System.AdjustExtraInterpolationTimes();
        }
    }
}
