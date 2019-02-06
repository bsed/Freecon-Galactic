using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;
using Freecon.Models.UI;
using Freecon.Core.Models.Enums;

namespace Core.Models.CargoHandlers
{
    public class CargoHandlerPortModel : CargoHandlerModel
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PortWareIdentifier, float> PortGoodCounts { get; set; }

        /// <summary>
        /// Ship purchasing from port
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PortWareIdentifier, float> Prices_ShipPurchaseFromPort { get; set; }

        /// <summary>
        /// Ship selling to port
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PortWareIdentifier, float> Prices_ShipSaleToPort { get; set; }
                
        public PortModelUIComponent UIComponent { get; set; }

        public CargoHandlerPortModel()
        {
            Prices_ShipPurchaseFromPort = new Dictionary<PortWareIdentifier, float>();
            Prices_ShipSaleToPort = new Dictionary<PortWareIdentifier, float>();
            PortGoodCounts = new Dictionary<PortWareIdentifier, float>();


            UIComponent = new PortModelUIComponent();        
        
        }
        

    }

    /// <summary>
    /// Precomputed containers for the UI
    /// </summary>
    public class PortModelUIComponent
    {
        


        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PortWareIdentifier, UIDisplayData> Goods { get; set; }

        //public Dictionary<PortGoodIdentifier, List<KeyValuePair<string, object>>> ShipStats { get; set; }

        //public Dictionary<PortGoodIdentifier, List<KeyValuePair<string, object>>> WeaponStats { get; set; }

        //public Dictionary<PortGoodIdentifier, List<KeyValuePair<string, object>>> Defenses { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, UIDisplayData> Modules { get; set; }

        public PortModelUIComponent()
        {
            //ShipStats = new Dictionary<PortGoodIdentifier, List<KeyValuePair<string, object>>>();
            //WeaponStats = new Dictionary<PortGoodIdentifier, List<KeyValuePair<string, object>>>();

            Goods = new Dictionary<PortWareIdentifier, UIDisplayData>();

            Modules = new Dictionary<int, UIDisplayData>();

        }

    }


}
