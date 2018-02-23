using LunaClient.Base.Interface;
using LunaClient.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach

namespace LunaClient.Windows
{
    public static class WindowsHandler
    {
        private static IWindow[] _windows = new IWindow[0];

        /// <summary>
        /// Here we pick all the classes that inherit from ISystem and we put them in the systems array
        /// </summary>
        public static void FillUpWindowsList()
        {
            var windowsList = new List<IWindow>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var systems = assembly.GetTypes().Where(t => t.IsClass && typeof(IWindow).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
                foreach (var sys in systems)
                {
                    var systemImplementation = WindowsContainer.Get(sys);
                    if (systemImplementation != null)
                        windowsList.Add(systemImplementation);
                }
            }

            _windows = windowsList.ToArray();
        }

        public static void Update()
        {
            for (var i = 0; i < _windows.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(_windows[i].WindowName);
                    _windows[i].Update();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "WindowsHandler-Update");
                }
            }
        }

        public static void OnGui()
        {
            for (var i = 0; i < _windows.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(_windows[i].WindowName);
                    _windows[i].OnGui();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "WindowsHandler-OnGui");
                }
            }
        }
    }
}