using LunaClient.Base;

namespace LunaClient.Systems.Bug
{
    public class BugEvents : SubSystem<BugSystem>
    {
        public void FlightReady()
        {
            System.FixLaunchSites();
        }
    }
}
