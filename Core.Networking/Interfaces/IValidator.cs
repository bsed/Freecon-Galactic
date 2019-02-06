using System;

namespace Freecon.Core.Networking.Interfaces
{
    public interface IValidator<TSubject>
    {
        void Validate(TSubject subject, Action<IValidationError> callback);
    }
}
