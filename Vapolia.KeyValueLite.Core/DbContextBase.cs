using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SQLite.Net;
using SQLite.Net.Interop;

namespace Vapolia.KeyValueLite
{
    public interface IDataStore
    {
        string DatabaseFilePathName { get; }
        ISQLitePlatform Platform { get; }
    }

    //internal interface IDbContextBuilder<TDbContext, TDataStore>
    //    where TDataStore : class, IDataStore
    //    where TDbContext : DbContextBase<TDbContext, TDataStore>, IDbContextBuilder<TDbContext, TDataStore>
    //{
    //    DbContextBase<TDbContext, TDataStore> CreateDbContext();
    //}

    public abstract class DbContextBase<TDbContext> : SQLiteConnection
        where TDbContext : DbContextBase<TDbContext> //, IDbContextBuilder<TDbContext, TDataStore>
    {
        private const string AppVersionKey = "appVersion";
        private const string OsVersionKey = "osVersion";
        private const string SchemaVersionKey = "schemaVersion";
        private const string SetWalMode = "PRAGMA journal_mode = WAL;";
        private readonly ILogger log;

        //private readonly IDataStore dataStore;

        /// <summary>
        /// After any change to db schema, you should increase CurrentSchemaVersion, 
        /// which by default destroy/recreate the existing db on the device.
        /// Alternatively you can implement each migration path in CheckSchemaVersion.
        /// </summary>
        protected abstract int CurrentSchemaVersion { get; }

        protected abstract void CreateUserTables(SQLiteConnection db);

        /// <returns>true to stop database update, as migrate can recreate a new db</returns>
        protected virtual bool Migrate(int previousVersion)
        {
            return false;
        }

        //static DbContextBase()
        //{
        //    InitDb();
        //}

        /// <summary>
        /// NoMutex: multi threaded mode
        /// FullMutex: serialized mode
        /// Doc: https://www.sqlite.org/threadsafe.html
        /// 
        /// Do not use SQLiteOpenFlags.SharedCache as it changes the locking model, as described here
        /// https://www.sqlite.org/sharedcache.html
        /// </summary>
        protected DbContextBase(IDataStore dataStore, ILogger logger) : base(
            dataStore.Platform,
            dataStore.DatabaseFilePathName,
            SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.NoMutex
            //Protection flag required for db access when device is locked
            | SQLiteOpenFlags.ProtectionCompleteUntilFirstUserAuthentication)
        {
            log = logger;
            //this.dataStore = dataStore;
        }

        protected void DropAllTables()
        {
            var tables = new Queue<string>(ExecuteSimpleQuery<string>("select name from sqlite_master where type = 'table' and name not like 'sqlite_%'").ToList());
            while (tables.Count != 0)
            {
                var table = tables.Dequeue();
                try
                {
                    Execute($"drop table `{table}`");
                }
                catch (SQLiteException)
                {
                    tables.Enqueue(table);
                }
            }
        }

        /// <summary>
        /// Must be static, as this class inherits SQLiteConnection which contructor always opens a connection.
        /// And we need all connections closed.
        /// </summary>
        public static void ResetDatabase(TDbContext db, IDataStore dataStore)
        {
            //Completely delete db
            if(File.Exists(dataStore.DatabaseFilePathName))
                File.Delete(dataStore.DatabaseFilePathName);
            InitDb(db, true);
        }

        /// <summary>
        /// 
        /// Creates the database.
        /// Check if this is a new app installation, or an update.
        /// Set some settings flags.
        /// </summary>
        public static TDbContext InitDb(TDbContext db, bool isReset = false)
        {
            var logger = db.log;
            logger.LogInformation($"InitDb started {typeof(TDbContext).Name}");

            //using (var db = new TDbContext())
            {
                db.CreateTableIfNotExist<DbMeta>();
                var schemaVersion = db.Table<DbMeta>().FirstOrDefault(m => m.Key == SchemaVersionKey);
                var isDbJustCreated = schemaVersion == null;

                //var currentAppVersion = new DbMeta
                //{
                //    Key = AppVersionKey,
                //    Value = Mvx.Resolve<ICoolSettings>().AppVersion.ToString()
                //};

                //var currentOsVersion = new DbMeta
                //{
                //    Key = OsVersionKey,
                //    Value = Mvx.Resolve<IHardware>().OperatingSystem
                //};

                if (isDbJustCreated)
                {
                    //Always use WAL mode. Set it once, as wal mode is persistent
                    if (db.ExecuteScalar<string>(SetWalMode) != "wal")
                        logger.LogWarning("Can not switch to WAL mode: unknown problem. Concurrent access will fail.");

                    //default_cache_size is persistent
                    db.Execute("PRAGMA default_cache_size = 2000;");

                    db.CreateTables();
                    db.IsDbJustCreated = true;
                }
                else
                {
                    var ver = Convert.ToInt32(schemaVersion.Value);
                    if (ver != db.CurrentSchemaVersion)
                    {
                        db.Migrate(ver);
                        //update version after migration succeeded
                        schemaVersion.Value = db.CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture);
                        db.Update(schemaVersion);
                    }

                    //var hasAppVersionChanged = true;
                    //var appVersion = db.Table<DbMeta>().FirstOrDefault(m => m.Key == AppVersionKey);
                    //if (appVersion == null)
                    //    db.Insert(currentAppVersion);
                    //else if (appVersion.Value != currentAppVersion.Value)
                    //{
                    //    appVersion.Value = currentAppVersion.Value;
                    //    db.Update(appVersion);
                    //}
                    //else
                    //    hasAppVersionChanged = false;

                    //var hasOsVersionChanged = true;
                    //var osVersion = db.Table<DbMeta>().FirstOrDefault(m => m.Key == OsVersionKey);
                    //if (osVersion == null)
                    //    db.Insert(currentOsVersion);
                    //else if (osVersion.Value != currentOsVersion.Value)
                    //{
                    //    osVersion.Value = currentOsVersion.Value;
                    //    db.Update(osVersion);
                    //}
                    //else
                    //    hasOsVersionChanged = false;

                    //db.HasOsVersionChanged = hasOsVersionChanged;
                    //db.HasAppVersionChanged = hasAppVersionChanged;
                }

                db.BusyTimeout = TimeSpan.FromSeconds(5);

				var threadSafe = db.Platform.SQLiteApi.Threadsafe();
				// 1: Thread-safe. Lock statements not required.
				// 2: Mutexing code is there, but mutexing on database connection and prepared statement objects is disabled.
				//    Application is responsible for serializing access to database connections and prepared statements, so must use lock statements.
				// Other: SQLite was compiled with mutexing code omitted. It is not safe to use SQLite concurrently from more than one thread.
				var sqliteVersion = db.ExecuteScalar<string>("select sqlite_version()");
                logger.LogInformation($"Using SQLite version {sqliteVersion} threadSafe:{threadSafe} {(threadSafe==1 ? "Serialized (thread safe)" : threadSafe==2 ? "Multi-Threaded (lock required)" : "Mutexing code omitted (not thread safe)")}");
			}

            return db;
        }

        //protected virtual bool HasOsVersionChanged { get; set; }
        //protected virtual bool HasAppVersionChanged { get; set; }
        protected virtual bool IsDbJustCreated { get; set; }

        protected void CreateTables()
        {
            this.CreateTableIfNotExist<DbMeta>();
            var schemaVersion = new DbMeta { Key = SchemaVersionKey, Value = CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture) };
            Insert(schemaVersion);
            //Insert(currentAppVersion);
            //Insert(currentOsVersion);
            CreateUserTables(this);
        }


        //private void ResetDatabase()
        //{
        //    //Drop all tables
        //    const string alltablesQuery = "select 'drop table ' || name || ';' from sqlite_master where type = 'table'";
        //    var droptablesQuery = ExecuteSimpleQuery<string>(alltablesQuery)
        //        .Where(s => !s.StartsWith("drop table sqlite"))
        //        .ToList();
        //    foreach (var dropTableQuery in droptablesQuery)
        //        Execute(dropTableQuery);
        //    InitDb();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Close();
            base.Dispose(disposing);
        }

        #region SQL Helpers
        public int Insert<T>(T obj, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                return Insert(obj, typeof(T));
            }
            catch (Exception e)
            {
                log.LogError(e, "SQLite Insert error '{3}' {0} in {1}:{2}", memberName, sourceFilePath, sourceLineNumber, e.Message);
                throw;
            }
        }

        public int InsertAll(IEnumerable objects, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return RunInTransaction2(() => InsertAll(objects, false));
        }

        private T RunInTransaction2<T>(Func<T> action)
        {
            var savePoint = SaveTransactionPoint();
            try
            {
                var result = action();
                Release(savePoint);
                return result;
            }
            catch (SQLiteException)
            {
                try
                {
                    RollbackTo(savePoint);
                }
                catch (SQLiteException)
                {
                }

                throw;
            }
        }
        #endregion
    }

    public static class SQLConnectionExtensions
    {
        public static void CreateTableIfNotExist<T>(this SQLiteConnection db)
        {
            var table = typeof(T).Name;
            var query = "SELECT name FROM sqlite_master WHERE name ='" + table + "' and type='table' limit 1";
            var name = db.ExecuteScalar<string>(query);
            if (name == null)
                db.CreateTable<T>();
        }
    }
}

