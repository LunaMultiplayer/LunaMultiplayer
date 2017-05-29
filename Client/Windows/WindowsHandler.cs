using LunaClient.Base.Interface;
using LunaClient.Systems;
using LunaClient.Windows.Chat;
using LunaClient.Windows.Connection;
using LunaClient.Windows.CraftLibrary;
using LunaClient.Windows.Debug;
using LunaClient.Windows.Mod;
using LunaClient.Windows.Options;
using LunaClient.Windows.ServerList;
using LunaClient.Windows.Status;
using LunaClient.Windows.Systems;
using LunaClient.Windows.UniverseConverter;
using System;

namespace LunaClient.Windows
{
    public static class WindowsHandler
    {
        private static readonly IWindow[] Windows =
        {
            WindowsContainer.Get<ConnectionWindow>(),
            WindowsContainer.Get<StatusWindow>(),
            WindowsContainer.Get<ChatWindow>(),
            WindowsContainer.Get<CraftLibraryWindow>(),
            WindowsContainer.Get<DebugWindow>(),
            WindowsContainer.Get<SystemsWindow>(),
            WindowsContainer.Get<ModWindow>(),
            WindowsContainer.Get<OptionsWindow>(),
            WindowsContainer.Get<UniverseConverterWindow>(),
            WindowsContainer.Get<ServerListWindow>(),
        };

        public static void Update()
        {
            foreach (var window in Windows)
            {
                try
                {
                    window.Update();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "WindowsHandler-Update");
                }
            }
        }

        public static void OnGui()
        {
            foreach (var window in Windows)
            {
                try
                {
                    window.OnGui();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "WindowsHandler-OnGui");
                }
            }
        }
    }
}