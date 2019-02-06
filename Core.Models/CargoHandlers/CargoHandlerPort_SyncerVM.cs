using Freecon.Core.Models.Enums;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using Freecon.Models.UI;
using System;
using System.Linq;

namespace Core.Models.CargoHandlers
{
    public class CargoHandlerPort_SyncerVM : CargoHandler_ReadAddRemoveVM<CargoHandlerPortModel>
    {
        /// <summary>
        /// int argument is num of StatefulCargo_RO.Type in stock
        /// </summary>
        protected Func<StatefulCargo_RO, int, PriceType, float> StatefulCargoPriceGetter;

        protected Func<StatelessCargo, PriceType, float> StatelessCargoPriceGetter;


        public CargoHandlerPort_SyncerVM()
        {

        }

        public CargoHandlerPort_SyncerVM(CargoHandlerPort_ROVM cargo, Func<StatefulCargo_RO, int, PriceType, float> statefulCargoPriceGetter, Func<StatelessCargo, PriceType, float> statelessCargoPriceGetter)
            : base(cargo)
        {
            StatefulCargoPriceGetter = statefulCargoPriceGetter;
            StatelessCargoPriceGetter = statelessCargoPriceGetter;
        }


        /// <summary>
        /// Attempts to ad numToAdd objects of CargoType type
        /// Returns true if succesful
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToAdd"></param>
        /// <returns></returns>
        public override CargoResult AddStatelessCargo(StatelessCargoTypes type, float numToAdd, bool suspendBoundsChecking)
        {

            if (_model.StatelessCargo.ContainsKey(type))//If there is already at least one of this item type on the ship
                _model.StatelessCargo[type].Quantity += numToAdd;
            else
            {
                _model.StatelessCargo.Add(type, new StatelessCargo(type, numToAdd));
            }
            _model.FilledHolds += numToAdd * StatelessCargo.SpacePerObject(type);
            if (StatelessCargoPriceGetter != null)
            {
                SetCargoPurchasePrice(PortHelper.GetPortWareIdentifier(type), StatelessCargoPriceGetter(new StatelessCargo(_model.StatelessCargo[type]), PriceType.PortPurchasing));
                SetCargoSalePrice(PortHelper.GetPortWareIdentifier(type), StatelessCargoPriceGetter(new StatelessCargo(_model.StatelessCargo[type]), PriceType.PortSelling));
            }
            PortWareIdentifier portGoodIdentifier;

            if (Enum.TryParse(type.ToString(), out portGoodIdentifier))
            {
                UpdateGoodCounts(portGoodIdentifier, numToAdd);
                UpdateUIStatLists(portGoodIdentifier, _model.StatelessCargo[type]);
            }

            return CargoResult.Success;

        }

        public override CargoResult AddStatefulCargo(StatefulCargo c, bool suspendBoundsChecking)
        {
            _model.StatefulCargo.Add(c.Id, c);
            _model.FilledHolds += StatefulCargo.SpacePerObject(c.CargoType);
            _incrementStatefulCargoCount(c.CargoType);
            if (StatefulCargoPriceGetter != null)
            {
                SetCargoPurchasePrice(PortHelper.GetPortWareIdentifier(c.CargoType), StatefulCargoPriceGetter(new StatefulCargo_RO(c), GetCargoAmount(c.CargoType), PriceType.PortPurchasing));
                SetCargoSalePrice(PortHelper.GetPortWareIdentifier(c.CargoType), StatefulCargoPriceGetter(new StatefulCargo_RO(c), GetCargoAmount(c.CargoType), PriceType.PortSelling));

            }

            PortWareIdentifier identifier;

            if(c.CargoType == StatefulCargoTypes.Module)
            {
                _model.UIComponent.Modules.Add(c.Id, UIHelper.GetUIData((Module)c));
            }            
            else if (Enum.TryParse(c.CargoType.ToString(), out identifier))
            {
                UpdateGoodCounts(identifier, +1);
                UpdateUIStatLists(identifier, c, true);
            }

            return CargoResult.Success;
        }

        /// <summary>
        /// If numChange is negative, quantity will be removed.
        /// </summary>
        /// <param name="portGoodIdentifier"></param>
        /// <param name="numChange"></param>
        void UpdateGoodCounts(PortWareIdentifier portGoodIdentifier, float numChange)
        {
            if (!_model.PortGoodCounts.ContainsKey(portGoodIdentifier) && numChange >= 0)
            {
                _model.PortGoodCounts.Add(portGoodIdentifier, (int)numChange);                    
                return;
            }
            
            _model.PortGoodCounts[portGoodIdentifier] += (int)numChange;

            if (_model.PortGoodCounts[portGoodIdentifier] <= 0)
                _model.PortGoodCounts.Remove(portGoodIdentifier);
                        
        }

        /// <summary>
        /// Adds if addOrRemove is true, removes otherwise
        /// </summary>
        /// <param name="statefulCargo"></param>
        /// <param name="addOrRemove"></param>
        void UpdateUIStatLists(PortWareIdentifier portGoodIdentifier, StatefulCargo statefulCargo, bool addOrRemove)
        {

            if (addOrRemove)
            {
                if (!_model.UIComponent.Goods.ContainsKey(portGoodIdentifier))
                    _model.UIComponent.Goods.Add(portGoodIdentifier, UIHelper.GetUIData(statefulCargo));
            }
            else
            {
                if (_model.UIComponent.Goods.ContainsKey(portGoodIdentifier))
                    _model.UIComponent.Goods.Remove(portGoodIdentifier);
            }       

        }

        /// <summary>
        /// If quantity <0, set statelessCargo to null to have it removed from the UIComponent
        /// </summary>
        /// <param name="portWareIdentifier"></param>
        /// <param name="statelessCargo"></param>
        void UpdateUIStatLists(PortWareIdentifier portWareIdentifier, StatelessCargo statelessCargo)
        {

            if (statelessCargo == null)
            {
                _model.UIComponent.Goods.Remove(portWareIdentifier);
            }
            else
            {
                var uiData = UIHelper.GetUIData(statelessCargo);
                if (!_model.UIComponent.Goods.ContainsKey(portWareIdentifier))
                {
                    _model.UIComponent.Goods.Add(portWareIdentifier, uiData);
                }
                else
                {
                    _model.UIComponent.Goods[portWareIdentifier] = uiData;
                }

            }


        }      

        /// <summary>
        /// Checks to see if the ship has the cargo type and whether it has at least numToRemove
        /// Returns true is remove is succesful
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToRemove"></param>
        /// <returns></returns>
        public override CargoResult RemoveStatelessCargo(StatelessCargoTypes type, float numToRemove)
        {
            CargoResult result;

            if (!_model.StatelessCargo.ContainsKey(type))
                result = CargoResult.CargoNotInHolds;
            else if (_model.StatelessCargo[type].Quantity > numToRemove)//At least one item will remain
            {
                _model.StatelessCargo[type].Quantity -= numToRemove;
                _model.FilledHolds -= numToRemove * StatelessCargo.SpacePerObject(type);
                if (StatelessCargoPriceGetter != null)
                {
                    SetCargoPurchasePrice(PortHelper.GetPortWareIdentifier(type), StatelessCargoPriceGetter(new StatelessCargo(_model.StatelessCargo[type]), PriceType.PortPurchasing));
                    SetCargoSalePrice(PortHelper.GetPortWareIdentifier(type), StatelessCargoPriceGetter(new StatelessCargo(_model.StatelessCargo[type]), PriceType.PortSelling));
                }
                result = CargoResult.Success;
            }
            else if (_model.StatelessCargo[type].Quantity == numToRemove)//Last item will be removed
            {
                _model.StatelessCargo.Remove(type);

                _model.FilledHolds -= numToRemove * StatelessCargo.SpacePerObject(type);
                _model.Prices_ShipSaleToPort.Remove(PortHelper.GetPortWareIdentifier(type));
                result = CargoResult.Success;

            }
            result = CargoResult.CargoNotInHolds;

            if(result == CargoResult.Success)
            {
                PortWareIdentifier identifier;
              
                if(Enum.TryParse(type.ToString(), out identifier))
                {
                    UpdateGoodCounts(identifier, -numToRemove);
                }
            }

            return result;
        }

        /// <summary>
        /// Removes the stateful cargo with the given ID if it exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override StatefulCargo RemoveStatefulCargo(int id)
        {
            if (_model.StatefulCargo.ContainsKey(id))
            {
                StatefulCargo s = _model.StatefulCargo[id];
                _model.FilledHolds -= Core.Models.StatefulCargo.SpacePerObject(s.CargoType);
                _decrementStatefulCargoCount(s.CargoType);
                if (StatefulCargoPriceGetter != null)
                {
                    SetCargoPurchasePrice(PortHelper.GetPortWareIdentifier(s.CargoType), StatefulCargoPriceGetter(new StatefulCargo_RO(s), GetCargoAmount(s.CargoType), PriceType.PortPurchasing));
                    SetCargoSalePrice(PortHelper.GetPortWareIdentifier(s.CargoType), StatefulCargoPriceGetter(new StatefulCargo_RO(s), GetCargoAmount(s.CargoType), PriceType.PortSelling));
                }


                PortWareIdentifier identifier = PortHelper.GetPortWareIdentifier(s.CargoType);

                if(s.CargoType == StatefulCargoTypes.Module)
                {
                    _model.UIComponent.Modules.Remove(id);
                }
                else if(identifier != PortWareIdentifier.Null)
                {
                    UpdateGoodCounts(identifier, -1);
                    UpdateUIStatLists(identifier, s, false);

                    if(!_model.PortGoodCounts.ContainsKey(identifier))//UpdateGoodCounts will have removed the key if no more goods of this type are in the inventory
                    {
                        _model.Prices_ShipSaleToPort.Remove(identifier);

                    }

                }
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
        public override StatefulCargo RemoveStatefulCargo(StatefulCargoTypes type)
        {
            var s = _model.StatefulCargo.First(e => e.Value.CargoType == type);
            if (s.Value != null)//This might crash?
            {
                _model.StatefulCargo.Remove(s.Key);
                _model.FilledHolds -= Core.Models.StatefulCargo.SpacePerObject(type);
                _decrementStatefulCargoCount(type);
                if (StatefulCargoPriceGetter != null)
                {
                    SetCargoPurchasePrice(PortHelper.GetPortWareIdentifier(type), StatefulCargoPriceGetter(new StatefulCargo_RO(s.Value), GetCargoAmount(s.Value.CargoType), PriceType.PortPurchasing));
                    SetCargoSalePrice(PortHelper.GetPortWareIdentifier(type), StatefulCargoPriceGetter(new StatefulCargo_RO(s.Value), GetCargoAmount(s.Value.CargoType), PriceType.PortSelling));
                } return s.Value;
            }
            else
            {
                return null;
            }
        }

        public virtual bool CheckCargoSpace(StatelessCargoTypes type, int numToAdd)
        {
            return true;

        }

        public override bool CheckCargoSpace(StatefulCargoTypes type, int quantity)
        {
            return true;
        }

        protected virtual void SetCargoSalePrice(PortWareIdentifier i, float price)
        {
            if(i == PortWareIdentifier.Null)
            {
                return;
            }

            if(_model.Prices_ShipSaleToPort.ContainsKey(i))
            {
                _model.Prices_ShipSaleToPort[i] = price;
            }
            else
            {
                _model.Prices_ShipSaleToPort.Add(i, price);
            }
        }

        protected virtual void SetCargoPurchasePrice(PortWareIdentifier i, float price)
        {
            if(i == PortWareIdentifier.Null)
            {
                return;
            }

            if (_model.Prices_ShipPurchaseFromPort.ContainsKey(i))
            {
                _model.Prices_ShipPurchaseFromPort[i] = price;
            }
            else
            {
                _model.Prices_ShipPurchaseFromPort.Add(i, price);
            }
        }
        
      
    }



    public enum PriceType
    {
        //Need to find a better place for this enum...
        PortSelling,
        PortPurchasing,

    }

}
