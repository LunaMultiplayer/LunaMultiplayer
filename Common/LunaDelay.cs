using System.Collections.Concurrent;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
namespace LunaCommon
{
    /// <summary>
    /// We lack a Task.Delay in this net framework and we should never use Thread.Sleep so here is a basic implementation of Task.Delay
    /// It implements a concurrent queue to reduce gargabe.
    /// </summary>
    public static class LunaDelay
    {
        #region DelayStructure

        private class DelayStructure
        {
            private Timer Timer { get; } = new Timer { AutoReset = false };
            internal TaskCompletionSource<bool> Tcs { get; private set; } = new TaskCompletionSource<bool>(false);
            
            internal DelayStructure(int milliseconds)
            {
                Timer.Interval = milliseconds;
                Timer.Elapsed += (obj, args) =>
                {
                    Timer.Stop();
                    Tcs.SetResult(true);
                    CompletedDelays.Enqueue(this);
                };
                Timer.Start();
            }

            internal void Reuse(int milliseconds)
            {
                Tcs = new TaskCompletionSource<bool>(false);
                Timer.Interval = milliseconds;
                Timer.Start();
            }
        }

        #endregion

        private static ConcurrentQueue<DelayStructure> CompletedDelays { get; } = new ConcurrentQueue<DelayStructure>();

        public static Task Delay(int milliseconds)
        {
            if (CompletedDelays.TryDequeue(out var delayStruct))
            {
                delayStruct.Reuse(milliseconds);
            }
            else
            {
                delayStruct = new DelayStructure(milliseconds);
            }

            return delayStruct.Tcs.Task;
        }
    }
}
