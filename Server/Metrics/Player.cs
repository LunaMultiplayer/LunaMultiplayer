namespace Server.Metrics {
    public class Player {
        public static readonly Prometheus.Counter Count = Prometheus.Metrics.CreateCounter(
            "lmp_player_online",
            "Whether or not the player is currently online.",
            new Prometheus.CounterConfiguration{LabelNames = new[] {"name"}}
        );

        public static void RemovePlayer(string name) {
            Count.RemoveLabelled(name);
        }
    }
}
