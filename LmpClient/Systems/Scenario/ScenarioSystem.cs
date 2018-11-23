using LmpClient.Base;
using LmpClient.Extensions;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UniLinq;

namespace LmpClient.Systems.Scenario
{
    public class ScenarioSystem : MessageSystem<ScenarioSystem, ScenarioMessageSender, ScenarioMessageHandler>
    {
        #region Fields

        private ConcurrentDictionary<string, string> CheckData { get; } = new ConcurrentDictionary<string, string>();
        public ConcurrentQueue<ScenarioEntry> ScenarioQueue { get; private set; } = new ConcurrentQueue<ScenarioEntry>();

        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<string, Type> _allScenarioTypesInAssemblies = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, Type> AllScenarioTypesInAssemblies
        {
            get
            {
                if (!_allScenarioTypesInAssemblies.Any())
                {
                    var scenarioTypes = AssemblyLoader.loadedAssemblies
                        .SelectMany(a => a.assembly.GetLoadableTypes())
                        .Where(s => s.IsSubclassOf(typeof(ScenarioModule)) && !_allScenarioTypesInAssemblies.ContainsKey(s.Name));

                    foreach (var scenarioType in scenarioTypes)
                        _allScenarioTypesInAssemblies.TryAdd(scenarioType.Name, scenarioType);
                }

                return _allScenarioTypesInAssemblies;
            }
        }

        private static List<string> ScenarioName { get; } = new List<string>();
        private static List<byte[]> ScenarioData { get; } = new List<byte[]>();
        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ScenarioSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            //Run it every 30 seconds
            SetupRoutine(new RoutineDefinition(30000, RoutineExecution.Update, SendScenarioModules));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CheckData.Clear();
            ScenarioQueue = new ConcurrentQueue<ScenarioEntry>();
            AllScenarioTypesInAssemblies.Clear();
        }

        private static readonly List<Tuple<string,ConfigNode>> ScenariosConfigNodes = new List<Tuple<string,ConfigNode>>();

        #endregion

        #region Public methods

        public void LoadMissingScenarioDataIntoGame()
        {
            //ResourceScenario.Instance.Load();

            var validScenarios = KSPScenarioType.GetAllScenarioTypesInAssemblies()
                .Where(s => !HighLogic.CurrentGame.scenarios.Exists(psm => psm.moduleName == s.ModuleType.Name) && LoadModuleByGameMode(s));

            foreach (var validScenario in validScenarios)
            {
                LunaLog.Log($"[LMP]: Creating new scenario module {validScenario.ModuleType.Name}");
                HighLogic.CurrentGame.AddProtoScenarioModule(validScenario.ModuleType, validScenario.ScenarioAttributes.TargetScenes);
            }
        }

        /// <summary>
        /// Check if the scenario has changed and sends it to the server
        /// </summary>
        public void SendScenarioModules()
        {
            if (Enabled)
            {
                try
                {
                    var modules = ScenarioRunner.GetLoadedModules();
                    ParseModulesToConfigNodes(modules);
                    TaskFactory.StartNew(SendModulesConfigNodes);
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"Error while trying to send the scenario modules!. Details {e}");
                }
            }
        }

        /// <summary>
        /// This transforms the scenarioModule to a config node. We cannot do this in another thread as Lingoona 
        /// is called sometimes and that makes a hard crash
        /// </summary>
        private static void ParseModulesToConfigNodes(IEnumerable<ScenarioModule> modules)
        {
            ScenariosConfigNodes.Clear();
            foreach (var scenarioModule in modules)
            {
                var scenarioType = scenarioModule.GetType().Name;

                if(IgnoredScenarios.IgnoreSend.Contains(scenarioType))
                    continue;

                if (!IsScenarioModuleAllowed(scenarioType))
                    continue;

                var configNode = new ConfigNode();
                scenarioModule.Save(configNode);

                ScenariosConfigNodes.Add(new Tuple<string, ConfigNode>(scenarioType, configNode));
            }
        }

        /// <summary>
        /// Sends the parsed config nodes to the server after doing basic checks
        /// </summary>
        private void SendModulesConfigNodes()
        {
            ScenarioData.Clear();
            ScenarioName.Clear();

            foreach (var scenarioConfigNode in ScenariosConfigNodes)
            {
                var scenarioBytes = ConfigNodeSerializer.Serialize(scenarioConfigNode.Item2);
                var scenarioHash = Common.CalculateSha256Hash(scenarioBytes);

                if (scenarioBytes.Length == 0)
                {
                    LunaLog.Log($"[LMP]: Error writing scenario data for {scenarioConfigNode.Item1}");
                    continue;
                }

                //Data is the same since last time - Skip it.
                if (CheckData.ContainsKey(scenarioConfigNode.Item1) && CheckData[scenarioConfigNode.Item1] == scenarioHash) continue;

                CheckData[scenarioConfigNode.Item1] = scenarioHash;

                ScenarioName.Add(scenarioConfigNode.Item1);
                ScenarioData.Add(scenarioBytes);
            }

            if (ScenarioName.Any())
                MessageSender.SendScenarioModuleData(ScenarioName, ScenarioData);
        }

        public void LoadScenarioDataIntoGame()
        {
            while (ScenarioQueue.TryDequeue(out var scenarioEntry))
            {
                var psm = new ProtoScenarioModule(scenarioEntry.ScenarioNode);
                if (IsScenarioModuleAllowed(psm.moduleName) && !IgnoredScenarios.IgnoreReceive.Contains(psm.moduleName))
                {
                    LunaLog.Log($"[LMP]: Loading {psm.moduleName} scenario data");
                    HighLogic.CurrentGame.scenarios.Add(psm);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Skipping {psm.moduleName} scenario data in {SettingsSystem.ServerSettings.GameMode} mode");
                }
            }
        }
        
        #endregion

        #region Private methods
        
        private static bool LoadModuleByGameMode(KSPScenarioType validScenario)
        {
            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.CAREER:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewCareerGames);
                case Game.Modes.SCIENCE_SANDBOX:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewScienceSandboxGames);
                case Game.Modes.SANDBOX:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewSandboxGames);
            }
            return false;
        }
        
        private static bool IsScenarioModuleAllowed(string scenarioName)
        {
            if (string.IsNullOrEmpty(scenarioName)) return false;

            if (!AllScenarioTypesInAssemblies.ContainsKey(scenarioName)) return false; //Module missing

            var scenarioType = AllScenarioTypesInAssemblies[scenarioName];

            var scenarioAttributes = (KSPScenario[])scenarioType.GetCustomAttributes(typeof(KSPScenario), true);
            if (scenarioAttributes.Length > 0)
            {
                var attribute = scenarioAttributes[0];
                var protoAllowed = false;
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    protoAllowed = attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingCareerGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewCareerGames);
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingScienceSandboxGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewScienceSandboxGames);
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
                {
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingSandboxGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewSandboxGames);
                }
                return protoAllowed;
            }

            //Scenario is not marked with KSPScenario - let's load it anyway.
            return true;
        }

        #endregion
    }
}
