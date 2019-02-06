using Nancy;
using Nancy.ErrorHandling;
using System.IO;

namespace Core.Web.StatusCodes
{
    public class StatusCodeHandler : IStatusCodeHandler
    {
        private readonly IRootPathProvider _rootPathProvider;

        public StatusCodeHandler(IRootPathProvider rootPathProvider)
        {
            _rootPathProvider = rootPathProvider;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return false;

        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            //Default handler returns a stack trace, we don't want to expose implementation details to clients
            context.Response.Contents = stream =>
                {
                     StreamWriter s = new StreamWriter(stream);
                    s.Write("Error " + statusCode);
                    s.Flush();

                };


        //    context.Response.Contents = stream =>
        //    {
        //        var filename = Path.Combine(_rootPathProvider.GetRootPath(), "content/PageNotFound.html");
        //        using (var file = File.OpenRead(filename))
        //        {
        //            file.CopyTo(stream);
        //        }
        //    };
        }
    }
}
