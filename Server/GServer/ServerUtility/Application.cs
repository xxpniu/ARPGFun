using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerUtility
{
    public class App:ServerApp<App>
    {

        private Func<App, Task>? _s;
        private Func<App,Task>? _t, _e;

        public App Create(Func<App, Task> setup = default!, Func<App,Task> tick =default!, Func<App,Task> stop=default!)
        {
            _s = setup;
            _t = tick;
            _e = stop;
            return this;
        }

        protected override async Task Start(CancellationToken token = default)
        {
            if(_s==null) return;
            await _s.Invoke(this);
        }
        
        protected override async Task Stop(CancellationToken token =default)
        {
            if (_e == null) return;
            await _e?.Invoke(this)!;
        }
    }
}
