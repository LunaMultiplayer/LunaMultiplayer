using HarmonyLib;
using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;
using System.Collections.Generic;

namespace LmpClient.Systems.ShareScienceSubject
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

        protected override GameMode RelevantGameModes => GameMode.Career | GameMode.Science;

        public bool Reverting { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnScienceRecieved.Add(ShareScienceSubjectEvents.ScienceRecieved);

            RevertEvent.onRevertingToLaunch.Add(ShareScienceSubjectEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Add(ShareScienceSubjectEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareScienceSubjectEvents.LevelLoaded);

        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnScienceRecieved.Remove(ShareScienceSubjectEvents.ScienceRecieved);

            RevertEvent.onRevertingToLaunch.Remove(ShareScienceSubjectEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareScienceSubjectEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareScienceSubjectEvents.LevelLoaded);

            Reverting = false;
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
        }
    }
}
