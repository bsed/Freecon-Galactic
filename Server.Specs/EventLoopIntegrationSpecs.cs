namespace Freecon.Server.Specs
{
    //public class when_adding_simple_async_task : base_event_loop_specs
    //{
    //    static AsyncWork _cheapWork;
    //    static CancellationTokenSource _workToken;
    //    static IWorkerResult _returnedResult;

    //    Establish context = () =>
    //    {
    //        var cw = CreateCheapWork(r =>
    //        {
    //            _returnedResult = r;
    //        });

    //        _cheapWork = cw.Item2;
    //        _workToken = cw.Item1;
    //    };

    //    Because of = () =>
    //    {
    //        _eventLoop.AddAsyncWork(_cheapWork);

    //        // Spinning wait for the task to finish
    //        var count = 0;
    //        while (_returnedResult == null && count < 20)
    //        {
    //            Thread.Sleep(25);
    //            count++;
    //        }

    //        _loopToken.Cancel();

    //        // Spinning wait for the task to finish
    //        count = 0;
    //        while (!_completed && count < 20)
    //        {
    //            Thread.Sleep(25);
    //            count++;
    //        }
    //    };

    //    It should_have_started = () => _cheapWork.StartTime.ShouldNotBeNull();
    //    It should_have_ended = () => _cheapWork.EndTime.ShouldNotBeNull();
    //    It should_have_returned_result = () => _returnedResult.ShouldNotBeNull();
    //    It should_be_completed = () => _completed.ShouldBeTrue();
    //}
    //public class when_throwing_error_in_task : base_event_loop_specs
    //{
    //    static AsyncWork _cheapWork;
    //    static CancellationTokenSource _workToken;
    //    static IWorkerResult _returnedResult;

    //    Establish context = () =>
    //    {
    //        var cw = CreateCheapWork(r =>
    //        {
    //            _returnedResult = r;
    //        });

    //        _cheapWork = cw.Item2;
    //        _workToken = cw.Item1;
    //    };

    //    Because of = () =>
    //    {
    //        _eventLoop.AddAsyncWork(_cheapWork);

    //        // Spinning wait for the task to finish
    //        var count = 0;
    //        while (_returnedResult == null && count < 20)
    //        {
    //            Thread.Sleep(25);
    //            count++;
    //        }

    //        _loopToken.Cancel();

    //        // Spinning wait for the task to finish
    //        count = 0;
    //        while (!_completed && count < 20)
    //        {
    //            Thread.Sleep(25);
    //            count++;
    //        }
    //    };

    //    It should_have_started = () => _cheapWork.StartTime.ShouldNotBeNull();
    //    It should_have_ended = () => _cheapWork.EndTime.ShouldNotBeNull();
    //    It should_have_returned_result = () => _returnedResult.ShouldNotBeNull();
    //    It should_be_completed = () => _completed.ShouldBeTrue();
    //}

    //public class base_event_loop_specs : base_specs
    //{
    //    protected static EventLoop _eventLoop;
    //    protected static CancellationTokenSource _loopToken;
    //    protected static bool _completed;

    //    Establish context = () =>
    //    {
    //        _eventLoop = new EventLoop();

    //        _loopToken = new CancellationTokenSource();

    //        Task.Run(async () =>
    //        {
    //            await _eventLoop.Loop(_loopToken.Token);
    //            _completed = true;
    //        });
    //    };

    //    public static Tuple<CancellationTokenSource, AsyncWork> CreateCheapWork(Action<IWorkerResult> callback)
    //    {
    //        var ts = new CancellationTokenSource();

    //        Func<CancellationToken, Task<IWorkerResult>> work = (c) => SomeCheapWork(c);
    //        return new Tuple<CancellationTokenSource, AsyncWork>(ts, new AsyncWork(work, ts.Token, callback));
    //    }

    //    private static Task<IWorkerResult> SomeCheapWork(CancellationToken ct)
    //    {
    //        return null;//Task.FromResult<IWorkerResult>(new WorkerCompletedResult());
    //    }

    //    public static Tuple<CancellationTokenSource, AsyncWork> CreateLongRunningWork(Action<IWorkerResult> callback)
    //    {
    //        var ts = new CancellationTokenSource();

    //        Func<CancellationToken, Task<IWorkerResult>> work = (c) => SomeWork(c);
    //        return new Tuple<CancellationTokenSource, AsyncWork>(ts, new AsyncWork(work, ts.Token, callback));
    //    }

    //    private static async Task<IWorkerResult> SomeWork(CancellationToken ct)
    //    {
    //        await Task.Delay(2000);

    //        return null;// new WorkerCompletedResult();
    //    }
    //}
}
