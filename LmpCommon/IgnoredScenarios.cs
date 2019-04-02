using System.Collections.Generic;

namespace LmpCommon
{
    public class IgnoredScenarios
    {
        public static List<string> IgnoreReceive { get; } = new List<string>
        {
            "ScenarioDiscoverableObjects", //Asteroids have their own system
            "ScenarioCustomWaypoints", //Don't sync this
        };

        public static List<string> IgnoreSend { get; } = new List<string>
        {
            "ScenarioNewGameIntro", //Do not send this scenario as it just contains true/false in case we accepted the tutorial
            "ScenarioDiscoverableObjects", //Asteroids have their own system
            "ScenarioCustomWaypoints",//Don't sync this
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
