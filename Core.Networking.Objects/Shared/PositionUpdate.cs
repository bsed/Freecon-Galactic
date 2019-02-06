namespace Freecon.Core.Networking.Models.Objects
{
    public class PositionUpdate : ICommMessage
    {
        public TodoMessageTypes PayloadType { get { return TodoMessageTypes.PositionUpdateData; } }
        
        public float X { get; set; }

        public float Y { get; set; }

        public PositionUpdate(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
