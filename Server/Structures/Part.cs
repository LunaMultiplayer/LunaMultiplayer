using LunaConfigNode;
using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{
    public class Part
    {
        public Dictionary<string, string> Fields;
        public Dictionary<string, Module> Modules;
        public ConfigNode Events;
        public ConfigNode Actions;
        public ConfigNode Partdata;

        public Part(ConfigNode cfgNode)
        {
            Fields = cfgNode.GetAllValues().ToDictionary(k => k.Key, k => k.Value);
            Modules = cfgNode.GetNodes("MODULE").ToDictionary(k => k.GetValue("name"), k => new Module(k));

            Events = cfgNode.GetNodes("EVENTS").FirstOrDefault();
            Actions = cfgNode.GetNodes("ACTIONS").FirstOrDefault();
            Partdata = cfgNode.GetNodes("PARTDATA").FirstOrDefault();
        }
    }
}
