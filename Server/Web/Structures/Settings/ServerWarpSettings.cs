using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerWarpSettings
    {
        public string WarpMode => WarpSettings.SettingsStore.WarpMode.ToString();
    }
}
