using System;

namespace LMP.Server.Command
{
    public class CommandDefinition
    {
        public string Description;
        public Action<string> Func;
        public string Name;

        public CommandDefinition(string name, Action<string> func, string description)
        {
            Name = name;
            Func = func;
            Description = description;
        }
    }
}