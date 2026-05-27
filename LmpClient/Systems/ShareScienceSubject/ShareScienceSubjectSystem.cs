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

        // Static so it survives OnDisabled()/OnEnabled() across scene transitions.
        // Captured at the moment the player launches, so revert can restore to that baseline.
        private static Dictionary<string, ScienceSubject> _preFlightSnapshot;
        private static bool _hasPreFlightSnapshot;

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
            GameEvents.onFlightReady.Add(OnFlightReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            GameEvents.OnScienceRecieved.Remove(ShareScienceSubjectEvents.ScienceRecieved);

            RevertEvent.onRevertingToLaunch.Remove(ShareScienceSubjectEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareScienceSubjectEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareScienceSubjectEvents.LevelLoaded);
            GameEvents.onFlightReady.Remove(OnFlightReady);

            Reverting = false;
            _scienceSubjects = null;
        }

        public override void SaveState()
        {
            base.SaveState();
            // Preserve existing pre-flight snapshot — SaveState is also called during revert
            // and we don't want to overwrite the snapshot with in-flight data
        }

        public override void RestoreState()
        {
            base.RestoreState();
            if (_hasPreFlightSnapshot)
                Traverse.Create(ResearchAndDevelopment.Instance).Field("scienceSubjects").SetValue(_preFlightSnapshot);
        }

        /// <summary>
        /// Sends the pre-flight snapshot to the server so it and other clients can revert.
        /// Called from RevertingDetected before any scene transition wipes state.
        /// </summary>
        public void SendRevertSnapshot()
        {
            if (!_hasPreFlightSnapshot || _preFlightSnapshot.Count == 0) return;
            MessageSender.SendScienceSubjectRevert(_preFlightSnapshot);
        }

        private void OnFlightReady()
        {
            if (!ShareSystemReady) return;
            _preFlightSnapshot = new Dictionary<string, ScienceSubject>(ScienceSubjects);
            _hasPreFlightSnapshot = true;
        }
    }
}
