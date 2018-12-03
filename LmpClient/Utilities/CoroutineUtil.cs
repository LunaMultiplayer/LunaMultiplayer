using System;
using System.Collections;
using UnityEngine;

namespace LmpClient.Utilities
{
    public class CoroutineUtil
    {
        public static void StartConditionRoutine(string routineName, Action action, Func<bool> condition, float maxFrameTries)
        {
            MainSystem.Singleton.StartCoroutine(RoutineWithCondition(routineName, action, condition, maxFrameTries));
        }

        public static void StartDelayedRoutine(string routineName, Action action, int framesDelay)
        {
            MainSystem.Singleton.StartCoroutine(DelayFrames(routineName, action, framesDelay));
        }

        public static void StartDelayedRoutine(string routineName, Action action, float delayInSec)
        {
            MainSystem.Singleton.StartCoroutine(DelaySeconds(routineName, action, delayInSec));
        }

        public static void ExecuteAction(string routineName, Action action, int amountOfFrames)
        {
            MainSystem.Singleton.StartCoroutine(RunForFrames(routineName, action, amountOfFrames));
        }

        public static void ExecuteAction(string routineName, Action action, float amountOfSeconds)
        {
            MainSystem.Singleton.StartCoroutine(RunForSeconds(routineName, action, amountOfSeconds));
        }

        private static IEnumerator RunForFrames(string routineName, Action action, int amountOfFrames)
        {
            while (amountOfFrames > 0)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"Error in run coroutine: {routineName}. Details {e}");
                }
                action.Invoke();
                amountOfFrames--;

                yield return 0;
            }
        }

        private static IEnumerator RunForSeconds(string routineName, Action action, float amountOfSeconds)
        {
            while (amountOfSeconds > 0)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"Error in run coroutine: {routineName}. Details {e}");
                }
                amountOfSeconds -= Time.deltaTime;

                yield return 0;
            }
        }

        private static IEnumerator DelaySeconds(string routineName, Action action, float delayInSec)
        {
            if (delayInSec > 0)
                yield return new WaitForSeconds(delayInSec);
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error in delayed coroutine: {routineName}. Details {e}");
            }
        }

        private static IEnumerator DelayFrames(string routineName, Action action, int framesToDelay)
        {
            var frames = 0;

            while (frames < framesToDelay)
            {
                frames++;
                yield return 0;
            }

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error in delayed coroutine: {routineName}. Details {e}");
            }
        }

        private static IEnumerator RoutineWithCondition(string routineName, Action action, Func<bool> condition, float maxFrameTries)
        {
            var tries = 0;

            while (!condition.Invoke() && tries < maxFrameTries)
            {
                tries++;
                yield return 0;
            }

            if (!condition.Invoke()) yield break;

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error in coroutine: {routineName}. Details {e}");
            }
        }
    }
}
