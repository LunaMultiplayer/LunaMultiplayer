using Contracts;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.ShareContracts
{
    public class ShareContractsEvents : SubSystem<ShareContractsSystem>
    {
        #region EventHandlers
        public void ContractAccepted(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract accepted: {contract.ContractGuid}");
        }

        public void ContractCancelled(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract cancelled: {contract.ContractGuid}");
        }

        public void ContractCompleted(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract completed: {contract.ContractGuid}");
        }

        public void ContractsListChanged()
        {
            LunaLog.Log("Contract list changed.");
        }

        public void ContractsLoaded()
        {
            LunaLog.Log("Contracts loaded.");
        }

        public void ContractDeclined(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract declined: {contract.ContractGuid}");
        }

        public void ContractFailed(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract failed: {contract.ContractGuid}");
        }

        public void ContractFinished(Contract contract)
        {
            /*
            Doesn't need to be synchronized because there is no ContractFinished state.
            Also the contract will be finished on the contract complete / failed / cancelled / ...
            */
        }

        public void ContractOffered(Contract contract)
        {
            LunaLog.Log($"Contract offered: {contract.ContractGuid} - {contract.Title}");

            if (!LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName)) return;

            //This should be only called on the client with the contract lock, because he has the generationCount != 0.
            System.MessageSender.SendContractMessage(contract);

            //Also store the current contracts on the server so new players will see this contract too.
            ScenarioSystem.Singleton.SendScenarioModules();
        }

        public void ContractParameterChanged(Contract contract, ContractParameter contractParameter)
        {
            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract parameter changed on:{contract.ContractGuid}");
        }

        public void ContractRead(Contract contract)
        {
            LunaLog.Log($"Contract read:{contract.ContractGuid}");
        }

        public void ContractSeen(Contract contract)
        {
            LunaLog.Log($"Contract seen:{contract.ContractGuid}");
        }
        #endregion
    }
}
