using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Nancy.Cryptography;
using RedisWrapper;

namespace Core.Web
{

    public class RedisBasedKeyGenerator : IKeyGenerator
    {
        private readonly RedisServer _redis;

        private readonly RandomNumberGenerator _randomGenerator = RandomNumberGenerator.Create();

        private Dictionary<int, byte[]> _cachedKeys;
        private Dictionary<int, DateTime> _timeOfLastFetchLookup;
        private TimeSpan _secondsBetweenFetch = TimeSpan.FromSeconds(60);

        public RedisBasedKeyGenerator(RedisServer redis)
        {
            _redis = redis;
            _cachedKeys = new Dictionary<int, byte[]>();
            _timeOfLastFetchLookup = new Dictionary<int, DateTime>();
        }

        public byte[] FetchSharedBytes(int count)
        {
            var redisKeyIdentifier = GenerateRedisKey(count);

            // Todo: Somehow rotate the key better.
            var keyBytes = Encoding.ASCII.GetString(GenerateRandomBytes(count));

            if (!_redis.SetRawValue(RedisDBKeyTypes.FreeconCsrfKey,  redisKeyIdentifier, keyBytes, null, SetWhen.NotExists))
            {
                keyBytes = _redis.GetRawValue(RedisDBKeyTypes.FreeconCsrfKey, redisKeyIdentifier);
            }

            _cachedKeys[count] = Encoding.ASCII.GetBytes(keyBytes);

            _timeOfLastFetchLookup[count] = DateTime.Now;

            return _cachedKeys[count];
        }

        public string GenerateRedisKey(int count)
        {
            return count.ToString();
        }

        public byte[] GenerateRandomBytes(int count)
        {
            var randomBytes = new byte[count];

            _randomGenerator.GetBytes(randomBytes);

            return randomBytes;
        }

        public byte[] GetBytes(int count)
        {
            DateTime lastFetch;
            var found = _timeOfLastFetchLookup.TryGetValue(count, out lastFetch);

            // If our key is expired or we just don't have it.
            if (found && DateTime.Now - lastFetch > _secondsBetweenFetch || !found)
            {
                return FetchSharedBytes(count);
            }

            return _cachedKeys[count];
        }
    }
}
