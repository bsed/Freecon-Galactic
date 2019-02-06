namespace Freecon.Core.Networking.Models.Objects
{
    public class RawMessageContainer : ICommMessage
    {
        public RawMessageContainer(TodoMessageTypes payloadType, byte[] data)
        {
            PayloadType = payloadType;
            Data = data;
        }

        public TodoMessageTypes PayloadType { get; set; }

        public byte[] Data { get; set; }
    }

    public enum TodoMessageTypes : uint
    {
        LoginRequest,
        nothing2,
        nothing3,
        nothing4,
        PositionUpdateData
    }
}
