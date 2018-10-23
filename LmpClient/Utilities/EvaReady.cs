using LmpClient.Events;
using System.Collections;

namespace LmpClient.Utilities
{
    public class EvaReady
    {
        /// <summary>
        /// Fires the EvaEvent.onCrewEvaReady event when the eva sent as parameter has it's orbit ready
        /// </summary>
        public static void FireOnCrewEvaReady(KerbalEVA eva)
        {
            MainSystem.Singleton.StartCoroutine(OnCrewEvaReady(eva));
        }

        private static IEnumerator OnCrewEvaReady(KerbalEVA eva)
        {
            while (eva != null && !eva.Ready)
            {
                yield return null;
            }
            if (eva != null && eva.Ready)
            {
                EvaEvent.onCrewEvaReady.Fire(eva.vessel);
            }
        }
    }
}
