using System;
using System.Reactive.Subjects;

namespace Reactive.Router
{
    public interface IReactiveRouter
    {
        void RegisterPublisher<T>(IObservable<T> provider);

        ISubject<T> RegisterPublisher<T>();

        IObservable<T> RegisterSubscriber<T>();
    }

    public interface IContract<T>
    {
        void AddProvider(IObservable<T> newProvider);

        IObservable<T> GetSubscription();
    }
}
