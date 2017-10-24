using System.Collections.Generic;
using System.Linq;

namespace LunaCommon.Groups
{
    public class Group
    {
        private HashSet<string> Members { get; } = new HashSet<string>();
        public string Owner { get; set; }
        public string Name { get; set; }

        public Group(string name, string owner)
        {
            Members.Add(owner);
            Owner = owner;
            Name = name;
        }

        public void AddMember(string name)
        {
            Members.Add(name);
        }

        public void RemoveMember(string name)
        {
            Members.Remove(name);
        }

        public bool HasMember(string name)
        {
            return Members.Contains(name);
        }

        public string[] GetMembers()
        {
            return Members.ToArray();
        }
    }
}
