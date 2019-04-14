using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vapolia.KeyValueLite
{
    /// <summary>
    /// usage:
    /// 
    /// using(await Syncer.Wait(sync)) { ...  }
    /// </summary>
    public class Syncer
    {
        private readonly ILogger log;

        public Syncer(ILogger logger)
        {
            log = logger;
        }

        public async Task<IDisposable> Wait(SemaphoreSlim sync)
        {
            try
            {
                //Log.Logger.Verbose("Syncer WaitAsync lock WAITING");
                await sync.WaitAsync(40000);
                //Log.Logger.Verbose("Syncer WaitAsync lock ON");
            }
            catch (Exception e)
            {
                log.LogError(e, $"Syncer WaitAsync lock FAILED (timeout)");
            }
            return new SyncDisposer(sync);
        }

        sealed class SyncDisposer : IDisposable
        {
            private readonly SemaphoreSlim sync;

            public SyncDisposer(SemaphoreSlim sync)
            {
                this.sync = sync;
            }

            public void Dispose()
            {
                //Log.Logger.Verbose("Syncer WaitAsync lock OFF");
                sync.Release();
            }
        }
    }
}