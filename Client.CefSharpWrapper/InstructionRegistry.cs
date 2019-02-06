using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.OffScreen;
using Client.View.JSMarshalling;
using Freecon.Core.Utils;
using Newtonsoft.Json;

namespace Freecon.Client.CefSharpWrapper
{
    /// <summary>
    /// A registry for keeping track of functions for Javascript to call.
    /// Callable from Javascript as `ClientInterface.Execute("Foo", "{whatever:true}");`
    /// or `ClientInterface.ExecuteAsync("Foo", "{whatever:true}");`
    /// </summary>
    public class InstructionRegistry
    {
        private ChromiumWebBrowser _browser;

        // The registries of strings -> functions
        private Dictionary<string, Func<JSMarshallContainer, string>> _registry;
        private Dictionary<string, Action<JSMarshallContainer>> _registryVoid;
        private Dictionary<string, Func<JSMarshallContainer, Task<string>>> _registryAsync;

        public InstructionRegistry()
        {
            _registry = new Dictionary<string, Func<JSMarshallContainer, string>>();
            _registryVoid = new Dictionary<string, Action<JSMarshallContainer>>();
            _registryAsync = new Dictionary<string, Func<JSMarshallContainer, Task<string>>>();
        }

        /// <summary>
        /// Calls a registered function and returns a stringified RegistryResponse.
        /// </summary>
        /// <param name="name">Name of the function to call.</param>
        /// <param name="json">The marshallContainer in json format.</param>
        /// <returns>A stringified RegistryResponse from result.</returns>
        public string Execute(string name, string json)
        {
            var marshallContainer = JsonConvert.DeserializeObject<JSMarshallContainer>(json);

            return LowercaseContractResolver.SerializeObject(CallRegisteredFunction(marshallContainer));
        }

        /// <summary>
        /// Calls a registered async function and returns a stringified RegistryResponse.
        /// </summary>
        /// <param name="name">Name of the async function to call.</param>
        /// <param name="json">The marshallContainer in json format.</param>
        /// <returns>A stringified RegistryResponse from result.</returns>
        public async Task<string> ExecuteAsync(string json)
        {
            var marshallContainer = JsonConvert.DeserializeObject<JSMarshallContainer>(json);

            return LowercaseContractResolver.SerializeObject(await CallRegisteredFunctionAsync(marshallContainer));
        }

        private RegistryResponse CallRegisteredFunction(JSMarshallContainer marshallContainer)
        {
            // Check registry for functions that have return values.
            if (_registry.ContainsKey(marshallContainer.MethodName))
            {
                return WrapException(() =>
                {
                    var result = _registry[marshallContainer.MethodName](marshallContainer);

                    return new RegistryResponse(result);
                });
            }

            // Check registry for void functions.
            if (_registryVoid.ContainsKey(marshallContainer.MethodName))
            {
                return WrapException(() =>
                {
                    _registryVoid[marshallContainer.MethodName](marshallContainer);

                    return new RegistryResponse(null, RegistryErrorType.None);
                });
            }

            // Spit out an error if unhandled.
            return new RegistryResponse(null, RegistryErrorType.UnknownValue);
        }

        /// <summary>
        /// Wraps a call to a function that returns a RegistryResponse.
        /// If it throws an exception, wrap it and hand that back.
        /// </summary>
        /// <param name="wrapped">The function to call that returns a RegistryResponse.</param>
        /// <returns>The returned response.</returns>
        private RegistryResponse WrapException(Func<RegistryResponse> wrapped)
        {
            try
            {
                return wrapped();
            }
            catch (Exception e)
            {
                return new RegistryResponse(e);
            }
        }

        private async Task<RegistryResponse> CallRegisteredFunctionAsync(JSMarshallContainer marshallContainer)
        {
            // Spit out error if unhandled.
            if (!_registry.ContainsKey(marshallContainer.MethodName))
            {
                return new RegistryResponse(null, RegistryErrorType.UnknownValue);
            }

            // Async call function and return response.
            try
            {
                var result = await _registryAsync[marshallContainer.MethodName](marshallContainer);

                return new RegistryResponse(result);
            }
            catch (Exception e)
            {
                // Toss error in message response and hand it back.
                return new RegistryResponse(e);
            }
        }

        /// <summary>
        /// Registers this class with the given browser instance.
        /// </summary>
        /// <param name="browser">Instance to register against.</param>
        public void RegisterBrowser(ChromiumWebBrowser browser)
        {
            _browser = browser;

            _browser.RegisterJsObject("ClientInterface", this);
        }

        /// <summary>
        /// Registers a function that accepts a JSMarshallContainer and returns a string.
        /// Callable from Javascript.
        /// </summary>
        /// <param name="name">Function name for Javascript to call.</param>
        /// <param name="instruction">The function to call.</param>
        public void RegisterInstruction(string name, Func<JSMarshallContainer, string> instruction)
        {
            if (_registry.ContainsKey(name))
            {
                throw new AlreadyRegisteredException("Instruction already registered: " + name);
            }

            _registry[name] = instruction;
        }

        /// <summary>
        /// Registers a function that accepts a string and returns void.
        /// Callable from Javascript.
        /// </summary>
        /// <param name="name">Function name for Javascript to call.</param>
        /// <param name="instruction">The function to call.</param>
        public void RegisterInstructionVoid(string name, Action<JSMarshallContainer> instruction)
        {
            if (_registry.ContainsKey(name))
            {
                throw new AlreadyRegisteredException("Instruction already registered: " + name);
            }

            _registryVoid[name] = instruction;
        }

        /// <summary>
        /// Registers an async function that accepts a string and returns string.
        /// Callable from Javascript.
        /// </summary>
        /// <param name="name">Function name for Javascript to call.</param>
        /// <param name="instruction">The async function to call.</param>
        public void RegisterInstruction(string name, Func<JSMarshallContainer, Task<string>> instruction)
        {
            if (_registryAsync.ContainsKey(name))
            {
                throw new AlreadyRegisteredException("Instruction already registered: " + name);
            }

            _registryAsync[name] = instruction;
        }

        /// <summary>
        /// Deregisters an instruction with the given name from all registries.
        /// Will not complain if not registered.
        /// </summary>
        /// <param name="name">Name of function to remove.</param>
        public void UnregisterInstruction(string name)
        {
            if (_registry.ContainsKey(name))
            {
                _registry.Remove(name);
            }

            if (_registryVoid.ContainsKey(name))
            {
                _registryVoid.Remove(name);
            }

            if (_registryAsync.ContainsKey(name))
            {
                _registryAsync.Remove(name);
            }
        }

        public void UnregisterEverything()
        {
            _registry = null;
            _registryVoid = null;
            _registryAsync = null;
        }
    }

    public class AlreadyRegisteredException : Exception
    {
        public AlreadyRegisteredException(string message) : base(message)
        {
        }
    }

    public class RegistryResponse
    {
        public RegistryErrorType ErrorType { get; }

        public string Exception { get; }

        public string Result { get; }

        public RegistryResponse(string result, RegistryErrorType errorType = RegistryErrorType.None)
        {
            Result = result;
            ErrorType = errorType;
        }

        public RegistryResponse(Exception exception)
        {
            Exception = exception.Message;
            ErrorType = RegistryErrorType.UserException;
        }
    }

    public enum RegistryErrorType
    {
        None,
        UnknownValue,
        UserException
    }
}
