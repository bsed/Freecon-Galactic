using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using Server.MongoDB;

namespace Server.Models.Database
{
    public class TeamModel: IDBObject
    {
        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
        public int Id { get; set; }

        public byte[] PlayerIDs { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.TeamModel; } }

    }
}
