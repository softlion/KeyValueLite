using System;
using System.IO;
using SQLite.Net.Interop;

namespace Vapolia.KeyValueLite
{
    public class DataStoreFactory : IDataStoreFactory
    {
        private readonly ISQLitePlatform platform;
        private readonly IPlatformService platformService;
        private readonly string baseFolder;

        public DataStoreFactory(ISQLitePlatform platform, IPlatformService platformService)
        {
            this.platform = platform;
            this.platformService = platformService;
            baseFolder = platformService.GetDatabaseFolder();
        }

        IDataStore IDataStoreFactory.CreateDataStore(string dataStoreName) 
            => new DataStore(platform, platformService).Create(GetDataStorePathName(dataStoreName));

        public virtual string GetDataStorePathName(string dataStoreName)
        {
            switch (dataStoreName)
            {
                case nameof(KeyValueLite):
                    return "keyvaluecache.db";
                default:
                    throw new ArgumentException("Unknown data store, or not implemented", nameof(dataStoreName));
            }
        }

        protected virtual string GetDbPathNameForCurrentUser(string dbName)
        {
            var userFolder = baseFolder; //Path.Combine(baseFolder,userSession.UserFolderName);
            var dbPathName = Path.Combine(userFolder, dbName);

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            return dbPathName;
        }
    }
}