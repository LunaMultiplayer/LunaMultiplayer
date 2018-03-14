using System.Reflection;

namespace LunaClient.Systems.Bug
{
    /// <summary>
    /// This class fixes KSP bugs
    /// </summary>
    public class BugSystem : Base.System<BugSystem>
    {
        #region Fields & properties

        private static readonly FieldInfo Body = typeof(LaunchSite).GetField("body", BindingFlags.NonPublic | BindingFlags.Instance);

        public BugEvents BugEvents { get; } = new BugEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(BugSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFlightReady.Add(BugEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onFlightReady.Remove(BugEvents.FlightReady);
        }

        #endregion

        public void FixLaunchSites()
        {
            foreach (var launchSite in PSystemSetup.Instance.StockLaunchSites)
            {
                Body.SetValue(launchSite, FlightGlobals.Bodies[0]);
            }
        }
    }
}
