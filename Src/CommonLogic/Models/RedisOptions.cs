namespace StackExchange.Redis
{
    /// <summary>
    /// Any options to be used to configure a Redis service
    /// </summary>
    internal sealed class RedisOptions
    {
        /// <summary>
        /// The Host of the Redis server
        /// </summary>
        public string? Host { get; set; }
    }
}
