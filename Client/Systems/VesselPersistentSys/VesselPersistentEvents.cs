using LunaClient.Base;

namespace LunaClient.Systems.VesselPersistentSys
{
    public class VesselPersistentEvents : SubSystem<VesselPersistentSystem>
    {
        public void PartPersistentIdChanged(uint vesselPersistentId, uint from, uint to)
        {
            System.MessageSender.SendPartPersistantIdChanged(vesselPersistentId, from, to);
        }

        public void VesselPersistentIdChanged(uint from, uint to)
        {
            System.MessageSender.SendVesselPersistantIdChanged(from, to);
        }
    }
}
