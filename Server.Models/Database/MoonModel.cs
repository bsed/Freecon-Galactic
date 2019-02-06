//using Freecon.Models.TypeEnums;
//using MongoDB.Bson.Serialization.Attributes;

//namespace Server.Models
//{
//    //This should probably inherit from planet
//    public class MoonModel : AreaModel
//    {
//        public int LayoutId { get; set; }

//        public byte Scale { get; set; }

//        public virtual PlanetTypes PlanetType { get; set; }

//        public override AreaTypes AreaType { get { return AreaTypes.Moon; } }

//        public MoonModel()
//        { }

//        public MoonModel(AreaModel a) : base(a)
//        {            
//        }

//    }
//}