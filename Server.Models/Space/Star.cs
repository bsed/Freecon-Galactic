using Core.Models.Enums;

namespace Server.Models
{
    public class Star
    {
        public float InnerGravityStrength { get; set; }
        public float OuterGravityStrength { get; set; }
        
        public float Density { get; set; }
        public float Radius { get; set; }

        public SunTypes Type { get; set; }

        int Id { get; set; }

        /// <summary>
        /// Creates a sun inside of a system. Scale is in sim units
        /// </summary>
        public Star(float size, float mass, float innerGravityStrength, float outerGravityStrength, SunTypes type)
        {
            Radius = size;
            Density = mass;
            InnerGravityStrength = innerGravityStrength;
            OuterGravityStrength = outerGravityStrength;
            Type = type;

            
        }
             

       
    }
}