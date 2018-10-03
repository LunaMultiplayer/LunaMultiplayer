using System;
using System.Linq;
using System.Reflection;
using Harmony;
using LmpClient.ModuleStore.Injectors.Base;

namespace LmpClient.ModuleStore.Injectors.KIS
{
    public class KisInjector : BaseInjector
    {
        public override Assembly Assembly => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "KIS");
        public override Type Type => Assembly?.GetType("KIS.KIS_Shared");
        public override MethodInfo Method => Type?.GetMethods().Where(m=> m.Name == "CreatePart").OrderByDescending(m=> m.GetParameters().Length).FirstOrDefault();
        
        public override MethodInfo PrefixMethod => null;
        public override MethodInfo PostFixMethod => typeof(KisInjector).GetMethod("Postfix", AccessTools.all);

        /// <summary>
        /// When dropping a KIS inventory item don't set it's type as "debris" as the server will remove it with the dekessler
        /// </summary>
        /// <param name="__result"></param>
        private static void Postfix(Part __result)
        {
            if (__result.vessel.vesselType == VesselType.Debris)
                __result.vessel.vesselType = VesselType.Unknown;
        }
    }
}
