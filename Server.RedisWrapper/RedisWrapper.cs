using System;
using System.CodeDom;
using System.Collections.Generic;
using StackExchange.Redis;
using Freecon.Core.Networking.Models;
using Core.Utilities;
using System.Threading.Tasks;
using Server.Managers;


namespace RedisWrapper
{
    /// <summary>
    /// Wrapper for redis, allows for controlled publishing, subscribing, and accessing redis DB
    /// </summary>
    public class RedisServer
    {
        private Action<string, string, string> _logDebug;
        private Action<Exception, string, string> _logError;

        ConnectionMultiplexer _redisServer;
        ISubscriber _sub;
        IDatabase _db;
        Random _random = new Random();

        public IDatabase Database { get { return _db; } }

        public RedisServer(Action<Exception, string, string> logError, Action<string, string, string> logDebug, string address)
        {
            _redisServer = ConnectionMultiplexer.Connect(address);
            _db = _redisServer.GetDatabase();
            _sub = _redisServer.GetSubscriber();

            SetLoggers(logError, logDebug);
        }

        public RedisServer(Action<Exception, string, string> logError, Action<string, string, string> logDebug, ConfigurationOptions config)
        {
            _redisServer = ConnectionMultiplexer.Connect(config);
            _db = _redisServer.GetDatabase();
            _sub = _redisServer.GetSubscriber();

            SetLoggers(logError, logDebug);
        }

        protected T DeserializeMsgPack<T>(string key, RedisValue rawData)
            where T : MessagePackSerializableObject, new()
        {
            var encodedData = (byte[])rawData;

            if (encodedData == null)
            {
                _logError(new MsgPackMissingDataException("Data null when fetching from Redis"), key, rawData);
                return null;
            }

            try
            {
                _logDebug("Deserializing Redis message", key, rawData);
                return SerializationUtilities.DeserializeMsgPack<T>(encodedData);
            }
            catch (Exception e)
            {
                _logError(e, key, rawData);
                return null;
            }
        }

        protected void SetLoggers(Action<Exception, string, string> logError, Action<string, string, string> logDebug)
        {
            Action<Exception, string, string> errorNoop = (e, key, data) => { };
            Action<string, string, string> infoNoop = (info, key, data) => { };

            _logError = logError ?? errorNoop;
            _logDebug = logDebug ?? infoNoop;
        }

        /// <summary>
        /// Publishes the message to the specified channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        public void PublishObject(MessageTypes channel, NetworkMessageContainer msg)
        {
            _sub.Publish(channel.ToString(), msg.Serialize());            
        }

        /// <summary>
        /// Publishes the message, using channelType and channelId to route the message appropriately
        /// </summary>
        /// <param name="channelType"></param>
        /// <param name="channelId"></param>
        /// <param name="msg"></param>
        public void PublishObject(ChannelTypes channelType, int channelId, NetworkMessageContainer msg)
        {
            _sub.Publish(channelType.ToString() + channelId.ToString(), msg.Serialize());
        }

        /// <summary>
        /// Publishes the message, using channelType and channelId to route the message appropriately
        /// </summary>
        /// <param name="channelType"></param>
        /// <param name="channelId"></param>
        /// <param name="msg"></param>
        public Task PublishObjectAsync(ChannelTypes channelType, int channelId, NetworkMessageContainer msg)
        {
            return _sub.PublishAsync(channelType.ToString() + channelId.ToString(), msg.Serialize());
        }
        /// <summary>
        /// Synchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">RedisDBKey value to convert to string and lookup.</param>
        /// <param name="obj">MsgPack object to serialize.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public bool SetValue(
            RedisDBKeyTypes keyType,
            MessagePackSerializableObject obj,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            return SetValue(keyType, "", obj, expiry, when, commandFlags);
        }

        /// <summary>
        /// Asynchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">RedisDBKey value to convert to string and lookup.</param>
        /// <param name="obj">MsgPack object to serialize.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public async Task<bool> SetValueAsync(
            RedisDBKeyTypes keyType,
            MessagePackSerializableObject obj,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            return await SetValueAsync(keyType, "", obj, expiry, when, commandFlags);
        }

        /// <summary>
        /// Synchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">Redis keyType to set.</param>
        /// <param name="keyIdentifier">suffix added to keyType for unique keys. If unused, set to ""</param>
        /// <param name="obj">MsgPack object to serialize.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public bool SetValue(
            RedisDBKeyTypes keyType,
            string keyIdentifier,
            MessagePackSerializableObject obj,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            string keyString = keyType + "_" + keyIdentifier;
#if DEBUG
            LogDebug("[Redis Sync Set]: {0}" + expiry, keyString, obj.Serialize().ToString());
#endif

            return _db.StringSet(keyString, obj.Serialize(), expiry, (When)when, (CommandFlags)commandFlags);
        }

        /// <summary>
        /// Asynchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">Redis keyType to set.</param>
        /// <param name="keyIdentifier">suffix added to keyType for unique keys. If unused, set to ""</param>
        /// <param name="obj">MsgPack object to serialize.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public Task<bool> SetValueAsync(
            RedisDBKeyTypes keyType,
            string keyIdentifier,
            MessagePackSerializableObject obj,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            string keyString = keyType + "_" + keyIdentifier;
#if DEBUG
            LogDebug("[Redis Async Set]: {0}" + expiry, keyString, obj.Serialize().ToString());
#endif

            return _db.StringSetAsync(keyString, obj.Serialize(), expiry, (When)when, (CommandFlags)commandFlags);
        }

        /// <summary>
        /// Synchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">Redis keyType to set.</param>
        /// <param name="keyIdentifier">suffix added to keyType for unique keys. If unused, set to ""</param>
        /// <param name="data">String to insert for data.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public bool SetRawValue(
            RedisDBKeyTypes keyType,
            string keyIdentifier,
            string data,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {

            string keyString = keyType + "_" + keyIdentifier;
#if DEBUG
            LogDebug("[Redis Sync Set Raw]: {0}" + expiry, keyString, data.ToString());
#endif

            return _db.StringSet(keyString, data, expiry, (When)when, (CommandFlags)commandFlags);
        }

        /// <summary>
        /// Asynchronously set keyType to hold the string value. If keyType already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="keyType">Redis keyType to set.</param>
        /// <param name="keyIdentifier">suffix added to keyType for unique keys. If unused, set to ""</param>
        /// <param name="data">String to insert for data.</param>
        /// <param name="expiry">(optional) TimeSpan date to expire value at.</param>
        /// <param name="when">Allows you to specify criteria for setting object. Update only if not-exist, etc.</param>
        /// <param name="commandFlags">Flags to pass to Redis.</param>
        /// <returns>True or false depending on if value was successfully set.</returns>
        public Task<bool> SetRawValueAsync(
            RedisDBKeyTypes keyType,
            string keyIdentifier,
            string data,
            TimeSpan? expiry = null,
            SetWhen when = SetWhen.Always,
            RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            string keyString = keyType + "_" + keyIdentifier;
#if DEBUG
            LogDebug("[Redis Async Set Raw]: {0}" + expiry, keyString, data.ToString());
#endif
            return _db.StringSetAsync(keyString, data, expiry, (When)when, (CommandFlags)commandFlags);
        }

        /// <summary>
        /// Fetch value synchronously from Redis and deserialize as MsgPack object <typeparam name="type">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">A message pack serializable object.</typeparam>
        /// <param name="keyType">Key type to lookup.</param>
        /// <param name="keyIdentifier">Key identifier too lookup. If unused, set to ""</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>Instance of MsgPack object of type <typeparam name="type">T</typeparam>.</returns>
        public T GetValue<T>(RedisDBKeyTypes keyType, string keyIdentifier, RedisCommandFlags flags = RedisCommandFlags.None)
            where T : MessagePackSerializableObject, new()
        {
            var keyString = BuildKeyString(keyType, keyIdentifier);
            var data = _db.StringGet(keyString, (CommandFlags)flags);

            return !data.HasValue ? null : DeserializeMsgPack<T>(keyString, data);
        }

        /// <summary>
        /// Fetch value asynchronously from Redis and deserialize as MsgPack object <typeparam name="type">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">A message pack serializable object.</typeparam>
        /// <param name="keyType">Key type to lookup.</param>
        /// <param name="keyIdentifier">Key identifier too lookup. If unused, set to ""</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>Instance of MsgPack object of type <typeparam name="type">T</typeparam>.</returns>
        public async Task<T> GetValueAsync<T>(RedisDBKeyTypes keyType, string keyIdentifier, RedisCommandFlags flags = RedisCommandFlags.None)
            where T : MessagePackSerializableObject, new()
        {
            var keyString = BuildKeyString(keyType, keyIdentifier);
            var data = await _db.StringGetAsync(keyString, (CommandFlags)flags);

            return !data.HasValue ? null : DeserializeMsgPack<T>(keyString, data);
        }

        /// <summary>
        /// Fetch value synchronously from Redis and deserialize as MsgPack object <typeparam name="type">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">A message pack serializable object.</typeparam>
        /// <param name="rawKeyType">RedisDBKey value to convert to string and lookup.</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>Instance of MsgPack object of type <typeparam name="type">T</typeparam>.</returns>
        public T GetValue<T>(RedisDBKeyTypes rawKeyType, RedisCommandFlags flags = RedisCommandFlags.None)
            where T : MessagePackSerializableObject, new()
        {
            return GetValue<T>(rawKeyType, "", flags);
        }

        /// <summary>
        /// Fetch value asynchronously from Redis and deserialize as MsgPack object <typeparam name="type">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">A message pack serializable object.</typeparam>
        /// <param name="rawKeyType">RedisDBKey value to convert to string and lookup.</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>Instance of MsgPack object of type <typeparam name="type">T</typeparam>.</returns>
        public async Task<T> GetValueAsync<T>(RedisDBKeyTypes rawKeyType, RedisCommandFlags flags = RedisCommandFlags.None)
            where T : MessagePackSerializableObject, new()
        {
            return await GetValueAsync<T>(rawKeyType, "", flags);
        }

        /// <summary>
        /// Fetch value synchronously from Redis and return raw string value.
        /// </summary>
        /// <param name="keyType">Key type to lookup.</param>
        /// <param name="keyIdentifier">Key identifier too lookup. If unused, set to ""</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>String value stored in Redis.</returns>
        public string GetRawValue(RedisDBKeyTypes keyType, string keyIdentifier, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var keyString = BuildKeyString(keyType, keyIdentifier);
            var data = _db.StringGet(keyString, (CommandFlags)flags);

            LogDebug("Getting raw Redis data", keyString, data.ToString());

            return data;
        }

        /// <summary>
        /// Fetch value asynchronously from Redis and return raw string value.
        /// </summary>
        /// <param name="keyType">Key type to lookup.</param>
        /// <param name="keyIdentifier">Key identifier too lookup. If unused, set to ""</param>
        /// <param name="flags">Flags to pass to Redis.</param>
        /// <returns>String value stored in Redis.</returns>
        public async Task<string> GetRawValueAsync(RedisDBKeyTypes keyType, string keyIdentifier, RedisCommandFlags commandFlags = RedisCommandFlags.None)
        {
            var keyString = BuildKeyString(keyType, keyIdentifier);
            var data = await _db.StringGetAsync(keyString, (CommandFlags)commandFlags);

            LogDebug("Getting raw Redis data", keyString, data.ToString());

            return data;
        }

        /// <summary>
        /// HashSets in redis are keyType-value dictionaries
        /// </summary>
        /// <param name="hashSetIdentifier"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Not sure yet TODO:Test undocumented return. Probably returns true if set is successful</returns>
        public async Task<bool> SetHashValue(RedisDBKeyTypes hashSetIdentifier, int key, int? value)
        {
            return await _db.HashSetAsync(hashSetIdentifier.ToString(), key, value);
        }

        /// <summary>
        /// HashSets in redis are keyType-value dictionaries
        /// </summary>
        /// <param name="hashSetIdentifier"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Not sure yet TODO:Test undocumented return. Probably returns true if set is successful</returns>
        public async Task<RedisValue> GetHashValue(RedisDBKeyTypes hashSetIdentifier, int key)
        {            
            return await _db.HashGetAsync(hashSetIdentifier.ToString(), key);
        }

        /// <summary>
        /// HashSets in redis are keyType-value dictionaries
        /// </summary>
        /// <param name="hashSetIdentifier"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Not sure yet TODO:Test undocumented return. Probably returns true if set is successful</returns>
        public async Task<bool> SetHashValue(RedisDBKeyTypes hashSetIdentifier, int key, string value)
        {
            return await _db.HashSetAsync(hashSetIdentifier.ToString(), key, value);
        }

        public async Task<bool> CheckHashValue(RedisDBKeyTypes hashSetIdentifier, int key)
        {
            return await _db.HashExistsAsync(hashSetIdentifier.ToString(), key);
        }

        public async Task ClearHashValue(RedisDBKeyTypes hashSetIdentifier, int key)
        {
            await _db.HashDeleteAsync(hashSetIdentifier.ToString(), key);
        }

        /// <summary>
        /// Registers a callback to the specified channel. callback argument is guaranteed to be a byte[] encoded MessagePackSerializableObject object
        /// Use SerializationUtilities to deserialize to the correct message, which should be paired to the channel
        /// </summary>
        public void Subscribe(MessageTypes channel, EventHandler<NetworkMessageContainer> callback)
        {
            _sub.Subscribe(channel.ToString(), (rc, rv) => {
                try
                {
                    callback(this, SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(rv));
                
                }
                catch(Exception e)
                {
                    ConsoleManager.WriteLine("Redis exception! " + e.Message, ConsoleMessageType.Error);
                    ConsoleManager.WriteLine(e.StackTrace, ConsoleMessageType.Error);
                }
                
            });            
        }

        /// <summary>
        /// Registers a callback to the specified channelType with the given channelID
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        public void Subscribe(ChannelTypes channelType, int channelID, EventHandler<NetworkMessageContainer> callback)
        {
            _sub.Subscribe(channelType.ToString() + channelID.ToString(), (rc, rv) =>
            {
                try
                {
                    callback(this, SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(rv));
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLine("Redis exception! " + e.Message, ConsoleMessageType.Error);
                    ConsoleManager.WriteLine(e.StackTrace, ConsoleMessageType.Error);
                }

            });
        }

        public void UnSubscribe(MessageTypes channel, EventHandler<NetworkMessageContainer> callback)
        {
            _sub.Unsubscribe(channel.ToString(), (rc, rv) => { callback(this, SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(rv)); });
        }

        public void UnSubscribe(ChannelTypes channelType, int channelID, EventHandler<NetworkMessageContainer> callback)
        {
            _sub.Unsubscribe(channelType.ToString() + channelID.ToString(), (rc, rv) => { callback(this, SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(rv)); });
        }

        /// <summary>
        /// Generates an id which is unique from server startup. Uniqueness does not persist on server shutdown
        /// Uses transaction to allow safe concurrency
        /// </summary>
        /// <returns></returns>
        public async Task<int> GenerateUniqueId()
        {
            bool success = false;
            var idValue = 0;
            
            while (!success)
            {
                idValue = _random.Next(-int.MaxValue, int.MaxValue);

                var tr = _db.CreateTransaction();
                tr.AddCondition(Condition.HashNotEqual("UniqueIDHashSet", idValue, idValue));
                tr.HashSetAsync("UniqueIDHashSet", idValue, idValue);
 
                success = await tr.ExecuteAsync();
            }

            return idValue;
        }

        protected void LogDebug(string message, string key, string data)
        {
            _logDebug(message, key, data);
        }
        
        protected void LogError(Exception exception, string key, string data)
        {
            _logError(exception, key, data);
        }

        protected string BuildKeyString(RedisDBKeyTypes keyType, string keyIdentifier)
        {
            return keyType + "_" + keyIdentifier;
        }
    }

    public enum ChannelTypes
    {
        MasterSlave,

        /// <summary>
        /// For slave-simulator game data transmission, where redis channels use areaIDs as descriminators
        /// </summary>
        SimulatorToServer_Data,


        /// <summary>
        /// For slave-simulator game data transmission, where redis channels use areaIDs as descriminators
        /// </summary>
        ServerToSimulator_Data,
        


        /// <summary>
        /// For slave-simulator network/administrative messages (e.g. pings, connection requests)
        /// </summary>
        SimulatorToServer_Network,

        /// <summary>
        /// For slave-simulator network/administrative messages (e.g. pings, connection requests)
        /// </summary>
        ServerToSimulator_Network,

        WebToSlave,

        WebLogin,
    
    }

    public enum RedisDBKeyTypes
    {
        /// <summary>
        /// If set, master server is online. If unset, master server is offline, and EVERYBODY PANIC AAHHH
        /// </summary>
        MasterServerTimestamp,

        /// <summary>
        /// Hash set containing currently in use slave IDs, to prevent collisions
        /// If you're thinking that it's pretty anal to actively prevent collisions between integers generated from rand.next(-int.maxvalue, int.maxvalue), you're probably right. 
        /// But fuck your shit I do what I want
        /// </summary>
        SlaveIDHashSet,
        
        /// <summary>
        /// Expiring keyType which allows the master server to detect slave shutdown/crashes
        /// </summary>
        SlaveHeartbeat,

        FreeconCsrfKey,

        PlayerSessionKey,

        /// <summary>
        /// State authority to ensure correct area for Nancy during high DB latency
        /// </summary>
        PlayerIDToCurrentAreaID,

        TokenToIDKey,
    }

}
