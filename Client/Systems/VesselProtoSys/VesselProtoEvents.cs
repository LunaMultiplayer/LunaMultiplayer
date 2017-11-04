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
            if (System.AllPlayerVessels.ContainsKey(data.id))
                System.AllPlayerVessels[data.id].ProtoVessel = data.BackupVessel();
        }
    }
}
