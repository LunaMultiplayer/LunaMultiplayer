namespace Server.Metrics {
    public class Player {
        public static readonly Prometheus.Gauge Count = Prometheus.Metrics.CreateGauge(
            "lmp_player_online",
            "Whether or not the player is currently online.",
            new Prometheus.GaugeConfiguration{LabelNames = new[] {"name"}}
        );

        public static void RemovePlayer(string name) {
            Count.RemoveLabelled(name);
        }
    }
}