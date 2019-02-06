using Core.Models.Enums;
using Freecon.Core.Models.Enums;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.CargoHandlers
{
    public class CargoHandlerPort_ROVM : CargoHandler_ReadOnlyVM<CargoHandlerPortModel>
    {

        public CargoHandlerPort_ROVM()
        {
            _model.TotalHolds = float.MaxValue;
        }

        public CargoHandlerPort_ROVM(CargoHandler_ReadOnlyVM<CargoHandlerModel> cargoHandler)
        {
            _model = (CargoHandlerPortModel)cargoHandler._model;
        }

        public CargoHandlerPort_ROVM(CargoHandler_ReadOnlyVM<CargoHandlerPortModel> cargoHandler)
        {
            _model = cargoHandler._model;
        }

        public CargoHandlerPort_ROVM(CargoHandlerPortModel model)
        {
            _model = model;
        }


        /// <summary>
        /// Checks if at quantity of cargoType is avaliable and is for sale by port
        /// </summary>
        /// <param name="cargoType"></param>
        /// <returns></returns>
        public bool IsCargoForSale(Dictionary<StatefulCargoTypes, float> typesAndQuantities)
        {
            try//This probably isn't thread safe, hence the try catch
            {
                foreach(var t in typesAndQuantities)
                {
                    if(_model.StatefulCargo.Select(c => { return c.Value.CargoType == t.Key && c.Value.IsForSale; }).Count() < t.Value)
                        return false;
                }
            }            
            catch
            {
                return false;
            }

            return true;
           
        }

        /// <summary>
        /// Checks if the given statefulCargoIDs are in stock and for sale
        /// </summary>
        /// <param name="statefulCargoIDs"></param>
        /// <returns>Returns a list of the StatefulCargo objects that are in stock and for sale</returns>
        public List<StatefulCargo> IsCargoForSale(List<int> statefulCargoIDs)
        {
            List<StatefulCargo> retList = new List<StatefulCargo>();
            foreach(var i in statefulCargoIDs)
            {
                if (_model.StatefulCargo.ContainsKey(i))
                    retList.Add(_model.StatefulCargo[i]);
            }

            return retList;
        }

        /// <summary>
        /// Checks if this port is buying the specified type
        /// </summary>
        /// <param name="portWareIdentifier"></param>
        /// <returns></returns>
        public bool IsCargoForPurchase(PortWareIdentifier portWareIdentifier)
        {
            return true;
        }

        public float GetPrice(Dictionary<StatefulCargoTypes, float> typesAndQuantities, PortTradeDirection direction)
        {
            float totalPrice = 0;
            foreach(var t in typesAndQuantities)
            {
                totalPrice += GetPrice(t.Key, (int)t.Value, direction);
            }

            return totalPrice;
        }

        public float GetPrice(Dictionary<StatelessCargoTypes, float> typesAndQuantities, PortTradeDirection direction)
        {
            float totalPrice = 0;
            foreach (var t in typesAndQuantities)
            {
                totalPrice += GetPrice(t.Key, t.Value, direction);
            }

            return totalPrice;
        }
              

        public float GetPrice(StatelessCargoTypes cargoType, float quantity, PortTradeDirection direction)
        {
            var identifier = PortHelper.GetPortWareIdentifier(cargoType);
            return GetPrice(identifier, quantity, direction);
        }

        public float GetPrice(StatefulCargoTypes cargoType, int quantity, PortTradeDirection direction)
        {
            var identifier = PortHelper.GetPortWareIdentifier(cargoType);
            return GetPrice(identifier, quantity, direction);

        }

        public float GetPrice(List<StatefulCargo> statefulCargoObjects, PortTradeDirection direction)
        {
            float sum = 0;
            foreach(var s in statefulCargoObjects)
            {
                sum += GetPrice(s.CargoType, 1, direction);
            }

            return sum;
        }

        public float GetPrice(PortServiceTypes serviceType, float quantity, PortTradeDirection direction)
        {
            var identifier = PortHelper.GetPortWareIdentifier(serviceType);
            return GetPrice(identifier, quantity, direction);
            
        }

        public float GetPrice(PortWareIdentifier identifier, float quantity, PortTradeDirection direction)
        {
            switch(direction)
            {
                case PortTradeDirection.ShipPurchaseFromPort:
                    {
                        if (_model.Prices_ShipPurchaseFromPort.ContainsKey(identifier))
                            return _model.Prices_ShipPurchaseFromPort[identifier] * quantity;
                        else
                            return float.MaxValue;
                    }
                case PortTradeDirection.ShipSaleToPort:
                    {
                        if (_model.Prices_ShipSaleToPort.ContainsKey(identifier))
                            return _model.Prices_ShipSaleToPort[identifier] * quantity;
                        else
                            return float.MaxValue;
                    }
                default:
                    {
                        throw new NotImplementedException("Sale direction " + direction.ToString() + " not implemented.");
                    }


            }

            
        }
          
    }



}
