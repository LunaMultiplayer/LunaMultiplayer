using System;
using System.Collections;
using UnityEngine;

namespace LmpClient.Utilities
{
    public class CoroutineUtil
    {
        public static void StartDelayedRoutine(string routineName, Action action, float delayInSec)
        {
            MainSystem.Singleton.StartCoroutine(DelaySeconds(routineName, action, delayInSec));
        }

        private static IEnumerator DelaySeconds(string routineName, Action action, float delayInSec)
        {
            if (delayInSec > 0)
                yield return new WaitForSeconds(delayInSec);
            try
            {
                action();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error in delayed coroutine: {routineName}. Details {e}");
            }
        }
    }
}
