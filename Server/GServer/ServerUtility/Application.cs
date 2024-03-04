using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerUtility
{
    public class App:ServerApp<App>
    {

        private Func<App, CancellationToken , Task>? _s;
        private Func<App, CancellationToken ,Task>? _t, _e;

        public App Create(Func<App,CancellationToken, Task> setup = default!, Func<App,CancellationToken,Task> tick =default!, Func<App,CancellationToken,Task> stop=default!)
        {
            _s = setup;
            _t = tick;
            _e = stop;
            return this;
        }

        protected override async Task Start(CancellationToken token = default)
        {
            if(_s==null) return;
            await _s.Invoke(this,token);
        }
        
        protected override async Task Stop(CancellationToken token =default)
        {
            if (_e == null) return;
            await _e?.Invoke(this,  token)!;
        }
    }
}
