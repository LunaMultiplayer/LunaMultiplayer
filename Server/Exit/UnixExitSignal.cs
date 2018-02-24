using Mono.Unix;
using System;
using System.Threading.Tasks;

namespace Server.Exit
{
    public class UnixExitSignal : IExitSignal
    {
        public event EventHandler Exit;

        private readonly UnixSignal[] _signals = {
            new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
            new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
            new UnixSignal(Mono.Unix.Native.Signum.SIGUSR1)
        };

        public UnixExitSignal()
        {
            Task.Factory.StartNew(() =>
            {
                // blocking call to wait for any kill signal
                var index = UnixSignal.WaitAny(_signals, -1);

                Exit?.Invoke(null, EventArgs.Empty);

            });
        }

    }
}
