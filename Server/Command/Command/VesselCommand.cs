using Server.Command.Command.Base;
using Server.Log;
using Server.System;
using System;
using System.Linq;
using VesselClass = Server.System.Vessel.Classes.Vessel;

namespace Server.Command.Command
{
    public class VesselCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            var args = commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length < 2 || args[0].ToLower() != "info")
            {
                LunaLog.Error("Syntax error. Usage: /vessel info [name/guid]");
                return false;
            }

            var identifier = string.Join(" ", args.Skip(1));
            VesselClass vessel = null;

            if (Guid.TryParse(identifier, out var vesselId))
            {
                VesselStoreSystem.CurrentVessels.TryGetValue(vesselId, out vessel);
            }
            else
            {
                // Try to find by name
                vessel = VesselStoreSystem.CurrentVessels.Values.FirstOrDefault(v => 
                    v.Fields.GetSingle("name")?.Value.Equals(identifier, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (vessel != null)
            {
                var id = vessel.Fields.GetSingle("id")?.Value;
                var pidStr = vessel.Fields.GetSingle("pid")?.Value ?? id ?? "Unknown";
                var name = vessel.Fields.GetSingle("name")?.Value ?? "Unknown";
                var type = vessel.Fields.GetSingle("type")?.Value ?? "Unknown";
                var orbitingBody = vessel.GetOrbitingBodyName();
                var flightSituation = vessel.Fields.GetSingle("sit")?.Value ?? "Unknown";
                var partCount = vessel.Parts.Count();
                
                LunaLog.Normal($"--- Vessel Info: {name} ---");
                LunaLog.Normal($"ID: {pidStr}");
                LunaLog.Normal($"Type: {type}");
                LunaLog.Normal($"Location: {flightSituation} at {orbitingBody}");
                LunaLog.Normal($"Parts: {partCount}");
                
                if (Guid.TryParse(pidStr, out var pid))
                {
                    var controlLockOwner = LockSystem.LockQuery.GetControlLockOwner(pid);
                    if (controlLockOwner != null)
                        LunaLog.Normal($"Controller: {controlLockOwner}");
                    else
                        LunaLog.Normal("Controller: None");
                }

                return true;
            }

            LunaLog.Normal($"Vessel '{identifier}' not found.");
            return false;
        }
    }
}
