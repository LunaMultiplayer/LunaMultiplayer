using Server.Settings.Base;
using System;
using System.Linq;
using System.Reflection;

namespace Server.Settings
{
    public class SettingsHandler
    {
        public static void LoadSettings()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ISettings).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var instance = Activator.CreateInstance(type);
                ((ISettings)instance).Load();
            }
        }
    }
}
