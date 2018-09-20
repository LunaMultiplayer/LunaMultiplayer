using LunaConfigNode;
using System.Linq;

namespace Server.Structures
{
    public class Part
    {
        public MixedCollection<string, string> Fields;
        public MixedCollection<string, Module> Modules;
        public ConfigNode Events;
        public ConfigNode Actions;
        public ConfigNode Partdata;

        public Part(ConfigNode cfgNode)
        {
            Fields = new MixedCollection<string, string>(cfgNode.GetAllValues().Select(n => new MutableKeyValue<string, string>(n.Key, n.Value)));
            Modules = new MixedCollection<string, Module>(cfgNode.GetNodes("MODULE").Select(m=> new MutableKeyValue<string, Module>(m.GetValue("name"), new Module(m))));

            Events = cfgNode.GetNodes("EVENTS").FirstOrDefault();
            Actions = cfgNode.GetNodes("ACTIONS").FirstOrDefault();
            Partdata = cfgNode.GetNodes("PARTDATA").FirstOrDefault();
        }
    }
}
