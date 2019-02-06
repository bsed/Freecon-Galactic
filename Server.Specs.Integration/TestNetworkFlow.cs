using Autofac;
using FakeItEasy;
using Freecon.Core.Networking;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Server.App;
using Freecon.Server.Configs;
using Freecon.Server.Core;
using Freecon.Server.Core.Reactive;
using Machine.Specifications;
using System;
using System.Threading;
using TestClient.Lidgren;

namespace Freecon.Server.Specs.Integration
{
    public class check_network_flow
    {
        public static IClientCommRouter router;

        public static LidgrenTestClient testClient;

        public static FreeconServerApp server;

        public static ClientCommMessage<PositionUpdate> receivedMessage;

        Establish context = () =>
        {
            router = A.Fake<IClientCommRouter>();

            var container = IntegrationTestUtils.SetupEnvironment();

            router = container.Resolve<IClientCommRouter>();

            server = new FreeconServerApp(container);

            Thread.Sleep(100);

            testClient = new LidgrenTestClient(
                container.Resolve<IMessageSerializer>(),
                new TestNetworkConfig()
            );

            router.RegisterSubscriber<PositionUpdate>().Subscribe(positionUpdate => receivedMessage = positionUpdate);
        };

        Because of = () =>
        {
            testClient.SendMessage(new PositionUpdate(12, 14));

            // Todo: Write async helper to iterate faster on this.
            Thread.Sleep(500);
        };

        It should_have_received_message = () => receivedMessage.ShouldNotBeNull();
        It should_have_x_position = () => receivedMessage.Message.X.ShouldBeCloseTo(12);
        It should_have_y_position = () => receivedMessage.Message.Y.ShouldBeCloseTo(14);
    }
}
