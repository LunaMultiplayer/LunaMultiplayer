using System.Collections.Generic;

namespace LunaCommon
{
    public class IgnoredScenarios
    {
        /// <summary>
        /// No need to send the ScenarioNewGameIntro as that is for tutorials.
        /// Asteorids are handled in a different way so ignore them
        /// CustomWaypoints are useless and not shared between clients
        /// </summary>
        public static List<string> Names = new List<string>
        {
            "ScenarioNewGameIntro",
            "ScenarioDiscoverableObjects",
            "ScenarioCustomWaypoints",
            "ContractSystem", //This scenario has his own handling system
            "Funding",//This scenario has his own handling system
            "ProgressTracking",//This scenario has his own handling system
            "Reputation",//This scenario has his own handling system
            "ResearchAndDevelopment",//This scenario has his own handling system
            "ScenarioDestructibles",//This scenario has his own handling system
            "ScenarioUpgradeableFacilities",//This scenario has his own handling system
            "StrategySystem"//This scenario has his own handling system
        };
    }
}
