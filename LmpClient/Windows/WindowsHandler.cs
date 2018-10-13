using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LmpClient.Base.Interface;
using UnityEngine.Profiling;

// ReSharper disable ForCanBeConvertedToForeach

namespace LmpClient.Windows
{
    public static class WindowsHandler
    {
        public static IWindow[] Windows { get; private set; } = new IWindow[0];

        /// <summary>
        /// Here we pick all the classes that inherit from ISystem and we put them in the systems array
        /// </summary>
        public static void FillUpWindowsList()
        {
            var windowsList = new List<IWindow>();

            var windows = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && typeof(IWindow).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
            foreach (var window in windows)
            {
                try
                {
                    if (window.GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null, null) is IWindow windowImplementation)
                        windowsList.Add(windowImplementation);
                }

                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception loading window type {window.FullName}: {ex.Message}");
                }
            }

            Windows = windowsList.ToArray();
        }

        public static void Update()
        {
            for (var i = 0; i < Windows.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(Windows[i].WindowName);
                    Windows[i].Update();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    MainSystem.Singleton.HandleException(e, "WindowsHandler-Update");
                }
            }
        }

        public static void OnGui()
        {
            for (var i = 0; i < Windows.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(Windows[i].WindowName);
                    Windows[i].OnGui();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    MainSystem.Singleton.HandleException(e, "WindowsHandler-OnGui");
                }
            }
        }
    }
}
