using Nancy;
using Newtonsoft.Json;

namespace Core.Web
{
    public class NancyMain:NancyModule
    {
        public NancyMain()
        {
            var mockResponses = new MockResponses();

            Get["/colony-data"] = parameters => JsonConvert.SerializeObject(mockResponses.GetFakeColonyData());
        }
    }
}
