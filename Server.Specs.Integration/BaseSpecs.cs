using Autofac;
using Freecon.Core.Networking.Proto;
using Freecon.Server.App;
using Freecon.Server.Configs;
using System;

namespace Freecon.Server.Specs.Integration
{
    /// <summary>
    /// A suite of helper utilities for running integration tests.
    /// </summary>
    public class IntegrationTestUtils
    {
        /// <summary>
        /// Sets up the entire Freecon environment for testing.
        /// </summary>
        /// <param name="containerCallback">
        /// Optionally takes in a callback that is passed a ContainerBuilder object from AutoFac that
        /// allows the user to specificy specific dependencies to be resolved at container build time.
        /// </param>
        /// <returns>
        /// Autofac IContainer that is the configured Freecon environment.
        /// </returns>
        public static IContainer SetupEnvironment(Action<ContainerBuilder> containerCallback = null)
        {
            // Automapper requires an explicit map between types.
            ProtobufMappingSetup.Setup();

            return SetupContainer(containerCallback);
        }

        public static IContainer SetupContainer(Action<ContainerBuilder> containerCallback)
        {
            var serverBootstrapper = new ServerBootstrapper();

            return serverBootstrapper.Setup(FreeconServerEnvironment.Test, containerCallback);
        }
    }
}
