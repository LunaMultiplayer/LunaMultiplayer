using System.Linq;
using Server.Command.Command.Base;
using Server.Log;
using Server.System;

namespace Server.Command.Command
{
    public class ListLocksCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            var allLocks = LockSystem.LockQuery.GetAllLocks().ToList();
            if (!allLocks.Any())
            {
                LunaLog.Normal("No locks");
            }
            else
            {
                foreach (var lockDef in allLocks)
                {
                    LunaLog.Normal(lockDef.ToString());
                }
            }

            return true;
        }
    }
}
