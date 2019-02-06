using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Web.Post;
using Freecon.Core.Networking;
using Freecon.Core.Utils;
using Freecon.Server.Core;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;

namespace Core.Web.Modules
{
    public abstract class BaseHandler : NancyModule
    {
        private WebUserIdentity WebUser
        {
            get
            {
                return Context.CurrentUser as WebUserIdentity;
            }
        }

        public BaseHandler(string mount) : base(mount) { }

        public BaseHandler() { }

        public void GetWithAdminAuth(string route, Func<dynamic, PlayerSession, CancellationToken, Task<dynamic>> routeHandler)
        {
            WithRouteAuth(Get, route, UserRoles.Admin, routeHandler);
        }
        public void PostWithAdminAuth<T>(string route, Func<dynamic, PlayerSession, T, CancellationToken, Task<dynamic>> routeHandler)
            where T : FreeconPostBody
        {
            PostWithRouteAuth<T>(Post, route, UserRoles.Admin, routeHandler);
        }

        public void GetWithPlayerAuth(string route, Func<dynamic, PlayerSession, CancellationToken, Task<dynamic>> routeHandler)
        {
            WithRouteAuth(Get, route, UserRoles.User, routeHandler);
        }

        public void PostWithPlayerAuth<T>(string route, Func<dynamic, PlayerSession, T, CancellationToken, Task<dynamic>> routeHandler)
            where T : FreeconPostBody
        {
            PostWithRouteAuth<T>(Post, route, UserRoles.User, routeHandler);
        }

        private void PostWithRouteAuth<T>(RouteBuilder builder,
            string route,
            UserRoles requiredRole,
            Func<dynamic, PlayerSession, T, CancellationToken, Task<dynamic>> routeHandler
        )
            where T : FreeconPostBody
        {
            // Todo: Verify CSRF stuff.

            builder[route, runAsync: true] = async (requestParams, token) =>
            {
                var body = this.Request.Body.FetchRequestBody<T>();

                if (body == null)
                {
                    return MessageWithStatus(HttpStatusCode.BadRequest, "Missing body with request");
                }
                
                var validationResponse = ProcessRequest(requiredRole);

                if (validationResponse != null)
                {
                    return validationResponse;
                }

                return await routeHandler(requestParams, WebUser.Session, body, token);
            };
        }

        private void WithRouteAuth(
            RouteBuilder builder,
            string route,
            UserRoles requiredRole,
            Func<dynamic, PlayerSession, CancellationToken, Task<dynamic>> routeHandler)
        {
            builder[route, runAsync: true] = async (requestParams, token) =>
            {
                var validationResponse = ProcessRequest(requiredRole);

                if (validationResponse != null)
                {
                    return validationResponse;
                } 

                return await routeHandler(requestParams, WebUser.Session, token);
            };
        }

        private Response ProcessRequest(UserRoles requiredRole)
        {
            if (!Context.CurrentUser.HasClaim(requiredRole.ToString()))
            {
                return Unauthorized();
            }
            
            if (WebUser == null)
            {
                return InternalServiceError();
            }

            return null;
        }

        public Response MessageWithStatus(HttpStatusCode statusCode, string message, object meta = null)
        {
            return ReturnJsonResponse(new FreeconJsonResponse(message, meta), statusCode);
        }

        public Response ReturnJsonResponse(object message, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            // Extra new line for curl output
            var responseJson = (Response)(LowercaseContractResolver.SerializeObject(message) + "\n");

            responseJson.ContentType = "application/json";
            responseJson.StatusCode = statusCode;

            return responseJson;
        }

        public Response InternalServiceError(
            string message = "Internal Service Error",
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError
        )
        {
            return ReturnJsonResponse(new
            {
                hasError = true,
                message
            }, statusCode);
        }

        public Response Unauthorized(
            string message = "Unauthorized",
            HttpStatusCode statusCode = HttpStatusCode.Unauthorized
        )
        {
            return ReturnJsonResponse(new
            {
                hasError = true,
                message
            }, statusCode);
        }
    }
}
