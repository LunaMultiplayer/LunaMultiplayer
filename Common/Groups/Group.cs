using System;
using System.Collections.Generic;

namespace LunaCommon.Groups
{
    [Serializable]
    public class Group
    {
        public HashSet<string> Members { get; set; }
        public HashSet<string> Invited { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }

        public Group()
        {
            Members = new HashSet<string>();
            Invited = new HashSet<string>();
        }

        public Group Clone()
        {
            return MemberwiseClone() as Group;
        }
    }
}
