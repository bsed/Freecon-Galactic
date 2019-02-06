using Nancy;
using Newtonsoft.Json;
using Server.Database;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Web
{
    public class TestHandler:NancyModule
    {
         IDatabaseManager _databaseManager;

         public TestHandler(IDatabaseManager databaseManager)
             : base("/test")
        {
            _databaseManager = databaseManager;
            
            //Method:GET
            //Pattern:land-data
            //Action: _getLandData           
            Get["/hello-world", runAsync:true] = _helloWorld;
        }


        async Task<dynamic> _helloWorld(dynamic parameters, CancellationToken cancellationToken)
        {
            Console.WriteLine("Handling request...");

            return JsonConvert.SerializeObject(new {hi = "wow"});
        }


    }
}
