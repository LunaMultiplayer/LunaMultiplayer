using System.Collections.Generic;

namespace LmpCommon
{
    public class IgnoredScenarios
    {
        public static List<string> IgnoreReceive { get; } = new List<string>
        {
            "ScenarioNewGameIntro", //Don't receive this - it resets the "new game" flag and triggers first-run popups (e.g. ClickThroughBlocker)
            "ScenarioDiscoverableObjects", //Asteroids have their own system
            "ScenarioCustomWaypoints", //Don't sync this
            // CivilianPopulation stores its entire state in a single value
            // (repoJSON) whose contents are a JSON blob containing literal '{' and
            // '}' characters. The server-side LunaConfigNode parser treats those
            // braces as structural tokens, which produces a malformed sub-node
            // tree on round-trip. Handing that to KSP's ProtoScenarioModule(node)
            // ctor NREs inside ConfigNode.CopyToRecursive. Skip the receive path
            // entirely so a stray file on the server can never crash the client.
            "CivilianPopulationModule",
        };

        public static List<string> IgnoreSend { get; } = new List<string>
        {
            "ScenarioNewGameIntro", //Do not send this scenario as it just contains true/false in case we accepted the tutorial
            "ScenarioDiscoverableObjects", //Asteroids have their own system
            "ScenarioCustomWaypoints",//Don't sync this
            "ContractSystem", //This scenario has its own handling system
            "Funding",//This scenario has its own handling system
            "ProgressTracking",//This scenario has its own handling system
            "Reputation",//This scenario has its own handling system
            "ResearchAndDevelopment",//This scenario has its own handling system
            "ScenarioDestructibles",//This scenario has its own handling system
            "ScenarioUpgradeableFacilities",//This scenario has its own handling system
            "StrategySystem",//This scenario has its own handling system
            // See IgnoreReceive note above. Clients keep their own local
            // CivilianPopulation state, but never echo it to the server because
            // the resulting scenario file cannot survive a parse + serialize
            // round-trip through LunaConfigNode without producing a tree that
            // crashes other clients on load.
            "CivilianPopulationModule",
        };
    }
}
