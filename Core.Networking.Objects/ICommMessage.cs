using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Models
{
    public interface ICommMessage
    {
        TodoMessageTypes PayloadType { get; }
    }
}
