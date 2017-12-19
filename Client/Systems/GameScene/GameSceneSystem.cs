namespace LunaClient.Systems.GameScene
{
    /// <summary>
    /// This class controls when the scene changes. It resets the needed systems
    /// </summary>
    public class GameSceneSystem : Base.System
    {
        private GameSceneEvents GameSceneEvents { get; } = new GameSceneEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(GameSceneSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onLevelWasLoadedGUIReady.Add(GameSceneEvents.OnSceneChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onLevelWasLoadedGUIReady.Remove(GameSceneEvents.OnSceneChanged);
        }

        #endregion
    }
}
