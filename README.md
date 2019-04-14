# KeyValueLite

A netstandard key value store, backed by sqlite, alternative to Akavache

(c)2018-2019 Benjamin Mayrargue  
MIT License  

# Features

* Just works
* Async operations
* Thread safe
* Fast enough
* Direct write on underlying sqlite database. No need to flush.
* Stores DateTime/DateTimeOffset using ticks, not strings, preserving nanoseconds
* Akavache interface for easy and fast migration: use the `KeyValueLiteLikeAkavache` class.

# Setup

Add to your netstandard project:
* Nuget of this project (soon available).

Add to your executable projects:
* Add Microsoft.Extensions.Logging
* SQLitePCLRaw.bundle_e_sqlite3
* SQLite.Net-PCL (currently private)

Initialization code:

```csharp
SQLitePCL.Batteries_V2.Init();

var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
var logger = new Microsoft.Extensions.Logging.Logger<KeyValueLite>(loggerFactory);

var dataStoreFactory = new DataStoreFactory(new SQLitePlatform(), new GenericPlatformService());
cacheService = new KeyValueLite(dataStoreFactory, new KeyValueItemNewtonsoftJsonSerializer(), logger);
```

# Usage

## Get a value, or create it if it does not exist
**Usage scenario**   
Get an object from the cache, or create it synchronously  
Get an object from the cache, or fetch it from a webservice. Optionaly set an expiration time.

```csharp
Task<T> GetOrCreateObject<T>(string key, Func<T> create)
Task<T> GetOrFetchObject<T>(string key, Func<Task<T>> create, DateTimeOffset? expiresOn = null)
```


Samples:
```csharp
var value = await GetOrCreateObject("sample key", () => "sample string value")
var value = await GetOrCreateObject("sample key", () => new SomeObject { SomeProperty = 12 })

var value = await GetOrFetchObject("sample key", async () => await httpClient.GetAync("https://happy/api/method"));
```

## Add values
Add a list of value  
Persists the specified key, value and expiration, updating it if the key already exists.

```csharp
Task InsertObjects<T>(Dictionary<string, T> keyValuePairs, DateTimeOffset? expiresOn = null)
Task Set(string key, object value, DateTimeOffset? expiresOn = null)
```

Samples:
```csharp
await InsertObjects(new Dictionary<string,IPAddress>() { {"someKey", someIp}, {"someKey2", someIp2} }, DateTimeOffset.Now.AddDays(1));
await Set("someKey", someObject, DateTimeOffset.Now.AddMinutes(60));
```

## Retrieve a value
**Usage scenario**   
Get an object from the cache, or null if it has expired or is not in the cache  
Get all objects of this type from the cache

```csharp
Task<T> Get<T>(string key)
Task<List<T>> GetAll<T>()
```

Samples:
```csharp
var value = await Get<string>("sample key");
var values = await GetAll<string>();
```


## Delete a value
**Usage scenario**   
Delete an object from the cache
Delete all objects of this type from the cache  

```csharp
Task Remove(string key)
Task RemoveAll<T>()
```

Samples:
```csharp
await Remove("the key");
await RemoveAll<string>();
```

If the key does not exist, it does nothing.




# Usage (advanced)
Get the internal keyValueItem matching this key, or null
```csharp
var keyValueItem = await keyValueStore.Get("sample key");
```

Persists the specified `KeyValueItem`, updating it if the key already exists.
```csharp
Task Set(KeyValueItem keyValueItem);
```

Removes the specified key value item.
```csharp
Task Remove(KeyValueItem keyValueItem)
```


# About Newtonsoft.Json

If you prefer not to use this Json library, implement your own IKeyValueItemSerializer and use the Core nuget.  
See `KeyValueItemNewtonsoftJsonSerializer` for an example of IKeyValueItemSerializer implementation.
