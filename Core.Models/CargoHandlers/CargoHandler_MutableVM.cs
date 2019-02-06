using Freecon.Models;
using Freecon.Models.TypeEnums;
using System.Linq;

namespace Core.Models.CargoHandlers
{
    /// <summary>
    /// This view model should only be used by CargoSynchronizer
    /// </summary>
    public class CargoHandler_ReadAddRemoveVM<ModelType> : CargoHandler_ReadAddVM<ModelType>, ISyncerCargoHandler
        where ModelType : CargoHandlerModel, new()
    {

        public int ModelId { get { return _model.Id; } }

        public CargoHandler_ReadAddRemoveVM()
        { }

        public CargoHandler_ReadAddRemoveVM(CargoHandler_ReadOnlyVM<ModelType> cargo):base(cargo)
        {
            
        }

        public CargoHandler_ReadAddRemoveVM(CargoHandler_ReadAddVM<ModelType> cargo):base(cargo)
        {

        }            

        public CargoHandler_ReadAddRemoveVM(ModelType cargoHandlerModel)
        {
            _model = cargoHandlerModel;
        }

        /// <summary>
        /// Checks to see if the ship has the cargo type and whether it has at least numToRemove
        /// Returns true is remove is succesful
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToRemove"></param>
        /// <returns></returns>
        public virtual CargoResult RemoveStatelessCargo(StatelessCargoTypes type, float numToRemove)
        {
            if (!_model.StatelessCargo.ContainsKey(type))
                return CargoResult.CargoNotInHolds;
            else if (_model.StatelessCargo[type].Quantity > numToRemove)//At least one item will remain
            {
                _model.StatelessCargo[type].Quantity -= numToRemove;
                _model.FilledHolds -= numToRemove * StatelessCargo.SpacePerObject(type);

                return CargoResult.Success;
            }
            else if (_model.StatelessCargo[type].Quantity == numToRemove)//Last item will be removed
            {
                _model.StatelessCargo.Remove(type);

                _model.FilledHolds -= numToRemove * StatelessCargo.SpacePerObject(type);
                return CargoResult.Success;

            }
            return CargoResult.CargoNotInHolds;

        }

        /// <summary>
        /// Removes the stateful cargo with the given ID if it exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual StatefulCargo RemoveStatefulCargo(int id)
        {
            if (_model.StatefulCargo.ContainsKey(id))
            {
                StatefulCargo s = _model.StatefulCargo[id];
                _model.FilledHolds -= Core.Models.StatefulCargo.SpacePerObject(s.CargoType);
                _decrementStatefulCargoCount(s.CargoType);
                _model.StatefulCargo.Remove(id);
                return s;
            }
            else
                return null;

        }

        /// <summary>
        /// Removes the stateful cargo with the given ID if it exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual StatefulCargo RemoveStatefulCargo(StatefulCargoTypes type)
        {
            var s = _model.StatefulCargo.First(e => e.Value.CargoType == type);
            if (s.Value != null)//This might crash?
            {
                _model.StatefulCargo.Remove(s.Key);
                _model.FilledHolds -= Core.Models.StatefulCargo.SpacePerObject(type);
                _decrementStatefulCargoCount(type);
                _model.StatefulCargo.Remove(s.Key);
                return s.Value;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// DON'T USE THIS EVER FOR ANYTHING UNDER ANY CIRCUMSTANCES
        /// </summary>
        /// <returns></returns>
        public void CHEATADDCARGO(StatelessCargoTypes type, int amount)
        {
            //Adds without checking or changing capacity. Cheap way to fuck around with "infinite" holds. Will remove eventually.
            if (_model.StatelessCargo.ContainsKey(type))
                _model.StatelessCargo[type].Quantity += amount;
            else
                _model.StatelessCargo.Add(type, new StatelessCargo(type, amount));
        }

        /// <summary>
        /// DON'T USE THIS EVER FOR ANYTHING UNDER ANY CIRCUMSTANCES
        /// </summary>
        /// <returns></returns>
        public void CHEATADDCARGO(StatefulCargo c)
        {
            //Adds without checking or changing capacity. Cheap way to fuck around with "infinite" holds. Will remove eventually.
            _model.StatefulCargo.Add(c.Id, c);

        }

    }

    public enum CargoResult
    {
        Success,

        IDNotRegistered,
        NotEnoughCargoSpace,
        CargoNotInHolds,
        IDLockAttemptTimeout,
        SequenceFailed,
        RolledBack,
        StatefulCargoIDAlreadyAdded,

    }
}
