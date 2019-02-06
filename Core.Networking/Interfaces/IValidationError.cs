using System;

namespace Freecon.Core.Networking.Interfaces
{

    public interface IValidationError
    {
        Exception Exception { get; }
    }
}
