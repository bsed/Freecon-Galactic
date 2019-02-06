using Freecon.Server.Configs;

namespace Freecon.Server.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrapper = new ServerBootstrapper();

            var container = bootstrapper.Setup(FreeconServerEnvironment.Development);

            var app = new FreeconServerApp(container);
        }
    }
}
