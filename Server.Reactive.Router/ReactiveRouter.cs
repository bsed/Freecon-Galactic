using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Reactive.Router
{
    public class ReactiveRouter : IReactiveRouter
    {
        private Action<Exception> _errorHandler;
        private Dictionary<Type, object> _contracts;

        public ReactiveRouter(Action<Exception> errorHandler)
        {
            _errorHandler = errorHandler;

            _contracts = new Dictionary<Type, object>();
        }

        public void RegisterPublisher<T>(IObservable<T> provider)
        {
            var contract = GetOrCreateContract<T>();

            contract.AddProvider(provider);
        }

        public ISubject<T> RegisterPublisher<T>()
        {
            var contract = GetOrCreateContract<T>();

            var subject = new Subject<T>();

            contract.AddProvider(subject);

            return subject;
        }

        public IObservable<T> RegisterSubscriber<T>()
        {
            return GetOrCreateContract<T>().GetSubscription();
        }

        private Contract<T> GetOrCreateContract<T>()
        {
            var publishType = typeof(T);

            // Get existing contract.
            object boxedContract;
            if (_contracts.TryGetValue(publishType, out boxedContract))
            {
                return (Contract<T>)boxedContract;
            }

            // Otherwise, create a new one and register it.
            var contract = new Contract<T>(_errorHandler);

            _contracts.Add(publishType, contract);

            return contract;
        }
    }

    public class Contract<T> : IContract<T>
    {
        private Action<Exception> _errorHandler;

        private List<IObservable<T>> _providers;

        private Subject<T> _subscription;

        public Contract(Action<Exception> errorHandler) 
        {
            _errorHandler = errorHandler;

            _providers = new List<IObservable<T>>();

            _subscription = new Subject<T>();
        }

        public void AddProvider(IObservable<T> newProvider)
        {
            // Provider already registered.
            if (_providers.Contains(newProvider))
            {
                return;
            }

            // Add the new provider to our list.
            _providers.Add(newProvider);

            // Fire our subscriber to notify our clients.
            newProvider.Subscribe(
                (next) => _subscription.OnNext(next),
                _errorHandler,
                // Remove our publisher
                () =>
                {
                    if (_providers.Contains(newProvider))
                    {
                        _providers.Remove(newProvider);
                    }
                }
            );
        }

        public IObservable<T> GetSubscription() 
        {
            return _subscription;
        }
    }
}
