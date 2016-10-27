using System;
using LunaClient.Base.Interface;
using LunaClient.Windows.Chat;
using LunaClient.Windows.Connection;
using LunaClient.Windows.CraftLibrary;
using LunaClient.Windows.Debug;
using LunaClient.Windows.Disclaimer;
using LunaClient.Windows.Mod;
using LunaClient.Windows.Options;
using LunaClient.Windows.ServerList;
using LunaClient.Windows.Status;
using LunaClient.Windows.UniverseConverter;

namespace LunaClient.Windows
{
    public static class WindowsHandler
    {
        public static void Update()
        {
            TryUpdate(DisclaimerWindow.Singleton);
            TryUpdate(ConnectionWindow.Singleton);
            TryUpdate(StatusWindow.Singleton);
            TryUpdate(ChatWindow.Singleton);
            TryUpdate(CraftLibraryWindow.Singleton);
            TryUpdate(DebugWindow.Singleton);
            TryUpdate(ModWindow.Singleton);
            TryUpdate(OptionsWindow.Singleton);
            TryUpdate(UniverseConverterWindow.Singleton);
            TryUpdate(ServerListWindow.Singleton);
        }
        
        public static void OnGui()
        {
            TryOnGui(DisclaimerWindow.Singleton);
            TryOnGui(ConnectionWindow.Singleton);
            TryOnGui(StatusWindow.Singleton);
            TryOnGui(ChatWindow.Singleton);
            TryOnGui(CraftLibraryWindow.Singleton);
            TryOnGui(DebugWindow.Singleton);
            TryOnGui(ModWindow.Singleton);
            TryOnGui(OptionsWindow.Singleton);
            TryOnGui(UniverseConverterWindow.Singleton);
            TryOnGui(ServerListWindow.Singleton);
        }

        private static void TryUpdate(IWindow window)
        {
            try
            {
                window.Update();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "WindowsHandler-Update");
            }
        }

        private static void TryOnGui(IWindow window)
        {
            try
            {
                window.OnGui();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "WindowsHandler-OnGui");
            }
        }
    }
}