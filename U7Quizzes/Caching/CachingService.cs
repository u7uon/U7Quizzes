
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace U7Quizzes.Caching
{
    public class CachingService : ICachingService
    {
        private readonly IDistributedCache _cache;

        public CachingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> Get<T>(string key)
        {
            ValidateKey(key);

            var data = await _cache.GetStringAsync(key);

            return data != null ? JsonSerializer.Deserialize<T>(data) : default;

        }

        public async Task Set<T>(T data, string key)
        {
            ValidateKey(key);


            var json = JsonSerializer.Serialize(data);

            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        }


        public async Task Remove(string key)
        {
            ValidateKey(key);
            await _cache.RemoveAsync(key);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Cache key cannot be null or empty");
        }

        public string GenerateKey<T>(T filter)
        {
            var key = new StringBuilder();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var value = property.GetValue(filter);
                if (value == null) continue;

                key.Append(property.Name + "=");

                if (value is IEnumerable<int> list)
                {
                    key.Append(string.Join(",", list));
                }
                else
                {
                    key.Append(value.ToString());
                }

                key.Append(";");
            }

            return  HashKey( key.ToString().TrimEnd(';'));
        }


        private string HashKey(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // e.g., A1B2C3D4E...
        }
    }  
}
