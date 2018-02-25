namespace LunaClient.Systems.GameScene
{
    /// <summary>
    /// This class controls when the scene changes. It resets the needed systems
    /// </summary>
    public class GameSceneSystem : Base.System<GameSceneSystem>
    {
        private GameSceneEvents GameSceneEvents { get; } = new GameSceneEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(GameSceneSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onGameSceneLoadRequested.Add(GameSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Add(GameSceneEvents.OnSceneChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onGameSceneLoadRequested.Remove(GameSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Remove(GameSceneEvents.OnSceneChanged);
        }

        #endregion
    }
}
