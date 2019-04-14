using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Vapolia.KeyValueLite
{
    public class KeyValueLiteLikeAkavache
    {
        private readonly KeyValueLite kvLite;

        public KeyValueLiteLikeAkavache(IDataStoreFactory dsFactory, IKeyValueItemSerializer serializer, ILogger logger)
        {
            kvLite = new KeyValueLite(dsFactory, serializer, logger);
        }

        public async Task<T> GetOrCreateObject<T>(string key, Func<T> create, [CallerMemberName]string caller = null) 
            => await kvLite.GetOrCreateObject(key, create);

        public async Task Invalidate(string key, [CallerMemberName]string caller = null) 
            => await kvLite.Remove(key);

        public async Task InsertObject<T>(string key, T value, DateTimeOffset? expiresOn = null, [CallerMemberName]string caller = null) 
            => await kvLite.Set(key, value, expiresOn);

        public async Task<T> GetOrFetchObject<T>(string key, Func<Task<T>> loadCache, DateTimeOffset? expiresOn = null, [CallerMemberName]string caller = null) 
            => await kvLite.GetOrFetchObject(key, loadCache, expiresOn);

        public async Task<T> GetObject<T>(string key, [CallerMemberName]string caller = null) 
            => await kvLite.Get<T>(key);

        public async Task<IEnumerable<T>> GetAllObjects<T>([CallerMemberName]string caller = null) 
            => await kvLite.GetAll<T>();

        public async Task InsertObjects<T>(Dictionary<string, T> keyValuePairs, [CallerMemberName]string caller = null) 
            => await kvLite.InsertObjects<T>(keyValuePairs);

        public async Task InvalidateAllObjects<T>([CallerMemberName]string caller = null) 
            => await kvLite.RemoveAll<T>();

        public async Task InvalidateAll([CallerMemberName]string caller = null) 
            => await kvLite.RemoveAll();
    }
}