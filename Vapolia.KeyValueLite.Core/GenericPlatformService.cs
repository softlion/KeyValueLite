using System;
using System.IO;

namespace Vapolia.KeyValueLite.Core
{
    [Preserve(AllMembers = true)]
    public class GenericPlatformService : IPlatformService
    {
        public string GetDatabaseFolder()
        {
            var path = Path.Combine(GetLibraryFolder(), "private");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (String.IsNullOrWhiteSpace(path))
                throw new Exception("Db directory is null: can not create database");

            return path;
        }

        public virtual string GetOrCreateDatabase(string dbPathName)
        {
            return dbPathName;
        }

        protected virtual string GetLibraryFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }
}
