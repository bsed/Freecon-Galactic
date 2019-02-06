using Freecon.Models.TypeEnums;

namespace Core.Models.CargoHandlers
{
    /// <summary>
    /// Allows cargo to be read and added only. Small probability of adding cargo past fill point if used outside of cargoSynchronizer, we can remove it if necessary.
    /// Simplifies Factory class, enables factories to directly instantiate the objects that they construct.
    /// </summary>
    /// <typeparam name="ModelType"></typeparam>
    public class CargoHandler_ReadAddVM<ModelType>:CargoHandler_ReadOnlyVM<ModelType>
        where ModelType:CargoHandlerModel, new()
    {

        public CargoHandler_ReadAddVM()
        {}

        public CargoHandler_ReadAddVM(CargoHandler_ReadOnlyVM<ModelType> cargo)
        {
            _model = cargo._model;
        
        }
        /// <summary>
        /// Attempts to add numToAdd objects of CargoType type. If suspendBoundsChecking, cargo is added without checking cargo space. Used for temporarily overflowing trades
        /// Returns true if succesful
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToAdd"></param>
        /// <returns></returns>
        public virtual CargoResult AddStatelessCargo(StatelessCargoTypes type, float numToAdd, bool suspendBoundsChecking)
        {

#if !ADMIN            
            
            if (suspendBoundsChecking || CheckCargoSpace(type, numToAdd))//Check if there is enough space to add the cargo
            {
#endif
            if (_model.StatelessCargo.ContainsKey(type))//If there is already at least one of this item type on the ship
                _model.StatelessCargo[type].Quantity += numToAdd;
            else
            {
                _model.StatelessCargo.Add(type, new StatelessCargo(type, numToAdd));
            }
            _model.FilledHolds += numToAdd * StatelessCargo.SpacePerObject(type);
            return CargoResult.Success;

#if !ADMIN
            }
            return CargoResult.NotEnoughCargoSpace;
#endif

        }

        public virtual CargoResult AddStatefulCargo(StatefulCargo c, bool suspendBoundsChecking)
        {

#if !ADMIN

            if (suspendBoundsChecking || CheckCargoSpace(c.CargoType, 1))
            {
#endif
            if (!_model.StatefulCargo.ContainsKey(c.Id))
            {
                _model.StatefulCargo.Add(c.Id, c);
                _model.FilledHolds += StatefulCargo.SpacePerObject(c.CargoType);
                _incrementStatefulCargoCount(c.CargoType);
                return CargoResult.Success;
            }
            else
            {
                return CargoResult.StatefulCargoIDAlreadyAdded;
            }
#if !ADMIN
            }
            else
            {
                return CargoResult.NotEnoughCargoSpace;
                
            }
     
#endif

        }

        protected virtual void _incrementStatefulCargoCount(StatefulCargoTypes t)
        {
            if (_model.StatefulCargoCounts.ContainsKey(t))
                _model.StatefulCargoCounts[t]++;
            else
                _model.StatefulCargoCounts.Add(t, 1);
        }

        protected virtual void _decrementStatefulCargoCount(StatefulCargoTypes t)
        {
            if (_model.StatefulCargoCounts.ContainsKey(t))
            {
                _model.StatefulCargoCounts[t]--;
                if (_model.StatefulCargoCounts[t] == 0)
                    _model.StatefulCargoCounts.Remove(t);//Might be excessive to remove it
            }
        }

    }
}
