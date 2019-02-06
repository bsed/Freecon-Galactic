using Autofac;
using Freecon.Core.Networking;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Proto;
using Freecon.Server.Configs;
using Freecon.Server.Core;
using Reactive.Router;
using System;
using System.Collections.Generic;
using System.Reflection;
using Freecon.Core.Utils;

namespace Freecon.Server.App
{
    public class ServerBootstrapper
    {
        /// <summary>
        /// This is the container from the Dependency Injection framework.
        /// You can use this to manually resolve things. Ideally, you won't do that though.
        /// You're not that naughty though, right?
        /// </summary>
        public IContainer Container { get; private set; }

        public IContainer Setup(FreeconServerEnvironment environment, Action<ContainerBuilder> configCallback = null)
        {
            var builder = new ContainerBuilder();

            // This works sometimes. Other times, you have to do something like:
            // var neverUsed = typeof(someTypeInAnAssembly);
            // In order for the CLR to load your assembly up.
            var assemblies = new List<Assembly>()
            {
                typeof(ClientCommRouter).Assembly,
                typeof(IClientCommRouter).Assembly
            };

            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

            var assemblyArray = assemblies.ToArray();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(p => p.Name.EndsWith("Manager") || p.Name.EndsWith("Service") || p.Name.EndsWith("Router"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(p => p.Name.EndsWith("Manager") || p.Name.EndsWith("Service") || p.Name.EndsWith("Router"))
                .AsImplementedInterfaces()
                .SingleInstance();

            var messageSerializer = new ProtobufMessageSerializer();
            var conf = new DevelopNetworkConfig();
            var logger = new VerboseLoggerUtil();

            // #Yolo exception logging
            var router = new ReactiveRouter((ex) => logger.Log("Exception: " + ex.ToString(), LogLevel.Error));

            // Todo: Command line arguments or something to specify environment.
            var configManager = new ConfigManager(environment);

            var networkManager = new NetworkManager(messageSerializer, configManager, logger);

            // Register dependencies that need configuration.
            builder.RegisterInstance<ILoggerUtil>(logger).SingleInstance();
            builder.RegisterInstance<IMessageSerializer>(messageSerializer);
            builder.RegisterInstance<ConfigManager>(configManager).SingleInstance();
            builder.RegisterInstance<NetworkManager>(networkManager).SingleInstance();
            builder.RegisterInstance<IReactiveRouter>(router).SingleInstance();

            if (configCallback != null)
            {
                configCallback(builder);
            }

            Container = builder.Build();

            RegisterMappings(Container.Resolve<IClientRequestRouter>());
            
            return Container;
        }

        /// <summary>
        /// Registers mappings for the Server instance.
        /// Note: You have to register new message types with MessageSerializer.Proto as well.
        /// </summary>
        /// <param name="clientRequestRouter">Client Request Router instance to register agianst.</param>
        public void RegisterMappings(IClientRequestRouter clientRequestRouter)
        {
            ProtobufMappingSetup.Setup();

            // Set up the route mappings. Take the enum and associate it with the type.
            clientRequestRouter.RegisterType<PositionUpdate>(TodoMessageTypes.PositionUpdateData);
        }
    }
}
