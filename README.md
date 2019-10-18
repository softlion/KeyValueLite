# KeyValueLite

A netstandard key value store, backed by sqlite, alternative to Akavache

(c)2018-2019 Benjamin Mayrargue  
MIT License  

[![NuGet](https://img.shields.io/nuget/v/vapolia-keyvaluelite.svg?style=plastic)](https://www.nuget.org/packages/vapolia-keyvaluelite/)

# Features

* Just works
* Async operations
* Thread safe
* Fast enough
* Direct write on underlying sqlite database. No need to flush.
* Stores DateTime/DateTimeOffset using ticks, not strings, preserving nanoseconds
* Akavache interface for easy and fast migration: use the `KeyValueLiteLikeAkavache` class.

# Setup

Add the nuget to your netstandard project  
[![NuGet](https://img.shields.io/nuget/v/vapolia-keyvaluelite.svg?style=plastic)](https://www.nuget.org/packages/vapolia-keyvaluelite/)

Initialization code to get the `cacheService` singleton instance:

```csharp
//Init SQLite
SQLitePCL.Batteries_V2.Init();

//Use an already existing logger factory, or create a new one with this code:
var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
var logger = loggerFactory.CreateLogger(nameof(KeyValueLite));

//Create the cacheService using the sqlite database path provided by the DataStoreFactory class 
//Note: you can provide your own IDataStoreFactory implementation.
var dsFactory = new DataStoreFactory(new GenericPlatformService());
cacheService = new KeyValueLite(dsFactory, new KeyValueItemNewtonsoftJsonSerializer(), logger);
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


# About Newtonsoft.Json versus System.Text.Json

Starting from version 3.0.0, the dependency on Newtonsoft.Json is removed and replaced by System.Text.Json in Vapolia.KeyValueLite.

If you prefer not to use these Json libraries, implement your own IKeyValueItemSerializer and use the Vapolia.KeyValueLite.Core nuget.
Check the `KeyValueItemSytemTextJsonSerializer` and `KeyValueItemNewtonsoftJsonSerializer` classes for a sample implementation of the IKeyValueItemSerializer interface.
