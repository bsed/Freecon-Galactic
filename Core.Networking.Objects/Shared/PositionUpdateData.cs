namespace Freecon.Core.Networking.Models.Objects
{
    public enum PositionUpdateTargetType
    {
        Ship,
        Projectile
    }

    public class PositionUpdateData
    {
        public PositionUpdateTargetType TargetType { get; set; }
        public int TargetId { get; set; }
        public float XPos { get; set; }
        public float YPos { get; set; }
        public float Rotation { get; set; }
        public float XVel { get; set; }
        public float YVel { get; set; }
        public float AngularVelocity { get; set; }
        public float CurrentShields { get; set; }
        public float CurrentHealth { get; set; }
        public bool Thrusting { get; set; }
        public float Timestamp { get; set; }

        public PositionUpdateData() 
        { }

        public PositionUpdateData(PositionUpdateTargetType targetType, int targetId, float xPos, float yPos, float rotation, float xVel, float yVel, float angularVelocity, float currentShields, float currentHealth, bool thrusting, float timestamp)
        {
            TargetType = targetType;
            TargetId = targetId;
            XPos = xPos;
            YPos = yPos;
            Rotation = rotation;
            XVel = xVel;
            YVel = yVel;
            AngularVelocity = angularVelocity;
            CurrentShields = currentShields;
            CurrentHealth = currentHealth;
            Thrusting = thrusting;
            Timestamp = timestamp;
        }
    }
}
