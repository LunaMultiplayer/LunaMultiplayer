using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.VesselStore;
using System.Reflection;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.KscScene
{
    /// <summary>
    /// This class controls what happens when we are in KSC screen
    /// </summary>
    public class KscSceneSystem : System<KscSceneSystem>
    {
        #region Fields and properties

        public readonly MethodInfo ClearVesselMarkers = typeof(KSCVesselMarkers).GetMethod("ClearVesselMarkers", BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool Ready => HighLogic.LoadedScene == GameScenes.SPACECENTER && KSCVesselMarkers.fetch != null;

        private static AstronautComplex _astronautComplex;
        private static AstronautComplex AstronautComplex
        {
            get
            {
                if (_astronautComplex == null)
                    _astronautComplex = Object.FindObjectOfType<AstronautComplex>();
                return _astronautComplex;
            }
        }

        private static readonly MethodInfo InitGui = typeof(AstronautComplex).GetMethod("InitiateGUI", BindingFlags.NonPublic | BindingFlags.Instance);

        private static KscSceneEvents KscSceneEvents { get; }= new KscSceneEvents();

        public bool SceneIsAstronautComplex { get; set; } = false;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(KscSceneSystem);

        protected override void OnEnabled()
        {
            SceneIsAstronautComplex = false;
            base.OnEnabled();
            GameEvents.onLevelWasLoadedGUIReady.Add(KscSceneEvents.LevelLoaded);
            GameEvents.onGUIAstronautComplexSpawn.Add(KscSceneEvents.AstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Add(KscSceneEvents.AstronautComplexDespawn);
        }

        protected override void OnDisabled()
        {
            SceneIsAstronautComplex = false;
            base.OnDisabled();
            GameEvents.onLevelWasLoadedGUIReady.Remove(KscSceneEvents.LevelLoaded);
            GameEvents.onGUIAstronautComplexSpawn.Remove(KscSceneEvents.AstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(KscSceneEvents.AstronautComplexDespawn);
        }

        #endregion
    }
}
