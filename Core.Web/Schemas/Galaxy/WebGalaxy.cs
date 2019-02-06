using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Web.Schemas.Galaxy
{
    /// <summary>
    /// Galaxy class that exists so that we can have a JSON-friendly format for this.
    /// </summary>
    public class WebGalaxy
    {
        [JsonProperty(PropertyName = "galaxyAttributes")]
        public List<WebMetaAttribute> GalaxyAttributes { get; protected set; }

        /// <summary>
        /// The star systems for the galaxy.
        /// </summary>
        [JsonProperty(PropertyName = "systems")]
        public List<WebSystem> Systems { get; protected set; }

        public WebGalaxy(List<WebSystem> systems, List<WebMetaAttribute> galaxyAttributes)
        {
            Systems = systems;
            GalaxyAttributes = galaxyAttributes;
        }
    }

    public class WebSystem
    {
        [JsonProperty(PropertyName = "systemAttributes")]
        public List<WebMetaAttribute> SystemAttributes { get; protected set; }

        [JsonProperty(PropertyName = "planets")]
        public List<WebPlanet> Planets { get; protected set; }

        [JsonProperty(PropertyName = "ports")]
        public List<WebPort> Ports { get; protected set; }

        /// <summary>
        /// Unique Id for the system in the galaxy.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; protected set; }

        public WebSystem(List<WebPlanet> planets, List<WebPort> ports, List<WebMetaAttribute> systemAttributes, int id)
        {
            Planets = planets;
            SystemAttributes = systemAttributes;
            Id = id;
            Ports = ports;
        }
    }

    /// <summary>
    /// Allows us to tag any additional meta data that we need to.
    /// </summary>
    public class WebMetaAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; protected set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; protected set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "attributeType")]
        public WebMetaAttibuteType AttributeType { get; protected set; }

        public WebMetaAttribute(string name, string value, WebMetaAttibuteType attributeType)
        {
            Name = name;
            Value = value;
            AttributeType = attributeType;
        }
    }

    public enum WebMetaAttibuteType
    {
        str,
        int32,
        float32
    }

    /// <summary>
    /// Planets that are contained inside of a parent system.
    /// </summary>
    public class WebPlanet
    {
        [JsonProperty(PropertyName = "planetAttributes")]
        public List<WebMetaAttribute> PlanetAttributes { get; protected set; }

        /// <summary>
        /// Unique ID for the planet in the galaxy.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; protected set; }

        [JsonProperty(PropertyName = "planetType")]
        public string PlanetType { get; protected set; }

        [JsonProperty(PropertyName = "x")]
        public int X { get; protected set; }

        [JsonProperty(PropertyName = "y")]
        public int Y { get; protected set; }

        /// <summary>
        /// Id of the star system that contains us.
        /// </summary>
        [JsonProperty(PropertyName = "parentSystemId")]
        public int ParentSystemId { get; protected set; }

        /// <summary>
        /// Optional. Used if this is a moon. Tells us what we "orbit".
        /// If this is set, then X,Y are offsets from the parent.
        /// </summary>
        [JsonProperty(PropertyName = "parentPlanetId")]
        public int? ParentPlanetId { get; protected set; }

        public WebPlanet(List<WebMetaAttribute> planetAttributes, string planetType, int x, int y, int id, int? parentPlanetId, int parentSystemId)
        {
            PlanetAttributes = planetAttributes;
            PlanetType = planetType;
            X = x;
            Y = y;
            Id = id;
            ParentPlanetId = parentPlanetId;
            ParentSystemId = parentSystemId;
        }
    }

    /// <summary>
    /// These are ports. They're effectively moons because they must orbit a planet.
    /// </summary>
    public class WebPort
    {
        [JsonProperty(PropertyName = "portAttributes")]
        public List<WebMetaAttribute> PortAttributes { get; protected set; }

        /// <summary>
        /// Unique Id for the port in the galaxy.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; protected set; }

        [JsonProperty(PropertyName = "portType")]
        public string PortType { get; protected set; }

        [JsonProperty(PropertyName = "x")]
        public int X { get; protected set; }

        [JsonProperty(PropertyName = "y")]
        public int Y { get; protected set; }

        /// <summary>
        /// Id of the star system that contains us.
        /// </summary>
        [JsonProperty(PropertyName = "parentSystemId")]
        public int ParentSystemId { get; protected set; }

        /// <summary>
        /// Tells port what we "orbit".
        /// X,Y are offsets from the parent.
        /// </summary>
        [JsonProperty(PropertyName = "parentPlanetId")]
        public int ParentPlanetId { get; protected set; }

        public WebPort(List<WebMetaAttribute> portAttributes, string portType, int x, int y, int id, int parentPlanetId, int parentSystemId)
        {
            PortAttributes = portAttributes;
            PortType = portType;
            X = x;
            Y = y;
            Id = id;
            ParentPlanetId = parentPlanetId;
            ParentSystemId = parentSystemId;
        }
    }
}
