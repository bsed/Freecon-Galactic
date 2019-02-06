using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Freecon.Server.Core.Interfaces
{
    public interface IDbIdData
    {
        [BsonId]
        IDTypes IdType { get; set; }

        int LastIdGenerated { get; set; }

        HashSet<int> FreeIDs { get; set; }

        HashSet<int> UsedIDs { get; set; }

        HashSet<int> ReservedIds { get; set; }
    }
}
