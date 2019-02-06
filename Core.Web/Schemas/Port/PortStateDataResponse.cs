using System.Collections.Generic;
using Core.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Models;
using Freecon.Models;
using Freecon.Core.Models.Enums;

namespace Core.Web.Schemas.Port
{
    public class PortStateDataResponse
    {
        public List<IPortGoodCategory> GoodCategories { get; protected set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PortType PortType { get; protected set; }

        public string Name { get; protected set; }

        public PortStateDataResponse(PortModel portModel)
        {
            Name = portModel.AreaName;
            PortType = portModel.PortType;
            GoodCategories = new List<IPortGoodCategory>();

            // TODO: Clean this up to be less verbose.
            var ships = new PortGoodCategory<PortGood>("Ships", new List<PortGood>());
            var weapons = new PortGoodCategory<PortGood>("Weapons", new List<PortGood>());
            var resources = new PortGoodCategory<PortGood>("Resources", new List<PortGood>());
            var defenses = new PortGoodCategory<PortGood>("Planetary Defenses", new List<PortGood>());
            var consumables = new PortGoodCategory<PortGood>("Consumables", new List<PortGood>());//Would be nice to have things to repair health, give temp buffs, etc...
            var components = new PortGoodCategory<PortGood>("Components", new List<PortGood>());
            var modules = new PortGoodCategory<PortGood>("Modules", new List<PortGood>());

            foreach (var uidata in portModel.Cargo.UIComponent.Goods)
            {
                var category = PortHelper.GetPortGoodCategory(uidata.Key);
                var quantity = portModel.Cargo.PortGoodCounts[uidata.Key];
                var purchasePrice = portModel.Cargo.Prices_ShipPurchaseFromPort[uidata.Key];

                switch (category)
                {
                    case PortGoodCategory.Ship:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "asseturl", purchasePrice, quantity);
                        ships.Goods.Add(pg);
                        break;
                    }
                    case PortGoodCategory.Weapon:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "asseturl", purchasePrice, quantity);
                        weapons.Goods.Add(pg);
                        break;
                    }
                    case PortGoodCategory.Resource:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "assetUrl", purchasePrice, quantity);
                        resources.Goods.Add(pg);
                        break;
                    }
                    case PortGoodCategory.Defenses:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "asseturl", purchasePrice, quantity);
                        defenses.Goods.Add(pg);
                        break;
                    }
                    case PortGoodCategory.Consumables:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "asseturl", purchasePrice, quantity);
                        consumables.Goods.Add(pg);
                        break;
                    }
                    case PortGoodCategory.Components:
                    {
                        var pg = new PortGood(uidata.Key, uidata.Value, "asseturl", purchasePrice, quantity);
                        components.Goods.Add(pg);
                        break;

                    }

                }

            }


            foreach (var m in portModel.Cargo.UIComponent.Modules)
            {
                var purchasePrice = 666f;//I don't know exactly how we're going to get the purchase price yet

                var pg = new ModulePortGood(m.Key, PortWareIdentifier.Module, m.Value, "asseturl", purchasePrice, 1);
                modules.Goods.Add(pg);

            }



            GoodCategories.Add(ships);
            GoodCategories.Add(weapons);
            GoodCategories.Add(resources);
            GoodCategories.Add(defenses);
            GoodCategories.Add(consumables);
            GoodCategories.Add(components);
            GoodCategories.Add(modules);


        }


    }





}
