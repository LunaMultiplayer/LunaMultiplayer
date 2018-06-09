using Harmony;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;
using System.Collections.Generic;

namespace LunaClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectSystem : ShareProgressBaseSystem<ShareScienceSubjectSystem, ShareScienceSubjectMessageSender, ShareScienceSubjectMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareScienceSubjectSystem);

        private ShareScienceSubjectEvents ShareScienceSubjectEvents { get; } = new ShareScienceSubjectEvents();

        private Dictionary<string, ScienceSubject> _lastScienceSubjects = new Dictionary<string, ScienceSubject>();

        private static Dictionary<string, ScienceSubject> _scienceSubjects;
        public Dictionary<string, ScienceSubject> ScienceSubjects
        {
            get
            {
                if (_scienceSubjects == null)
                {
                    _scienceSubjects = Traverse.Create(ResearchAndDevelopment.Instance).Field("scienceSubjects").GetValue<Dictionary<string, ScienceSubject>>();
                }

                return _scienceSubjects;
            }
        }


        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnEnabled();
            
            GameEvents.OnScienceRecieved.Add(ShareScienceSubjectEvents.ScienceRecieved);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnDisabled();
            GameEvents.OnScienceRecieved.Remove(ShareScienceSubjectEvents.ScienceRecieved);
            _lastScienceSubjects.Clear();
            _scienceSubjects = null;
        }

        public override void SaveState()
        {
            base.SaveState();
            _lastScienceSubjects = ScienceSubjects;
        }

        public override void RestoreState()
        {
            base.RestoreState();
            Traverse.Create(ResearchAndDevelopment.Instance).Field("scienceSubjects").SetValue(_lastScienceSubjects);
            _scienceSubjects = null;
        }
    }
}
