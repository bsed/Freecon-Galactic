using System;

namespace Freecon.Core.Networking.Interfaces
{
    public interface IValidatorStack<TSubject>
    {
        void Validate(TSubject subject, Action<IValidationError> callback, Func<IValidationError> error);
    }
}
