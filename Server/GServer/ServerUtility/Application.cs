using System;
using System.Threading.Tasks;

namespace ServerUtility
{
    public class App:XSingleton<App>
    {
        private volatile bool IsRunning;

        public async Task Startup(Func<App, Task> setup =null)
        {
            IsRunning = true;
            if (setup == null) return;
            await setup.Invoke(this);
        }


        public async Task Stop(Func<App, Task> stop =null)
        {
            IsRunning = false;
            if (stop == null) return;
            await stop.Invoke(this);
        }


        public async Task Tick(Func<App, Task> tick = null,int tickDelay = 100)
        {
            while (IsRunning)
            {
                if (tick != null)
                {
                    await tick.Invoke(this);
                }
                await Task.Delay(tickDelay);
            }
        }
    }
}
