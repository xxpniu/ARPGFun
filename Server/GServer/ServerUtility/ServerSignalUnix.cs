#define MONO
using System;
using System.Threading.Tasks;
#if MONO
using Mono.Unix;
using Mono.Unix.Native;
#endif

namespace ServerUtility
{
    public class UnixExitSignal
    {
        public event EventHandler Exit;
#if MONO
        readonly UnixSignal[] signals = new UnixSignal[]
        {
        new UnixSignal(Signum.SIGTERM),
        new UnixSignal(Signum.SIGINT),
        new UnixSignal(Signum.SIGUSR1)
        };
#endif

        public Task CurrentWait { private set; get; }

        public UnixExitSignal()
        {
            CurrentWait = Task.Factory.StartNew(() =>
             {
#if MONO
                 // blocking call to wait for any kill signal
                 UnixSignal.WaitAny(signals, -1);
                 Exit?.Invoke(this, EventArgs.Empty);
#endif
             });
        }
    }
}

