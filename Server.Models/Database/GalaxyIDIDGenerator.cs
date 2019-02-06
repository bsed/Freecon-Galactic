using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization;

namespace Server.MongoDB
{
    /// <summary>
    /// Overrides Mongo's id in favor of our own galaxyIDs
    /// </summary>
    public class GalaxyIDIDGenerator : IIdGenerator
    {
        /// <summary>
        /// Returns IHasGalaxyID.Id
        /// </summary>
        /// <param name="container"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public object GenerateId(object container, object document)
        {
            return ((IHasGalaxyID)document).Id;
        }

        public bool IsEmpty(object o)
        {
            return false;
        }



    }
}
