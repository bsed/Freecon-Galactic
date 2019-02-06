using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;
using Freecon.Core.Interfaces;
using Freecon.Models;
using Freecon.Core.Networking.Models.Objects;

namespace Core.Models.CargoHandlers
{

    public delegate float StatelessCargoPriceGetter(StatelessCargo c);

    public delegate float StatefulCargoPriceGetter(StatefulCargo c);


    
    public class CargoHandlerModel : IDBObject, IHasUIData
    {
        public string UIDisplayName { get { return "Cargo Data"; } }

        public int Id { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        [UICollection]
        public Dictionary<StatelessCargoTypes, StatelessCargo> StatelessCargo { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        [UICollection]
        public Dictionary<int, StatefulCargo> StatefulCargo { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<StatefulCargoTypes, int> StatefulCargoCounts { get; set; }        

        [UIProperty]
        public float TotalHolds { get; set; }

        public float FilledHolds { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.CargoHandlerModel; } }

        public CargoHandlerModel()
        {
            TotalHolds = 200;
            FilledHolds = 0;
            StatelessCargo = new Dictionary<StatelessCargoTypes, StatelessCargo>();
            StatefulCargo = new Dictionary<int, StatefulCargo>();            
            StatefulCargoCounts = new Dictionary<StatefulCargoTypes, int>();
        }

        public virtual CargoData GetNetworkData()
        {
            CargoData data = new CargoData();

            data.TotalHolds = TotalHolds;
            data.FilledHolds = FilledHolds;

            foreach (var c in StatelessCargo)
            {
                data.StatelessCargo.Add(new StatelessCargoData() { CargoType = c.Key, Quantity = c.Value.Quantity });
            }

            foreach (var c in StatefulCargo)
            {
                data.StatefulCargo.Add(c.Value.GetNetworkObject());
            }

            return data;
        }

        public CargoHandlerModel GetClone()
        {
            return (CargoHandlerModel)MemberwiseClone();
        }
    }


}
