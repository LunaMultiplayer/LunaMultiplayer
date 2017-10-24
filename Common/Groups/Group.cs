using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Groups
{
    public class Group
    {
        HashSet<string> Members { get; } = new HashSet<string>();
        public string Owner { get; set; } = null;
        public string Name { get; set; } = null;

        public Group(string name, string owner)
        {
            Members.Add(owner);
            this.Owner = owner;
            this.Name = name;
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
