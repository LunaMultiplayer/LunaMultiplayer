using LunaConfigNode;
using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{
    public class Module
    {
        public Dictionary<string, string> Fields;
        public ConfigNode Events;
        public ConfigNode Actions;
        public ConfigNode Upgradesapplied;

        public Module(ConfigNode cfgNode)
        {
            Fields = cfgNode.GetAllValues().ToDictionary(k => k.Key, k => k.Value);

            Events = cfgNode.GetNodes("EVENTS")[0];
            Actions = cfgNode.GetNodes("ACTIONS")[0];
            Upgradesapplied = cfgNode.GetNodes("UPGRADESAPPLIED")[0];
        }
    }
}
