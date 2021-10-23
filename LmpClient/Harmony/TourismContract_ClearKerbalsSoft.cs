using FinePrint.Contracts;
using HarmonyLib;
using LmpClient.Systems.KerbalSys;
using LmpClient.Systems.ShareContracts;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to remove the tourist kerbals from the server when finishing/declining/cancelling tourism contracts
    /// </summary>
    [HarmonyPatch(typeof(TourismContract))]
    [HarmonyPatch("ClearKerbalsSoft")]
    public class TourismContract_ClearKerbalsSoft
    {
        [HarmonyPostfix]
        private static void PostfixClearKerbalsSoft(TourismContract __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;
            if (ShareContractsSystem.Singleton.IgnoreEvents) return;

            foreach (var kerbal in __instance.Tourists)
            {
                if (!HighLogic.CurrentGame.CrewRoster.Exists(kerbal))
                    KerbalSystem.Singleton.MessageSender.SendKerbalRemove(kerbal);
            }
        }
    }
}
