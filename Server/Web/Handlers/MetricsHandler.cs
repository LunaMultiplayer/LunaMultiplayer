using LmpCommon;
using System;
using System.Threading.Tasks;
using System.IO;
using uhttpsharp;
using Server.Settings.Structures;
using Server.Log;

namespace Server.Web.Handlers {
  public class MetricsHandler : IHttpRequestHandler {

    public static readonly Prometheus.Gauge BuildInfo = Prometheus.Metrics.CreateGauge(
      "lmp_build_info",
      "The build information of the Luna multiplayer server.",
      new Prometheus.GaugeConfiguration{LabelNames = new[] {"version"}}
    );

    public MetricsHandler() {
      if (!MetricsSettings.SettingsStore.EnableDefaultMetrics) {
        LunaLog.Info("Disabling default Prometheus metrics.");

        // Suppress the default metrics that come from the Prometheus client library.
        Prometheus.Metrics.SuppressDefaultMetrics();
      }

      // Populate the build info metric.
      BuildInfo.WithLabels(LmpVersioning.CurrentVersion.ToString()).Set(1);
    }

    public Task Handle(IHttpContext context, Func<Task> next) {
      // Write out the Prometheus metrics to the response.
      var stream = new MemoryStream();
      _ = Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream);
      context.Response = new HttpResponse("text/plain; version=0.0.4", stream, false);
      return Task.Factory.GetCompleted();
    }
  }
}
