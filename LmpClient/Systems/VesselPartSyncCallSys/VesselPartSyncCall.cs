using Harmony;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselPartSyncCallSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSyncCall
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string MethodName;

        #endregion

        public void ProcessPartMethodCallSync()
        {
            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null || !vessel.loaded) return;

            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            var part = vessel.protoVessel.GetProtoPart(PartFlightId);
            if (part != null)
            {
                var module = part.FindProtoPartModuleInProtoPart(ModuleName);
                if (module != null)
                {
                    if (module.moduleRef != null)
                    {
                        module.moduleRef.GetType().GetMethod(MethodName, AccessTools.all)?.Invoke(module.moduleRef, null);
                        PartModuleEvent.onPartModuleMethodProcessed.Fire(module, MethodName);
                    }
                }
            }
        }
    }
}
