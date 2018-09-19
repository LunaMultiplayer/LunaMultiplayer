using System.Collections.Generic;

// ReSharper disable All

#pragma warning disable IDE1006
namespace LmpUpdater.Appveyor.Contracts
{
    public class RoleAce
    {
        public int roleId { get; set; }
        public string name { get; set; }
        public bool isAdmin { get; set; }
        public List<AccessRight> accessRights { get; set; }
    }
}
