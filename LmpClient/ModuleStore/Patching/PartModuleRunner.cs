using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LmpClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an event
    /// </summary>
    public class PartModuleRunner
    {
        public static bool Ready => _awakeTask.IsCompleted;
        private static Task _awakeTask;
        private static double _percentage;


        /// <summary>
        /// Call this method to scan all the PartModules and patch the methods
        /// </summary>
        public static void Awake()
        {
            if (_awakeTask != null) return;

            _awakeTask = Task.Run(() =>
            {
                var partModules = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()
                    .Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(PartModule))))
                    .ToArray();

                Parallel.ForEach(partModules, partModule =>
                {
                    try
                    {
                        PartModulePatcher.PatchFieldsAndMethods(partModule);
                        IncreasePercentage(1d / partModules.Length);
                    }
                    catch (Exception ex)
                    {
                        LunaLog.LogError($"Exception patching module {partModule.Name} from assembly {partModule.Assembly.GetName().Name}: {ex.Message}");
                    }
                });
            });

            _awakeTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the percentage in a thread safe manner
        /// </summary>
        /// <returns></returns>
        public static string GetPercentage()
        {
            return (Interlocked.CompareExchange(ref _percentage, 0, 0) * 100).ToString("0.##");
        }

        /// <summary>
        /// Increases the percentage in a thread safe manner
        /// </summary>
        private static double IncreasePercentage(double value)
        {
            var newCurrentValue = _percentage; // non-volatile read, so may be stale
            while (true)
            {
                var currentValue = newCurrentValue;
                var newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref _percentage, newValue, currentValue);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (newCurrentValue == currentValue)
                    return newValue;
            }
        }
    }
}
