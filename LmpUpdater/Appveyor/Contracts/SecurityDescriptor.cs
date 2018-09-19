using System.Collections.Generic;

// ReSharper disable All

#pragma warning disable IDE1006
namespace LmpUpdater.Appveyor.Contracts
{
    public class SecurityDescriptor
    {
        public List<AccessRightDefinition> accessRightDefinitions { get; set; }
        public List<RoleAce> roleAces { get; set; }
    }
}
