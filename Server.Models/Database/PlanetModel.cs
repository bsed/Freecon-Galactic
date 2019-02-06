using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models
{
    public class PlanetModel : AreaModel
    {
        public override AreaTypes AreaType { get { return AreaTypes.Planet; } }

        public float Gravity { get; set; }
        public bool HasMoons{get; set;}

        public bool IsColonized{get; set;}
        public int? OwnerID{get; set;}
        public int? OwnerDefaultTeamId { get; set; }
        public int? ColonyID { get; set; }
        
        public int LayoutId { get; set; }

        public int Mass{get; set;}
        public PlanetTypes PlanetType{get; set;}
        public byte Scale {get; set;}

        public bool IsMoon { get; set; }



        public PlanetModel()
        {}

        public PlanetModel(AreaModel a) : base(a)
        { }


    }

 
}