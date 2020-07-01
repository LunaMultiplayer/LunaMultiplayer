using LmpCommon.Locks;
using Server.Command.Command.Base;
using Server.Log;
using Server.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Command.Command
{
    public class ListLocksCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            var allLocks = new List<LockDefinition>();
            foreach (var lockDefinition in LockSystem.LockQuery.GetAllLocks())
            {
                switch (lockDefinition.Type)
                {
                    case LockType.Contract:
                    case LockType.AsteroidComet:
                    case LockType.Kerbal:
                    case LockType.Spectator:
                        allLocks.Add(lockDefinition);
                        break;
                    case LockType.Control:
                    case LockType.Update:
                    case LockType.UnloadedUpdate:
                        if (VesselStoreSystem.VesselExists(lockDefinition.VesselId))
                            allLocks.Add(lockDefinition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

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
