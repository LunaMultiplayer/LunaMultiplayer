using LunaClient.Systems;
using System;
using UnityEngine;

namespace LunaClient
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Client : MonoBehaviour
    {
        public static string KspPath { get; private set; }

        public void Awake()
        {
            //We set this variable here so KspPath can be used on constructors
            KspPath = UrlDir.ApplicationRootPath;

            DontDestroyOnLoad(this);
            try
            {
                SystemsContainer.Get<MainSystem>().Reset();
            }
            catch (Exception e)
            {
                SystemsContainer.Get<MainSystem>().HandleException(e, $"MainClient-{nameof(Awake)}");
            }
        }

        public void OnApplicationQuit()
        {
            SystemsContainer.Get<MainSystem>().OnExit();
        }

        public void OnDestroy()
        {
            SystemsContainer.Get<MainSystem>().OnExit();
        }

        public void Update()
        {
            try
            {
                SystemsContainer.Get<MainSystem>().MainSystemUpdate();
            }
            catch (Exception e)
            {
                SystemsContainer.Get<MainSystem>().HandleException(e, $"MainClient-{nameof(Update)}");
            }
        }

        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            try
            {
                SystemsContainer.Get<MainSystem>().OnGui();
            }
            catch (Exception e)
            {
                SystemsContainer.Get<MainSystem>().HandleException(e, $"MainClient-{nameof(OnGUI)}");
            }
        }

        public void FixedUpdate()
        {
            try
            {
                SystemsContainer.Get<MainSystem>().MainSystemFixedUpdate();
            }
            catch (Exception e)
            {
                SystemsContainer.Get<MainSystem>().HandleException(e, $"MainClient-{nameof(FixedUpdate)}");
            }
        }

        public void LateUpdate()
        {
            try
            {
                SystemsContainer.Get<MainSystem>().MainSystemLateUpdate();
            }
            catch (Exception e)
            {
                SystemsContainer.Get<MainSystem>().HandleException(e, $"MainClient-{nameof(LateUpdate)}");
            }
        }
    }
}