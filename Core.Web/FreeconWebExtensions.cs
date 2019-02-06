using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Web.Post;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Newtonsoft.Json;

namespace Core.Web
{
    public static class FreeconWebExtensions
    {
        public static T FetchRequestBody<T>(this RequestStream body)
            where T : FreeconPostBody
        {
            var bodyString = body.AsString();
            body.Position = 0;
            return JsonConvert.DeserializeObject<T>(bodyString);
        }
    }
}
