using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Vapolia.KeyValueLite
{
    public class KeyValueLite : IDisposable
    {
        private readonly KeyValueDbContext db;
        private readonly SemaphoreSlim syncDb = new SemaphoreSlim(1);
        private readonly Syncer syncer;
        private readonly IKeyValueItemSerializer serializer;

        [Preserve(Conditional = true)]
        public KeyValueLite(IDataStoreFactory dsFactory, IKeyValueItemSerializer serializer, ILogger logger)
        {
            syncer = new Syncer(logger);
            this.serializer = serializer;
         
            var datastore = dsFactory.CreateDataStore(nameof(KeyValueLite));
            db = new KeyValueDbContext(datastore, logger);
            KeyValueDbContext.InitDb(db);
        }

        /// <summary>
        /// Retrieves a <code>KeyValueItem</code> based on the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>KeyValueItem if it exists or null otherwise. 
        /// If an expiration was configured and is past the current time, it will also return null.</returns>
        public virtual async Task<KeyValueItem> Get(string key)
        {
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                var kp = (from item in db.Table<KeyValueItem>()
                        where item.Key == key
                        //where item.ExpiresOn == null || item.ExpiresOn > now
                        select item
                    ).FirstOrDefault();

                if (kp?.ExpiresOn != null && kp.ExpiresOn.Value < DateTimeOffset.UtcNow)
                {
                    db.Delete(kp);
                    kp = null;
                }

                return kp;
            }
        }

        public virtual async Task<T> GetOrCreateObject<T>(string key, Func<T> create)
        {
            T value;

            var kp = await Get(key).ConfigureAwait(false);
            if (kp == null)
            {
                value = create();
                if(value != null)
                    await Set(key, value).ConfigureAwait(false);
            }
            else
            {
                value = serializer.GetValue<T>(kp);
            }

            return value;
        }

        public virtual async Task<T> GetOrFetchObject<T>(string key, Func<Task<T>> create, DateTimeOffset? expiresOn = null)
        {
            T value;

            var kp = await Get(key).ConfigureAwait(false);
            if (kp == null)
            {
                value = await create();
                if(value != null)
                    await Set(key, value).ConfigureAwait(false);
            }
            else
            {
                value = serializer.GetValue<T>(kp);
            }

            return value;
        }



        /// <summary>
        /// Retrieves a strongly typed value corresponding to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value corresponding to the key if it exists or null otherwise. 
        /// If an expiration was configured and is past the current time, it will also return null.</returns>
        public virtual async Task<T> Get<T>(string key)
        {
            var kp = await Get(key).ConfigureAwait(false);
            if(kp == null)
                return default;
            return serializer.GetValue<T>(kp);
        }

        public virtual async Task<List<T>> GetAll<T>()
        {
            var valueType = typeof(T).FullName;
            var now = DateTimeOffset.UtcNow;

            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                var expiredItemIds = db.Table<KeyValueItem>()
                    .Where(kv => kv.ValueType == valueType && kv.ExpiresOn != null && kv.ExpiresOn < now)
                    .Select(kv => kv.Key)
                    .ToList();
                db.DeleteIn<KeyValueItem>(expiredItemIds);
        
                return db.Table<KeyValueItem>()
                    .Where(kv => kv.ValueType == valueType)
                    .Select(kv => kv.Value)
                    .ToList()
                    .Select(item => serializer.GetValue<T>(item))
                    .ToList();
            }
        }

        public virtual async Task RemoveAll<T>()
        {
            var valueType = typeof(T).FullName;

            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                var itemIds = db.Table<KeyValueItem>()
                    .Where(kv => kv.ValueType == valueType)
                    .Select(kv => kv.Key)
                    .ToList();
                db.DeleteIn<KeyValueItem>(itemIds);
            }
        }

        public virtual async Task RemoveAll()
        {
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                db.DeleteAll<KeyValueItem>();
            }
        }

        /// <summary>
        /// Persists the specified <code>KeyValueItem</code>, updating it if the key already exists.
        /// </summary>
        /// <param name="keyValueItem">The key value item.</param>
        public virtual async Task Set(KeyValueItem keyValueItem)
        {
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                db.InsertOrReplace(keyValueItem);
            }
        }

        /// <summary>
        /// Persists the specified key, value and expiration, updating it if the key already exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresOn">The expire date after which this key is no longer valid.</param>
        public virtual Task Set(string key, object value, DateTimeOffset? expiresOn = null)
        {
            var keyValueItem = new KeyValueItem(key, serializer.SerializeToString(value), value?.GetType().FullName, expiresOn);
            return Set(keyValueItem);
        }

        public async Task InsertObjects<T>(Dictionary<string, T> keyValuePairs, DateTimeOffset? expiresOn = null)
        {
            var kps = keyValuePairs.Select(kp => new KeyValueItem(kp.Key, serializer.SerializeToString(kp.Value), kp.Value?.GetType().FullName, expiresOn)).ToList();
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                db.InsertOrReplaceAll(kps);
            }
        }

        /// <summary>
        /// Removes the specified key value item.
        /// </summary>
        /// <param name="keyValueItem">The key value item to remove.</param>
        public virtual async Task Remove(KeyValueItem keyValueItem)
        {
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                db.Delete(keyValueItem);
            }
        }

        /// <summary>
        /// Removes the specified key value item.
        /// </summary>
        /// <param name="key">The key.</param>
        public virtual async Task Remove(string key)
        {
            using (await syncer.Wait(syncDb).ConfigureAwait(false))
            {
                var itemKey = db.Table<KeyValueItem>().Where(kp => kp.Key == key).Select(kp => kp.Key).FirstOrDefault();
                if (itemKey != null)
                    db.Delete<KeyValueItem>(new [] {itemKey});
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }
    }
}
