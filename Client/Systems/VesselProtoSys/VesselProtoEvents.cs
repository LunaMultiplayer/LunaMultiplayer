using LunaClient.Base;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents: SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Called when a vessel is modified. We use it to update our own proto dictionary 
        /// and reflect changes so we don't have to call the "backupvessel" so often
        /// </summary>
        public void VesselModified(Vessel data)
        {
            if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.id))
                VesselsProtoStore.AllPlayerVessels[data.id].ProtoVessel = data.BackupVessel();
        }
    }
}
