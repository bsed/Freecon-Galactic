using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;
using Freecon.Server.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.MongoDB
{
    public class IdData:IDbIdData
    {
        [BsonId]
        public IDTypes IdType { get; set; }

        public int LastIdGenerated { get; set; }

        public HashSet<int> FreeIDs { get; set; }

        public HashSet<int> UsedIDs { get; set; }

        public HashSet<int> ReservedIds { get; set; }

        protected IdData()
        {
            
        }

        public IdData(IDTypes idType, int lastGeneratedId, HashSet<int> freeIDs, HashSet<int> usedIDs, HashSet<int> reservedIDs)
        {
            IdType = idType;
            FreeIDs = freeIDs;
            UsedIDs = usedIDs;
            ReservedIds = reservedIDs;

        }
    }
}
