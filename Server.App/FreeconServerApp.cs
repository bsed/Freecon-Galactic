using Autofac;

namespace Freecon.Server.App
{
    public class FreeconServerApp
    {
        public FreeconServerApp(IContainer container)
        {
            var networkManager = container.Resolve<NetworkManager>();
            var clientRequestRouter = container.Resolve<ClientRequestRouter>();

            // Todo: Make stream operations run in an async queue, instead of take up Lidgren thread time.
            // Also ensure thread syncronization context is correct, though it should be fine since operations
            // are run sequentially. Not exactly sure how async works with this though..?

            var dispatchManager = container.Resolve<DispatchManager>();

            dispatchManager.Start();

            // Todo: Setup main thread here that pushes to a queue and then reads off of it.
        }
    }
}
