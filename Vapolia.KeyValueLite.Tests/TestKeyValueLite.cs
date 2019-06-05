using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Vapolia.KeyValueLite.Core;

namespace Vapolia.KeyValueLite.Tests
{
    [TestClass]
    public class TestKeyValueLite
    {
        private KeyValueLite cacheService;

        [TestInitialize]
        public void TestInit()
        {
            SQLitePCL.Batteries_V2.Init();
            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var logger = loggerFactory.CreateLogger(nameof(KeyValueLite));

            //Clear db
            //var dbPath = dsFactory.GetDataStorePathName(nameof(KeyValueLite));
            //File.Delete(dbPath);

            var dsFactory = new DataStoreFactory(new GenericPlatformService());
            cacheService = new KeyValueLite(dsFactory, new KeyValueItemNewtonsoftJsonSerializer(), logger);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            //Add an object
            var test1Object = new TestObject1 { Id = 1, DateCreated = new DateTimeOffset(2019, 4, 14, 20, 02, 26, 127, TimeSpan.FromHours(2)) };
            var result1 = await cacheService.GetOrCreateObject<TestObject1>("test1", () => test1Object);
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, result1.Id);
            Assert.AreEqual(26, result1.DateCreated.Second);
            Assert.AreEqual(127, result1.DateCreated.Millisecond);

            //Get it
            result1 = await cacheService.Get<TestObject1>("test1");
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, result1.Id);
            Assert.AreEqual(26, result1.DateCreated.Second);
            Assert.AreEqual(127, result1.DateCreated.Millisecond);

            //Remove it
            await cacheService.Remove("test1");
            result1 = await cacheService.Get<TestObject1>("test1");
            Assert.IsNull(result1);
        }
    }

    public class TestObject1
    {
        public int Id { get;set;}
        public DateTimeOffset DateCreated { get;set;}
    }
}
