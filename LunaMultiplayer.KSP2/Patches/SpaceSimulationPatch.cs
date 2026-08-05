using HarmonyLib;
using KSP.Sim.impl;
using LunaMultiplayer.KSP2;

namespace LunaMultiplayer.KSP2.Patches
{
    /// <summary>
    /// 挂钩 SpaceSimulation.OnFixedUpdate：仿真每 tick 后采集/应用飞船状态。
    /// 这是同步层的主循环入口（对应 LMP 的 FixedUpdate 钩子）。
    /// </summary>
    [HarmonyPatch(typeof(SpaceSimulation), "OnFixedUpdate")]
    public static class SpaceSimulationPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // TODO: 仅在已连接（host/client）时同步；否则跳过。
            if (Network.Networker.IsConnected)
            {
                Sync.VesselSync.CollectAndSend();
            }
        }
    }
}
