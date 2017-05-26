using System;
using UnityEngine;

namespace LunaClient
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Client : MonoBehaviour
    {
        public static string KspPath { get; private set; }
        public static Client Singleton { get; set; }

        public Client()
        {
            Singleton = this;
        }

        public void Awake()
        {
            //We set this variable here so KspPath can be used on constructors
            KspPath = UrlDir.ApplicationRootPath;

            DontDestroyOnLoad(this);
            try
            {
                MainSystem.Singleton.Reset();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "MainClient-" + nameof(Awake));
            }
        }

        public void OnApplicationQuit()
        {
            MainSystem.Singleton.OnExit();
        }

        public void OnDestroy()
        {
            MainSystem.Singleton.OnExit();
        }

        public void Update()
        {
            try
            {
                MainSystem.Singleton.MainSystemUpdate();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "MainClient-" + nameof(Update));
            }
        }
        
        public void OnGui()
        {
            try
            {
                MainSystem.Singleton.OnGui();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "MainClient-" + nameof(OnGui));
            }
        }

        public void FixedUpdate()
        {
            try
            {
                MainSystem.Singleton.MainSystemFixedUpdate();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "MainClient-" + nameof(FixedUpdate));
            }
        }
    }
}