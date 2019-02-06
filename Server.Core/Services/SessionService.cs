using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Cryptography;
using Freecon.Core.Networking;
using Freecon.Server.Core.Interfaces;
using RedisWrapper;
using Server.Models;

namespace Freecon.Server.Core.Services
{
    public class SessionService : IServerService
    {
        private readonly CryptographyManager _cryptographyManager;
        private readonly RedisServer _redis;
        private readonly IServerConfigService _serviceConfigService;
        private static Random _random = new Random();

        public SessionService(
            CryptographyManager cryptographyManager,
            RedisServer redis,
            IServerConfigService serviceConfigService)
        {
            _cryptographyManager = cryptographyManager;
            _redis = redis;
            _serviceConfigService = serviceConfigService;
        }

        public async Task<PlayerSession> CreatePlayerSession(AccountModel account, IPAddress ip)
        {
            var apiToken = GetTokenString();
            var slaveId = _serviceConfigService.CurrentServiceId;
            var roles = new List<string>() {UserRoles.User.ToString()};

            if (account.IsAdmin)
            {
                roles.Add(UserRoles.Admin.ToString());
            }

            var session = new PlayerSession(account.PlayerID, account.Username, apiToken, slaveId, DateTime.Now, ip, roles);

            var setSession = _redis.SetValueAsync(RedisDBKeyTypes.PlayerSessionKey, account.PlayerID.ToString(), session);

            var setSessionToId = _redis.SetRawValueAsync(RedisDBKeyTypes.TokenToIDKey,
                session.ApiToken,
                session.PlayerId.ToString()
            );

            if (!(await setSession) || !(await setSessionToId))
            {
                throw new RedisSetDataException("Could not create Redis Player Session");
            }

            return session;
        }

        public async Task<PlayerSession> FindPlayerSession(int playerId)
        {
            return await _redis.GetValueAsync<PlayerSession>(RedisDBKeyTypes.PlayerSessionKey, playerId.ToString());
        }

        public async Task<PlayerSession> FindPlayerSessionByToken(string token)
        {
            int playerId;

            if (Int32.TryParse(await _redis.GetRawValueAsync(RedisDBKeyTypes.TokenToIDKey, token), out playerId))
            {
                return await FindPlayerSession(playerId);
            }

            return null;
        }
        

        private string GetTokenString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // Randomly select some characters
            var token = Enumerable.Repeat(chars, 64)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray();

            return new string(token);
        }
    }
}
