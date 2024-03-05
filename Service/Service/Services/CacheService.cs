using StackExchange.Redis;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Service.Services
{
    public class CacheService
    {
        const int ExpirePeriod = 10; //sec

        private IDatabase _db;
        private readonly IConfiguration _config;

        public CacheService(IConfiguration config)
        {
            _config = config;
            var connection = ConnectionMultiplexer.Connect(_config["RedisURL"]);
            _db = connection.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _db.StringGet(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default;
        }
        public bool SetData<T>(string key, T value)
        {
            TimeSpan expiryTime = DateTimeOffset.Now.AddSeconds(ExpirePeriod).DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonConvert.SerializeObject(value), expiryTime);
            return isSet;
        }
        public object RemoveData(string key)
        {
            bool _isKeyExist = _db.KeyExists(key);
            if (_isKeyExist == true)
            {
                return _db.KeyDelete(key);
            }
            return false;
        }
    }
}
