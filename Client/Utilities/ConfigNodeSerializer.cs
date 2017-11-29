using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LunaClient.Utilities
{
    public class ConfigNodeSerializer
    {
        static ConfigNodeSerializer()
        {
            //Create the delegates
            var configNodeType = typeof(ConfigNode);

            var writeNodeMethodInfo = configNodeType.GetMethod("WriteNode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (writeNodeMethodInfo == null) return;

            //pass null for instance so we only do the slower reflection part once ever, then provide the instance at runtime
            WriteNodeThunk = (WriteNodeDelegate)Delegate.CreateDelegate(typeof(WriteNodeDelegate), null, writeNodeMethodInfo);

            //these ones really are static and won't have a instance first parameter 
            var preFormatConfigMethodInfo = configNodeType.GetMethod("PreFormatConfig", BindingFlags.NonPublic | BindingFlags.Static);
            if (preFormatConfigMethodInfo == null) return;

            PreFormatConfigThunk = (PreFormatConfigDelegate)Delegate.CreateDelegate(typeof(PreFormatConfigDelegate), null, preFormatConfigMethodInfo);

            var recurseFormatMethodInfo = configNodeType.GetMethod("RecurseFormat", 
                BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(List<string[]>) }, null);
            if (recurseFormatMethodInfo == null) return;

            RecurseFormatThunk = (RecurseFormatDelegate)Delegate.CreateDelegate(typeof(RecurseFormatDelegate), null, recurseFormatMethodInfo);
        }

        private static WriteNodeDelegate WriteNodeThunk { get; }
        private static PreFormatConfigDelegate PreFormatConfigThunk { get; }
        private static RecurseFormatDelegate RecurseFormatThunk { get; }

        public static byte[] Serialize(ConfigNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            //Call the insides of what ConfigNode would have called if we said Save(filename)
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                //we late bind to the instance by passing the instance as the first argument
                WriteNodeThunk(node, writer);
                return stream.ToArray();
            }
        }

        public static ConfigNode Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0 || data.All(b => b == 0)) return null;

            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream))
            {
                var lines = new List<string>();

                while (!reader.EndOfStream)
                    lines.Add(reader.ReadLine());

                var cfg = PreFormatConfigThunk(lines.ToArray());
                var node = RecurseFormatThunk(cfg);

                return node;
            }
        }

        private delegate void WriteNodeDelegate(ConfigNode configNode, StreamWriter writer);

        private delegate List<string[]> PreFormatConfigDelegate(string[] cfgData);

        private delegate ConfigNode RecurseFormatDelegate(List<string[]> cfg);
    }
}