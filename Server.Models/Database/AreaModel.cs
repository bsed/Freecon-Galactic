using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;
using Server.MongoDB;
using System.Collections.Generic;
using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization.Options;
using Server.Models.Space;

namespace Server.Models
{

    public abstract class AreaModel : IDBObject
    {
        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
        public int Id { get; set; }
                
        public int IDToOrbit { get; set; }

        public int AreaSize { get; set; }

        public int? ParentAreaID { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, IFloatyAreaObject> FloatySpaceObjects { get; set; }

        //[ForeignKey("Id")]
        //public ICollection<IStructure> Structures { get; set; }
        //public byte[] SerializedStructureIDs { get; set; }
        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        //public Dictionary<int, IStructure> Structures { get; set; }
        public HashSet<int> StructureIDs { get ;set;}

        public HashSet<int> ShipIDs { get; set; }

        //public List<int> ShipIDs { get; set; }

        public byte SecurityLevel { get; set; }

        public string AreaName { get; set; }

        [BsonElement]
        public abstract AreaTypes AreaType { get; }

        public List<Warphole> Warpholes { get; set; }
        public float PosY { get; set; }
        public float PosX { get; set; }

        public int CurrentTrip { get; set; }
        public int Distance { get; set; }
        public int MaxTrip { get; set; }

        public List<ResourcePool> ResourcePools { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.AreaModel; } }

        public AreaModel()
        {
            _setDefaults();
        }

        public AreaModel(AreaModel a)
        {
            _setDefaults();

            Id = a.Id;
            IDToOrbit = a.IDToOrbit;
            AreaSize = a.AreaSize;
            ParentAreaID = a.ParentAreaID;

            StructureIDs = a.StructureIDs;            
            ShipIDs = a.ShipIDs;
            SecurityLevel = a.SecurityLevel;
            AreaName = a.AreaName;
            Warpholes = a.Warpholes;
            PosY = a.PosY;
            PosX = a.PosX;

            MaxTrip = a.MaxTrip;
            Distance = a.Distance;
            CurrentTrip = a.CurrentTrip;

            ResourcePools = a.ResourcePools;

        }


        private void _setDefaults()
        {
            ShipIDs = new HashSet<int>();
            Warpholes = new List<Warphole>();
            //Structures = new Dictionary<int, IStructure>();
            StructureIDs = new HashSet<int>();
            
            ResourcePools = new List<ResourcePool>();
        }

        public IDBObject GetClone()
        {
            return (IDBObject)MemberwiseClone();
        }
    }
}