namespace LmpClient.Extensions
{
    public static class ProtoPartExtension
    {
        /// <summary>
        /// Finds a proto part resource snapshot in a proto part snapshot without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartResourceSnapshot FindResourceInProtoPart(this ProtoPartSnapshot protoPart, string resourceName)
        {
            if (protoPart == null) return null;

            for (var i = 0; i < protoPart.resources.Count; i++)
            {
                if (protoPart.resources[i].resourceName == resourceName)
                    return protoPart.resources[i];
            }
            return null;
        }


        /// <summary>
        /// Finds a module in a proto part module without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartModuleSnapshot FindProtoPartModuleInProtoPart(this ProtoPartSnapshot part, string moduleName)
        {
            if (part == null) return null;

            for (var i = 0; i < part.modules.Count; i++)
            {
                if (part.modules[i].moduleName == moduleName)
                    return part.modules[i];
            }

            return null;
        }
    }
}
