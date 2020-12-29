using Contracts;
using HarmonyLib;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.ShareContracts;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to not allow you generating contracts in case you don't have the contract lock
    /// </summary>
    [HarmonyPatch(typeof(ContractSystem))]
    [HarmonyPatch("OnAwake")]
    public class ContractSystem_OnAwake
    {
        [HarmonyPostfix]
        private static void PostFixConstructor(ContractSystem __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (ShareContractsSystem.Singleton.DefaultContractGenerateIterations == 0)
                ShareContractsSystem.Singleton.DefaultContractGenerateIterations = ContractSystem.generateContractIterations;

            ContractSystem.generateContractIterations = LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName) ?
                ShareContractsSystem.Singleton.DefaultContractGenerateIterations : 0;
        }
    }
}
