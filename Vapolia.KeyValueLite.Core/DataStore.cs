using SQLite.Net.Interop;

namespace Vapolia.KeyValueLite
{
    public interface IDataStoreFactory
    {
        IDataStore CreateDataStore(string dataStoreName);
    }

    //Sample usage:
    //Mvx.RegisterSingleton(new DataStoreFactory(userSession, Mvx.Resolve<IPlatformService>()));
    //
    //public class DataStoreFactory : IDataStoreFactory
    //{
    //    private readonly UserSession userSession;
    //    private readonly string baseFolder;

    //    public DataStoreFactory(UserSession userSession, IPlatformService platformService)
    //    {
    //        this.userSession = userSession;
    //        baseFolder = platformService.GetDatabaseFolder();
    //    }

    //    public IDataStore CreateDataStore(string dataStoreName)
    //    {
    //        switch (dataStoreName)
    //        {
    //            case nameof(MainDbService):
    //                return GetDbPathNameForCurrentUser("main.db");
    //            case nameof(ObjectCacheService):
    //                return GetDbPathNameForCurrentUser("ojectcache.db");
    //            case nameof(SafeStorageService):
    //                return GetDbPathNameForCurrentUser("safestorage.db");
    //            case nameof(ImageCacheService):
    //                return Mvx.IoCConstruct<DataStore>().Create(Path.Combine(baseFolder, "imagecache.db"));
    //            default:
    //                throw new ArgumentException("Unknown data store, or not implemented", nameof(dataStoreName));
    //        }
    //    }

    //    private IDataStore GetDbPathNameForCurrentUser(string dbName)
    //    {
    //        var dbPath = Path.Combine(baseFolder,userSession.UserFolderName);
    //        var dbPathName = Path.Combine(dbPath, dbName);

    //        if(!Directory.Exists(dbPath))
    //            Directory.CreateDirectory(dbPath);

    //        //Migrate from version 3.0 (2018/5/30)
    //        var oldPathName = Path.Combine(baseFolder, dbName);
    //        if (File.Exists(oldPathName))
    //            File.Move(oldPathName, dbPathName);

    //        return Mvx.IoCConstruct<DataStore>().Create(dbPathName);
    //    }
    //}

    public class DataStore : BaseDataStore
    {
        [Preserve]
        public DataStore(ISQLitePlatform platform, IPlatformService ps) : base(platform, ps)
        {
        }
    }

    #region infra
    public interface IPlatformService
    {
        string GetOrCreateDatabase(string dbPathName);
        string GetDatabaseFolder();
    }

    public abstract class BaseDataStore : IDataStore
    {
        public string DatabaseFilePathName { get; private set; }
        public ISQLitePlatform Platform { get; }
        private IPlatformService ps;

        protected BaseDataStore(ISQLitePlatform platform, IPlatformService ps)
        {
            Platform = platform;
            this.ps = ps;
        }

        public IDataStore Create(string dbPathName)
        {
            DatabaseFilePathName = ps.GetOrCreateDatabase(dbPathName);
            ps = null;
            return this;
        }
    }
    #endregion
}
