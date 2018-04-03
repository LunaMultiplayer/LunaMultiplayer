using System;

namespace Server.Command
{
    public class CommandDefinition
    {
        public string Description;
        public Func<string, bool> Func;
        public string Name;

        public CommandDefinition(string name, Func<string, bool> func, string description)
        {
            Name = name;
            Func = func;
            Description = description;
        }
    }
}
