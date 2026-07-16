namespace LmpClient.Systems.Scenario
{
    public class ScenarioEntry
    {
        public string ScenarioModule { get; set; }
        public ConfigNode ScenarioNode { get; set; }

        /// <summary>
        /// Wire payload from the server when <see cref="ScenarioNode"/> could not be built (for NullScenario.log).
        /// </summary>
        public byte[] RawScenarioBytes { get; set; }

        public int RawNumBytes { get; set; }
    }
}