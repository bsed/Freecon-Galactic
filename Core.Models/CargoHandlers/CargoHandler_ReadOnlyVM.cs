using Freecon.Core.Networking.Models.Objects;
using Freecon.Models.TypeEnums;
using System.Linq;
using Freecon.Core.Interfaces;
using System.Collections.Generic;
using Freecon.Core.Networking.Objects;

namespace Core.Models.CargoHandlers
{
    /// <summary>
    /// This VM should remain limited to read only operations. All mutations must go through CargoSynchronizer
    /// </summary>
    public class CargoHandler_ReadOnlyVM<ModelType>
        where ModelType : CargoHandlerModel, new()
    {
        protected internal ModelType _model;//Abusing internal here, really need a friend keyword...Goal is to enable CargoHandlerSyncerVM to be constructed using _model directly from this class

        public float TotalHolds { get { return _model.TotalHolds; } set { _model.TotalHolds = value; } }

        public float FilledHolds { get { return _model.FilledHolds; } }


        public CargoHandler_ReadOnlyVM()
        {
            _model = new ModelType();
        }

        public CargoHandler_ReadOnlyVM(int totalHolds)
        {
            _model = new ModelType();
            _model.TotalHolds = totalHolds;
            _model.FilledHolds = 0;

        }

        public CargoHandler_ReadOnlyVM(ModelType model)
        {
            _model = model;
        }

        public CargoHandler_ReadOnlyVM(CargoHandler_ReadAddRemoveVM<ModelType> cargoVM)
        {
            _model = cargoVM._model;
        }

        public CargoHandler_ReadOnlyVM(CargoHandler_ReadAddVM<ModelType> cargoVM)
        {
            _model = cargoVM._model;
        }

        /// <summary>
        /// This feels dirty...first time creating an implicit cast
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static implicit operator CargoHandler_ReadOnlyVM<ModelType>(CargoHandlerPort_ROVM c)
        {
            return new CargoHandler_ReadOnlyVM<ModelType>((ModelType)(CargoHandlerModel)c._model);//Whew, looks like type inference in C# needs some work...
        }

        /// <summary>
        /// Returns the current number of StatelessCargo for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public float GetCargoAmount(StatelessCargoTypes type)
        {
            if (_model.StatelessCargo.ContainsKey(type))
            {
                return _model.StatelessCargo[type].Quantity;
            }
            else
            {
                return 0;
            }
        }
        
        public int GetCargoAmount(StatefulCargoTypes type)
        {
            return _model.StatefulCargoCounts[type];
        }

       
        

        /// <summary>
        /// Returns an object of with matching type, if any exists in the CargoHandler, Null otherwise
        /// Does not remove the object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual StatefulCargo GetAnyStatefulCargo(StatefulCargoTypes type)
        {
            var s = _model.StatefulCargo.FirstOrDefault(e => e.Value.CargoType == type);
            if (s.Value != null)//This might crash?
            {
                return s.Value;
            }
            else
            {
                return null;
            }
        }

        public virtual List<StatefulCargo> GetStatefulCargo(List<int> statefulCargoIDs)
        {
            List<StatefulCargo> retList = new List<StatefulCargo>();
            foreach(var i in statefulCargoIDs)
            {
                if(_model.StatefulCargo.ContainsKey(i))
                {
                    retList.Add(_model.StatefulCargo[i]);
                }
            }
            return retList;
        }

        public virtual StatefulCargo GetStatefulCargo(int statefulCargoID)
        {
            if (_model.StatefulCargo.ContainsKey(statefulCargoID))
                return _model.StatefulCargo[statefulCargoID];
            else
                return null;
        }
        /// <summary>
        /// Checks if a _model.StatefulCargo object with the matching id exists. Returns its type if true, StatefulCargoTypes.Null otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual StatefulCargoTypes IsCargoInHolds(int id)
        {
            if (_model.StatefulCargo.ContainsKey(id))
                return _model.StatefulCargo[id].CargoType;
            else
                return StatefulCargoTypes.Null;

        }
        public virtual bool IsCargoInHolds(StatefulCargo c)
        {
            return _model.StatefulCargo.ContainsKey(c.Id);
        }

        /// <summary>
        /// Checks if all ids in statefulCargoIds exist in the cargo. 
        /// </summary>
        /// <param name="statefulCargoIDs"></param>
        /// <returns></returns>
        public virtual bool IsCargoInHolds(List<int> statefulCargoIDs)
        {
            foreach (var i in statefulCargoIDs)
            {
                if (IsCargoInHolds(i) == StatefulCargoTypes.Null)
                    return false;
            }

            return true;
        }

        public virtual bool IsCargoInHolds(Dictionary<StatefulCargoTypes, int> typeAndQuantity)
        {
            foreach(var t in typeAndQuantity)
            {
                if (!IsCargoInHolds(t.Key, t.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the specified quantity of StatefulCargo with matching type exists.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsCargoInHolds(StatefulCargoTypes type, int quantity)
        {
            return _model.StatefulCargo.Select(c => { return c.Value.CargoType == type; }).Count() >= quantity;
        }

        public virtual bool IsCargoInHolds(StatelessCargoTypes type, float quantity)
        {
            return (_model.StatelessCargo.ContainsKey(type) || _model.StatelessCargo[type].Quantity >= quantity);

        }

        public virtual bool IsCargoInHolds(TradeData data)
        {
            var statefulIDs = data.CargoData.StatefulCargo.Select(c => c.Id).ToList();
            if (statefulIDs.Count > 0 && !IsCargoInHolds(statefulIDs))
                return false;

            foreach(var c in data.CargoData.StatelessCargo)
            {
                if (!IsCargoInHolds(c.CargoType, c.Quantity))
                    return false;
            }

            return true;
            
        }
       
        /// <summary>
        /// Checks if there is enough cargo space to add spaceOccupied + cargo space occupied by the specified cargo
        /// Increments spaceOccupied.
        /// returns true if there is enough space
        /// </summary>
        /// <param name="type"></param>
        /// <param name="quantity"></param>
        /// <param name="spaceOccupied">Adds the space occupied by the specified cargo</param>
        /// <returns></returns>
        public virtual bool CheckCargoSpace(StatelessCargoTypes type, float quantity, ref float spaceOccupied)
        {
            spaceOccupied += quantity * StatelessCargo.SpacePerObject(type);
            return (spaceOccupied <= _model.TotalHolds - _model.FilledHolds);
        }

        public virtual bool CheckCargoSpace(Dictionary<StatelessCargoTypes, float> typesAndQuantities, ref float spaceOccupied)
        {            
            foreach (var t in typesAndQuantities)
            {
                spaceOccupied += StatelessCargo.SpacePerObject(t.Key) * t.Value;
            }
            return spaceOccupied <= _model.TotalHolds - _model.FilledHolds;
        }

        public virtual bool CheckCargoSpace(Dictionary<StatefulCargoTypes, float> typesAndQuantities, ref float spaceOccupied)
        {
            foreach(var t in typesAndQuantities)
            {
                spaceOccupied += StatefulCargo.SpacePerObject(t.Key) * (int)t.Value;
            }
            return spaceOccupied <= _model.TotalHolds - _model.FilledHolds;
        }

        public virtual bool CheckCargoSpace(IEnumerable<StatefulCargo> statefulCargo, ref float spaceOccupied)
        {
            foreach(var s in statefulCargo)
            {
                spaceOccupied += StatefulCargo.SpacePerObject(s.CargoType);
            }

            return spaceOccupied <= _model.TotalHolds - _model.FilledHolds;
        }

        public virtual bool CheckCargoSpace(StatefulCargoTypes type, int quantity, ref float spaceOccupied)
        {
            spaceOccupied += Core.Models.StatefulCargo.SpacePerObject(type) * quantity;
            return (spaceOccupied <= _model.TotalHolds - _model.FilledHolds);
        }


        public virtual bool CheckCargoSpace(StatelessCargoTypes type, float quantity)
        {
            float spaceOccupied = 0;
            return CheckCargoSpace(type, quantity, ref spaceOccupied);
        }

        public virtual bool CheckCargoSpace(Dictionary<StatelessCargoTypes, float> typesAndQuantities)
        {
            float spaceOccupied = 0;
            return CheckCargoSpace(typesAndQuantities, ref spaceOccupied);
        }

        public virtual bool CheckCargoSpace(Dictionary<StatefulCargoTypes, float> typesAndQuantities)
        {
            float spaceOccupied = 0;
            return CheckCargoSpace(typesAndQuantities, ref spaceOccupied);
        }

        public virtual bool CheckCargoSpace(IEnumerable<StatefulCargo> statefulCargo)
        {
            float spaceOccupied = 0;
            return CheckCargoSpace(statefulCargo, ref spaceOccupied);
        }

        public virtual bool CheckCargoSpace(StatefulCargoTypes type, int quantity)
        {
            float spaceOccupied = 0;
            return CheckCargoSpace(type, quantity, ref spaceOccupied);
        }
              
        public virtual void WriteStatsToMessage(ShipData data)
        {
            data.Cargo = GetNetworkData();
        }

        public virtual CargoData GetNetworkData()
        {
            return _model.GetNetworkData();
        }

        //public virtual void SetCargoChangedHandler(OnCargoChanged handler)
        //{
        //    CargoChanged += handler;
        //}

        //public virtual void RemoveCargoChangedHandler(OnCargoChanged handler)
        //{
        //    CargoChanged -= handler;
        //}

        public virtual IDBObject GetDBObject()
        {
            return _model.GetClone();
        }

    }
}

