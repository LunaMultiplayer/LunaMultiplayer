using FinePrint.Utilities;
using LunaClient.Base;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UniLinq;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioSystem : MessageSystem<ScenarioSystem, ScenarioMessageSender, ScenarioMessageHandler>
    {
        #region Fields

        private ConcurrentDictionary<string, string> CheckData { get; } = new ConcurrentDictionary<string, string>();
        public ConcurrentQueue<ScenarioEntry> ScenarioQueue { get; private set; } = new ConcurrentQueue<ScenarioEntry>();
        private ConcurrentDictionary<string, Type> AllScenarioTypesInAssemblies { get; } = new ConcurrentDictionary<string, Type>();
        
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
            var validScenarios = KSPScenarioType.GetAllScenarioTypesInAssemblies()
                .Where(s =>
                    !HighLogic.CurrentGame.scenarios.Exists(psm => psm.moduleName == s.ModuleType.Name) &&
                    LoadModuleByGameMode(s));

            foreach (var validScenario in validScenarios)
            {
                LunaLog.Log($"[LMP]: Creating new scenario module {validScenario.ModuleType.Name}");
                HighLogic.CurrentGame.AddProtoScenarioModule(validScenario.ModuleType,
                    validScenario.ScenarioAttributes.TargetScenes);
            }
        }

        /// <summary>
        /// Check if the scenario has changed and sends it to the server
        /// </summary>
        public void SendScenarioModules()
        {
            if (Enabled && HighLogic.LoadedSceneIsFlight)
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
        private void ParseModulesToConfigNodes(IEnumerable<ScenarioModule> modules)
        {
            ScenariosConfigNodes.Clear();
            foreach (var scenarioModule in modules)
            {
                var scenarioType = scenarioModule.GetType().Name;

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
                if (scenarioEntry.ScenarioName == "ContractSystem")
                {
                    SpawnStrandedKerbalsForRescueMissions(scenarioEntry.ScenarioNode);
                    CreateMissingTourists(scenarioEntry.ScenarioNode);
                }
                if (scenarioEntry.ScenarioName == "ProgressTracking")
                    CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(scenarioEntry.ScenarioNode);

                CheckForBlankSceneSoTheGameDoesntBugOut(scenarioEntry);

                var psm = new ProtoScenarioModule(scenarioEntry.ScenarioNode);
                if (IsScenarioModuleAllowed(psm.moduleName))
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

        public void UpgradeTheAstronautComplexSoTheGameDoesntBugOut()
        {
            var sm = HighLogic.CurrentGame.scenarios.Find(psm => psm.moduleName == "ScenarioUpgradeableFacilities");
            if (sm != null &&
                ScenarioUpgradeableFacilities.protoUpgradeables.ContainsKey("SpaceCenter/AstronautComplex"))
            {
                foreach (var uf in ScenarioUpgradeableFacilities.protoUpgradeables["SpaceCenter/AstronautComplex"].facilityRefs)
                {
                    LunaLog.Log("[LMP]: Setting astronaut complex to max level");
                    uf.SetLevel(uf.MaxLevel);
                }
            }
        }

        #endregion

        #region Private methods

        private static void CreateMissingTourists(ConfigNode contractSystemNode)
        {
            var contractsNode = contractSystemNode.GetNode("CONTRACTS");

            var kerbalNames = contractsNode.GetNodes("CONTRACT")
                .Where(c => c.GetValue("type") == "TourismContract" && c.GetValue("state") == "Active")
                .SelectMany(c => c.GetNodes("PARAM"))
                .SelectMany(p => p.GetValues("kerbalName"));

            foreach (var kerbalName in kerbalNames)
            {
                LunaLog.Log($"[LMP]: Spawning missing tourist ({kerbalName}) for active tourism contract");
                var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Tourist);
                pcm.ChangeName(kerbalName);
            }
        }

        private static void SpawnStrandedKerbalsForRescueMissions(ConfigNode contractSystemNode)
        {
            var rescueContracts = contractSystemNode.GetNode("CONTRACTS").GetNodes("CONTRACT").Where(c => c.GetValue("type") == "RecoverAsset");
            foreach (var contractNode in rescueContracts)
            {
                if (contractNode.GetValue("state") == "Offered")
                {
                    var kerbalName = contractNode.GetValue("kerbalName");
                    if (!HighLogic.CurrentGame.CrewRoster.Exists(kerbalName))
                    {
                        LunaLog.Log($"[LMP]: Spawning missing kerbal ({kerbalName}) for offered KerbalRescue contract");
                        var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Unowned);
                        pcm.ChangeName(kerbalName);
                    }
                }
                if (contractNode.GetValue("state") == "Active")
                {
                    var kerbalName = contractNode.GetValue("kerbalName");
                    LunaLog.Log($"[LMP]: Spawning stranded kerbal ({kerbalName}) for active KerbalRescue contract");
                    var bodyId = int.Parse(contractNode.GetValue("body"));
                    if (!HighLogic.CurrentGame.CrewRoster.Exists(kerbalName))
                        GenerateStrandedKerbal(bodyId, kerbalName);
                }
            }
        }

        private static void GenerateStrandedKerbal(int bodyId, string kerbalName)
        {
            //Add kerbal to crew roster.
            LunaLog.Log($"[LMP]: Spawning missing kerbal, Name: {kerbalName}");

            var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Unowned);
            pcm.ChangeName(kerbalName);
            pcm.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            //Create protovessel
            var newPartId = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
            var contractBody = FlightGlobals.Bodies[bodyId];

            //Atmo: 10km above atmo, to half the planets radius out.
            //Non-atmo: 30km above ground, to half the planets radius out.
            var minAltitude = CelestialUtilities.GetMinimumOrbitalDistance(contractBody, 1.1f);
            var maxAltitude = minAltitude + contractBody.Radius * 0.5;

            var strandedOrbit = Orbit.CreateRandomOrbitAround(FlightGlobals.Bodies[bodyId], minAltitude, maxAltitude);

            var kerbalPartNode = new[] { ProtoVessel.CreatePartNode("kerbalEVA", newPartId, pcm) };

            var protoVesselNode = ProtoVessel.CreateVesselNode(kerbalName, VesselType.EVA, strandedOrbit, 0,
                kerbalPartNode);

            //It's not supposed to be infinite, but you're crazy if you think I'm going to decipher the values field of the rescue node.
            var discoveryNode = ProtoVessel.CreateDiscoveryNode(DiscoveryLevels.Unowned, UntrackedObjectClass.A,
                double.PositiveInfinity,
                double.PositiveInfinity);

            var protoVessel = new ProtoVessel(protoVesselNode, HighLogic.CurrentGame)
            {
                discoveryInfo = discoveryNode
            };

            HighLogic.CurrentGame.flightState.protoVessels.Add(protoVessel);
        }

        private static void CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(ConfigNode progressTrackingNode)
        {
            foreach (var possibleNode in progressTrackingNode.nodes)
                CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(possibleNode as ConfigNode);

            //The kerbals are kept in a ConfigNode named 'crew', with 'crews' as a comma space delimited array of names.
            if (progressTrackingNode.name == "crew")
            {
                var kerbalNames = progressTrackingNode.GetValue("crews");
                if (!string.IsNullOrEmpty(kerbalNames))
                {
                    var kerbalNamesSplit = kerbalNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var kerbalName in kerbalNamesSplit.Where(k => !HighLogic.CurrentGame.CrewRoster.Exists(k)))
                    {
                        LunaLog.Log($"[LMP]: Generating missing kerbal from ProgressTracking: {kerbalName}");
                        var pcm = CrewGenerator.RandomCrewMemberPrototype();
                        pcm.ChangeName(kerbalName);
                        HighLogic.CurrentGame.CrewRoster.AddCrewMember(pcm);

                        //Also send it off to the server
                        SystemsContainer.Get<KerbalSystem>().MessageSender.SendKerbal(pcm);
                    }
                }
            }
        }

        //If the scene field is blank, KSP will throw an error while starting the game, meaning players will be unable to join the server.
        private static void CheckForBlankSceneSoTheGameDoesntBugOut(ScenarioEntry scenarioEntry)
        {
            if (scenarioEntry.ScenarioNode.GetValue("scene") == string.Empty)
            {
                var nodeName = scenarioEntry.ScenarioName;
                ScreenMessages.PostScreenMessage($"{nodeName} is badly behaved!");
                LunaLog.Log($"[LMP]: {nodeName} is badly behaved!");
                scenarioEntry.ScenarioNode.SetValue("scene", "7, 8, 5, 6, 9");
            }
        }

        private static bool LoadModuleByGameMode(KSPScenarioType validScenario)
        {
            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.CAREER:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewCareerGames);
                case Game.Modes.SCIENCE_SANDBOX:
                    return
                        validScenario.ScenarioAttributes.HasCreateOption(
                            ScenarioCreationOptions.AddToNewScienceSandboxGames);
                case Game.Modes.SANDBOX:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewSandboxGames);
            }
            return false;
        }

        private void LoadScenarioTypes()
        {
            AllScenarioTypesInAssemblies.Clear();

            var scenarioTypes = AssemblyLoader.loadedAssemblies
                .SelectMany(a => a.assembly.GetTypes())
                .Where(s => s.IsSubclassOf(typeof(ScenarioModule)) && !AllScenarioTypesInAssemblies.ContainsKey(s.Name));

            foreach (var scenarioType in scenarioTypes)
                AllScenarioTypesInAssemblies.TryAdd(scenarioType.Name, scenarioType);
        }

        private bool IsScenarioModuleAllowed(string scenarioName)
        {
            //Blacklist asteroid module from every game mode
            //We hijack this and enable / disable it if we need to.
            //Do not send kerbnet custom waypoints aswell. they fuck it up sometimes
            if (string.IsNullOrEmpty(scenarioName) || scenarioName == "ScenarioDiscoverableObjects" || scenarioName == "ScenarioCustomWaypoints") return false;

            if (!AllScenarioTypesInAssemblies.Any()) LoadScenarioTypes(); //Load type dictionary on first use

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