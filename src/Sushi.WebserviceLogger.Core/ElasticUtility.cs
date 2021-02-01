using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Provides utility methods for Elastic.
    /// </summary>
    public static class ElasticUtility
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> IndexCache = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();
        private static readonly SemaphoreSlim CreateLock = new SemaphoreSlim(1);
        /// <summary>
        /// Create a new index for the given <paramref name="indexName"/> or updates the existing index with the mapping defined on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public static async Task<bool> CreateIndexIfNotExistsAsync<T>(ElasticClient client, string indexName) where T : class
        {
            bool result = false;
            if (!IndexCache.ContainsKey(indexName))
            {
                //create index if not exists, otherwise update mapping
                var indexExists = await client.Indices.ExistsAsync(indexName);
                //dynamic mapping is set to strict, so an exception will be thrown if we try to index documents with unmapped properties            
                var dynamicMapping = DynamicMapping.Strict;
                if (!indexExists.Exists)
                {
                    // use double check so only 1 thread can create it
                    await CreateLock.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        indexExists = await client.Indices.ExistsAsync(indexName);
                        if (!indexExists.Exists)
                        {
                            var createResponse = await client.Indices.CreateAsync(indexName, c => c.Map<T>(p => p.AutoMap().Dynamic(dynamicMapping))).ConfigureAwait(false);
                            if (!createResponse.IsValid)
                                throw new Exception("Failed to create index " + indexName);
                            result = true;
                        }
                    }
                    finally
                    {
                        CreateLock.Release();
                    }
                }
                else
                {
                    //update index mapping
                    await client.Indices.PutMappingAsync<T>(p => p.AutoMap().Index(indexName).Dynamic(dynamicMapping)).ConfigureAwait(false);
                }
                IndexCache.TryAdd(indexName, new object());
            }
            return result;
        }

        /// <summary>
        /// Create a new index for the given <paramref name="indexName"/> or updates the existing index with the mapping defined on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public static bool CreateIndexIfNotExists<T>(ElasticClient client, string indexName) where T : class
        {
            bool result = false;
            if (!IndexCache.ContainsKey(indexName))
            {
                //create index if not exists, otherwise update mapping
                var indexExists = client.Indices.Exists(indexName);
                //dynamic mapping is set to strict, so an exception will be thrown if we try to index documents with unmapped properties            
                var dynamicMapping = DynamicMapping.Strict;
                if (!indexExists.Exists)
                {
                    // use double check so only 1 thread can create it
                    CreateLock.Wait();
                    try
                    {
                        indexExists = client.Indices.Exists(indexName);
                        if (!indexExists.Exists)
                        {
                            var createResponse = client.Indices.Create(indexName, c => c.Map<T>(p => p.AutoMap().Dynamic(dynamicMapping)));
                            if (!createResponse.IsValid)
                                throw new Exception("Failed to create index " + indexName);
                            result = true;
                        }
                    }
                    finally
                    {
                        CreateLock.Release();
                    }
                }
                else
                {
                    //update index mapping
                    client.Indices.PutMapping<T>(p => p.AutoMap().Index(indexName).Dynamic(dynamicMapping));
                }
                IndexCache.TryAdd(indexName, new object());
            }
            return result;
        }
    }
}
