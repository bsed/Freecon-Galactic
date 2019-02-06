using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Core.Web.Post;
using Freecon.Core.Utils;
using Freecon.Server.Configs;
using Freecon.Server.Core.Services;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.Extensions;
using Nancy.Security;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using RedisWrapper;
using Server.Database;
using Server.Managers;

namespace Core.Web
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private RedisServer redisServer;
        private SessionService sessionService;

        private List<string> corsDomains = new List<string>()
        {
            "(http|https)://localhost:3080/?",
            "(http|https)://freecon-dev.spacerambles.com:3080/?"
        };

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {

            container.Register<IDatabaseManager, DBStateResolver>().AsSingleton();

#if !DEBUG
            DiagnosticsHook.Disable(pipelines);
#else
            StaticConfiguration.EnableRequestTracing = true;
#endif

            redisServer = new RedisServer(LogError, LogDebug, (new RedisConfig()).Address);
            container.Register<RedisServer>(redisServer);

            var serviceId = redisServer.GenerateUniqueId().Result;
            container.Register<IServerConfigService, WebServerConfigService>(new WebServerConfigService(redisServer, serviceId));

            sessionService = container.Resolve<SessionService>();

            // CSRF that uses Redis for shared token generation. Tokens currently don't expire.
            Csrf.Enable(pipelines, new CryptographyConfiguration(
                new RijndaelEncryptionProvider(new RedisBasedKeyGenerator(redisServer)),
                new DefaultHmacProvider(new RedisBasedKeyGenerator(redisServer)))
            );

            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx =>
            {
                var origin = ctx.Request.Headers["Origin"].FirstOrDefault();

                if (origin == null)
                {
                    return null;
                }

                var matches = corsDomains.FirstOrDefault(
                    allowed => Regex.IsMatch(origin, "^" + allowed + "$", RegexOptions.IgnoreCase));

                // No matches, so let's abort.
                if (matches == null)
                {
                    var responseJson = (Response)"CORS not allowed.";

                    responseJson.ContentType = "application/json";
                    responseJson.StatusCode = HttpStatusCode.BadRequest;

                    return responseJson;
                }
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                var origin = ctx.Request.Headers["Origin"].FirstOrDefault();

                if (origin == null)
                {
                    return;
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
                ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Accept,Origin,Content-type");
                ctx.Response.Headers.Add("Access-Control-Expose-Headers", "Accept,Origin,Content-type");
            });

            pipelines.BeforeRequest.AddItemToEndOfPipeline(ProcessSessionAuth);

            pipelines.OnError += (ctx, ex) => {
                throw ex;
            };
        }

        private async Task<Response> ProcessSessionAuth(NancyContext nancyContext, CancellationToken cancellationToken)
        {
            string token;

            if (nancyContext.Request.Method.ToLower() != "get")
            {
                var body = nancyContext.Request.Body.FetchRequestBody<FreeconPostBody>();

                if (body?.Token == null)
                {
                    return null;
                }

                token = body.Token;
            }
            else
            {
                var tokenQuery = nancyContext.Request.Query[WebConfig.CookieName];

                // No session cookie, skip.
                if (!tokenQuery.HasValue)
                {
                    return null;
                }

                token = tokenQuery.Value;
            }

            var user = await sessionService.FindPlayerSessionByToken(token);

            if (user != null)
            {
                nancyContext.CurrentUser = new WebUserIdentity(user);
            }
            
            return null;
        }
        
        public void LogDebug(string message, string key, string data)
        {
            Console.WriteLine("{0}, {1}, {2}", message, key, data);
        }

        public void LogError(Exception exception, string key, string data)
        {
            Console.WriteLine("{0}, {1}, {2}", exception.ToString(), key, data);
        }

#if DEBUG
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"thisisaverysecurepasswordlol" }; }
        }
#endif
    }
}
