using Core.Models.CargoHandlers;
using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using System.Collections.Generic;

namespace Server.Models
{
    public class PortModel:AreaModel
    {
        public override AreaTypes AreaType { get { return AreaTypes.Port; } }

        public int CashFromSales { get; set; }

        public CargoHandlerPortModel Cargo { get; set; }
        public List<PortService> ServicesForSale { get; set; }

        //Do we need this?
        public PortType PortType { get; set; }



        public int Size { get; set; }

        public bool IsMoon { get; set; }

        public PortModel() 
        {
            Cargo = new CargoHandlerPortModel();
            ServicesForSale = new List<PortService>();
            CashFromSales = 0;
        }

        public PortModel(PortModel a):base(a)
        {
            CashFromSales = a.CashFromSales;
            Cargo = a.Cargo;
            ServicesForSale = a.ServicesForSale;

        }

    }

    
}