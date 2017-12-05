using LMP.Server.Client;
using LMP.Server.Log;
using LMP.Server.System;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LMP.Server.Plugin
{
    internal static class LmpPluginHandler
    {
        private static object ListLock { get; } = new object();
        private static List<ILmpPlugin> LoadedPlugins { get; } = new List<ILmpPlugin>();

        static LmpPluginHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //This will find and return the assembly requested if it is already loaded
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.FullName == args.Name))
            {
                LunaLog.Debug($"Resolved plugin assembly reference: {args.Name} (referenced by {args.RequestingAssembly.FullName})");
                return assembly;
            }

            LunaLog.Error($"Could not resolve assembly {args.Name} referenced by {args.RequestingAssembly.FullName}");
            return null;
        }

        public static void LoadPlugins()
        {
            var pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (!FileHandler.FolderExists(pluginDirectory))
                FileHandler.FolderCreate(pluginDirectory);
            LunaLog.Debug("Loading plugins...");

            //Load all the assemblies just in case they depend on each other during instantation
            var loadedAssemblies = new List<Assembly>();
            var pluginFiles = FileHandler.GetFilesInPath(pluginDirectory, SearchOption.AllDirectories);
            foreach (var pluginFile in pluginFiles.Where(pluginFile => Path.GetExtension(pluginFile)?.ToLower() == ".dll"))
            {
                try
                {
                    //UnsafeLoadFrom will not throw an exception if the dll is marked as unsafe, 
                    //such as downloaded from internet in Windows
                    //See http://stackoverflow.com/a/15238782
                    var loadedAssembly = Assembly.UnsafeLoadFrom(pluginFile);
                    loadedAssemblies.Add(loadedAssembly);
                    LunaLog.Debug($"Loaded {pluginFile}");
                }
                catch (NotSupportedException)
                {
                    //This should only occur if using Assembly.LoadFrom() above instead of Assembly.UnsafeLoadFrom()
                    LunaLog.Debug($"Can't load dll, perhaps it is blocked: {pluginFile}");
                }
                catch
                {
                    LunaLog.Debug($"Error loading {pluginFile}");
                }
            }

            //Iterate through the assemblies looking for classes that have the ILmpPlugin interface
            foreach (var loadedAssembly in loadedAssemblies)
            {
                foreach (var loadedType in loadedAssembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(ILmpPlugin))))
                {
                    LunaLog.Debug($"Loading plugin: {loadedType.FullName}");
                    try
                    {
                        var pluginInstance = ActivatePluginType(loadedType);
                        if (pluginInstance != null)
                        {
                            LunaLog.Debug($"Loaded plugin: {loadedType.FullName}");
                            lock (ListLock)
                            {
                                LoadedPlugins.Add(pluginInstance);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LunaLog.Error($"Error loading plugin {loadedType.FullName}({loadedType.Assembly.FullName}) Exception: {ex}");
                    }
                }
                LunaLog.Debug("Done!");
            }
        }

        private static ILmpPlugin ActivatePluginType(Type loadedType)
        {
            try
            {
                //"as ILmpPlugin" will cast or return null if the Type is not a ILmpPlugin
                var pluginInstance = Activator.CreateInstance(loadedType) as ILmpPlugin;
                return pluginInstance;
            }
            catch (Exception e)
            {
                LunaLog.Error($"Cannot activate plugin {loadedType.Name}, Exception: {e}");
                return null;
            }
        }

        //Fire OnUpdate
        public static void FireOnUpdate()
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnUpdate();
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnUpdate event for {type.FullName} " +
                                      $"({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnServerStart
        public static void FireOnServerStart()
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnServerStart();
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnServerStart event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnServerStart
        public static void FireOnServerStop()
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnServerStop();
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnServerStop event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnClientConnect
        public static void FireOnClientConnect(ClientStructure client)
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnClientConnect(client);
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnClientConnect event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnClientAuthenticated
        public static void FireOnClientAuthenticated(ClientStructure client)
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnClientAuthenticated(client);
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnClientAuthenticated event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnClientDisconnect
        public static void FireOnClientDisconnect(ClientStructure client)
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnClientDisconnect(client);
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnClientDisconnect event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }

        //Fire OnMessageReceived
        public static void FireOnMessageReceived(ClientStructure client, IClientMessageBase message)
        {
            var handledByAny = false;
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnMessageReceived(client, message);

                        //prevent plugins from unhandling other plugin's Handled requests
                        if (message.Handled)
                            handledByAny = true;
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnMessageReceived event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                    message.Handled = handledByAny;
                }
            }
        }

        //Fire OnMessageReceived
        public static void FireOnMessageSent(ClientStructure client, IServerMessageBase message)
        {
            lock (ListLock)
            {
                foreach (var plugin in LoadedPlugins)
                {
                    try
                    {
                        plugin.OnMessageSent(client, message);
                    }
                    catch (Exception e)
                    {
                        var type = plugin.GetType();
                        LunaLog.Debug($"Error thrown in OnMessageSent event for {type.FullName} ({type.Assembly.FullName}), Exception: {e}");
                    }
                }
            }
        }
    }
}