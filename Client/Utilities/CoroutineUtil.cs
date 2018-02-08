using System;
using System.Collections;
using UnityEngine;

namespace LunaClient.Utilities
{
    public class CoroutineUtil
    {
        public static void StartDelayedRoutine(Action action, float delayInSec)
        {
            Client.Singleton.StartCoroutine(DelaySeconds(action, delayInSec));
        }

        private static IEnumerator DelaySeconds(Action action, float delayInSec)
        {
            yield return new WaitForSeconds(delayInSec);
            action();
        }
    }
}
