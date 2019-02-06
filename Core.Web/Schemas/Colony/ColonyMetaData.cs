using Freecon.Models.TypeEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Web.Schemas
{
    /// <summary>
    /// Data about the colony's type, name, etc.
    /// </summary>
    public class ColonyMetaData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanetTypes PlanetType { get; set; }

        public string ColonyName { get; set; }
    }
}