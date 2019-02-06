namespace Freecon.Server.Specs
{
    //public class when_pushing_a_raw_message : base_server_node_specs
    //{
    //    protected static ServerNodeTest<PositionUpdateProto, NewPositionUpdate> test;

    //    protected static NewPositionUpdate Observed;

    //    protected static PositionUpdateProto Proto = new PositionUpdateProto() { X = 11, Y = 12 };

    //    Establish context = () => TestSetup<PositionUpdateProto, NewPositionUpdate>(out test, 
    //                                                                           p => Observed = p, 
    //                                                                           Proto, 
    //                                                                           TodoMessageTypes.PositionUpdateData
    //                                                                        );

    //    Because of = () => test.Stream.OnNext(test.Raw);

    //    Behaves_like<ServerNodePublishBehaviors<PositionUpdateProto, NewPositionUpdate>> server_node_expected_behavior;
    //}

    //public class base_server_node_specs : base_specs
    //{
    //    public static void TestSetup<P, T>(out ServerNodeTest<P, T> test,
    //                                       Action<T> callback,
    //                                       P proto,
    //                                       TodoMessageTypes type
    //                                      ) where T : ICommMessage
    //    {
    //        test = new ServerNodeTest<P, T>(proto, type);

    //        test.Server.Start();

    //        test.ObservableStream.Subscribe<T>(callback);
    //    }
    //}

    //// Fields must be protected in both caller and behavior.
    //[Behaviors]
    //public class ServerNodePublishBehaviors<P, T> where T : ICommMessage
    //{
    //    protected static ServerNodeTest<P, T> test;

    //    protected static NewPositionUpdate Observed;

    //    It should_have_deserialized_container = () => A.CallTo(() => test.MockSerializer.DeserializeMessageContainer(null))
    //        .WithAnyArguments().MustHaveHappened();

    //    It should_have_deserialized_payload = () => A.CallTo(() => test.MockSerializer.DeserializePayload(null))
    //        .WithAnyArguments().MustHaveHappened();

    //    It should_have_registered_with_router = () => A.CallTo(() => test.MockRouter.RegisterPublisher<T>(test.ObservableStream))
    //        .MustHaveHappened();

    //    It should_have_observed_value = () => Observed.ShouldNotBeNull();
    //}

    ///// <summary>
    ///// This can be simplified a lot... We can perform some refactors on this later.
    ///// </summary>
    ///// <typeparam name="P"></typeparam>
    ///// <typeparam name="T"></typeparam>
    //public class ServerNodeTest<P, T> where T : ICommMessage
    //{
    //    public Subject<RawClientRequest> Stream;

    //    public AutoFake Mock;

    //    public IServerTransportBroker MockBroker;

    //    public IMessageSerializer MockSerializer;

    //    public ProtobufMessageSerializer RealSerializer;

    //    public IReactiveRouter MockRouter;

    //    public IClientNode FakeClient;

    //    public IServerNode Server;

    //    public P InputMessage;

    //    public RawClientRequest Raw;

    //    public IObservable<T> ObservableStream;

    //    public ServerNodeTest(P input, TodoMessageTypes messageType)
    //    {
    //        Stream = new Subject<RawClientRequest>();

    //        InputMessage = input;

    //        Mock = new AutoFake();

    //        Server = Mock.Resolve<ServerNode>();

    //        MockSerializer = Mock.Resolve<IMessageSerializer>();

    //        MockBroker = Mock.Resolve<IServerTransportBroker>();

    //        MockRouter = Mock.Resolve<IReactiveRouter>();

    //        FakeClient = A.Dummy<IClientNode>();

    //        RealSerializer = new ProtobufMessageSerializer();

    //        var transportNode = A.Fake<IServerTransportNode>();

    //        A.CallTo(() => transportNode.ReceivedMessage).Returns(Stream);

    //        A.CallTo(() => MockBroker.Start(null)).WithAnyArguments().Returns(transportNode);

    //        Setup(messageType);
    //    }

    //    public void Setup(TodoMessageTypes messageType)
    //    {
    //        Raw = base_specs.BuildRawRequest(InputMessage, messageType, FakeClient);

    //        var messageContainer = RealSerializer.DeserializeMessageContainer(Raw.RawRequest);

    //        A.CallTo(() => MockSerializer.DeserializeMessageContainer(Raw.RawRequest))
    //            .ReturnsLazily<RawMessageContainer>(() => messageContainer);

    //        A.CallTo(() => MockSerializer.DeserializePayload(null))
    //            .WithAnyArguments().ReturnsLazily(() => RealSerializer.DeserializePayload(messageContainer));

    //        // Get the stream that we invoked with.
    //        // We should really move off of passing a stream in, and instead rely on it making one for us.
    //        // Far easier to test!
    //        A.CallTo(() => MockRouter.RegisterPublisher<T>(null))
    //            .WithAnyArguments().Invokes(callObject => 
    //            {
    //                ObservableStream = (IObservable<T>)callObject.Arguments[0];
    //            });
    //    }
    //}
}
