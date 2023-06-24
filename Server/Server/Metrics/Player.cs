using Server.Settings.Structures;

namespace Server.Metrics {
    public class Player {
        public static readonly Prometheus.Gauge Total = Prometheus.Metrics.CreateGauge(
            "lmp_player_online_total",
            "The total count of unique players online.",
            new Prometheus.GaugeConfiguration{}
        );

        public static readonly Prometheus.Counter Online = Prometheus.Metrics.CreateCounter(
            "lmp_player_online",
            "Whether or not the player is currently online.",
            new Prometheus.CounterConfiguration{LabelNames = new[] {"name"}}
        );

        public static void AddPlayer(string name) {
            Total.Inc();

            if (MetricsSettings.SettingsStore.EnablePlayerDetailedMetrics) {
                Online.WithLabels(name).IncTo(1);
            }
        }

        public static void RemovePlayer(string name) {
            Total.Dec();

            if (MetricsSettings.SettingsStore.EnablePlayerDetailedMetrics) {
                Online.RemoveLabelled(name);
            }
        }
    }
}
