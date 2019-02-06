namespace Server.Models
{
    //#region Goods

    ///// <summary>
    ///// Base class for goods that can be bought and sold at ports
    ///// </summary>
    //public class Good
    //{

    //    public int Id { get; set; }

    //    /// <summary>
    //    /// Space of cargo used by item. Not all items are cargo
    //    /// </summary>
    //    public Int16 cargoSpaceTaken = 1;


    //    public StatelessCargoTypes CargoType;//Corresponding cargo type

    //    /// <summary>
    //    /// The port that holds the good
    //    /// </summary>
    //    protected Port port;

    //    public Good() { }

    //    public Good(Port p, GoodTypes goodType, int currentPrice, int numInStock, Int16 cargoSpaceTaken)
    //    {
    //        port = p;
    //        this.currentPrice = currentPrice;
    //        this.numInStock = numInStock;
    //        this.goodType = goodType;
    //        this.cargoSpaceTaken = cargoSpaceTaken;
    //    }

    //    /// <summary>
    //    /// Defines IShip or shields... etc
    //    /// </summary>
    //    public GoodTypes goodType { get; set; }

    //    /// <summary>
    //    /// Price that is displayed in the port
    //    /// </summary>
    //    public int currentPrice { get; set; }

    //    //WANRING: These might be better off as Int16, check later. Probably not.

    //    /// <summary>
    //    /// Number of items in stock; purchasable
    //    /// </summary>
    //    public int numInStock { get; set; }


    //    /// <summary>
    //    /// Returns true if sale was succesful
    //    /// </summary>
    //    /// <param name="s"></param>
    //    /// <param name="numToSell"></param>
    //    /// <returns></returns>
    //    public virtual bool CanSell(IShip s, int numToSell)
    //    {
    //        throw new NotImplementedException("Outfit.trySell is currently broken. Needs to be fixed so that all transactions go through EconomyManager");


    //        if (s.Cargo.CheckCargoSpace(this.CargoType, numToSell)) //Check if the IShip has space for the goods
    //            return false;
    //    }
    //}

    ///// <summary>
    ///// Will have to make a separate good for each type of IShip sold later
    ///// We could always just inherit from ShipGood too
    ///// </summary>
    //public class ShipGood : Good
    //{



    //    public ShipGood() { }

    //    /// <summary>
    //    /// IShip that is sold in a port.
    //    /// </summary>
    //    /// <param name="p">Port that IShip is sold at</param>
    //    /// <param name="shipType"></param>
    //    /// <param name="currentPrice"></param>
    //    /// <param name="numInStock"></param>
    //    /// <param name="cargoSpaceTaken"></param>
    //    public ShipGood(Port p, ShipStats dbs, int currentPrice, int numInStock, Int16 cargoSpaceTaken)
    //        : base(p, GoodTypes.Ship, currentPrice, numInStock, cargoSpaceTaken) // Not sure about inception ships
    //    {
    //        shs = dbs;
    //    }

    //    public ShipStats shs { get; set; }


    //    /// <summary>
    //    /// Returns true if sale was succesful
    //    /// </summary>
    //    /// <param name="s"></param>
    //    /// <param name="numToSell"></param>
    //    /// <returns></returns>
    //    public override bool CanSell(IShip s, int numToSell)
    //    {
    //        throw new NotImplementedException("Outfit.trySell is currently broken. Needs to be fixed so that all transactions go through EconomyManager");


    //        //if (s.ShipStats.ShipType == shs.ShipType)
    //        //{
    //        //    // Fail out if we already own the ship.
    //        //    messageManager.SendChatMessage("You already own this ship.", "", (byte) ChatTypes.error,
    //        //                                   messageManager.Server, s.GetPlayer().connection);
    //        //    return false;
    //        //}
    //        //if (EconomyManager.trySell(port, s.GetPlayer(), currentPrice))
    //        //{
    //        //    ShipManager.ChangePlayersShip(s, shs);

    //        //    messageManager.SendChatMessage("You have purchased a new ship.", "", (byte) ChatTypes.alert,
    //        //                                   messageManager.Server, s.GetPlayer().connection);            
    //        //    return true;
    //        //}
    //        //else
    //        //    messageManager.SendChatMessage("You cannot afford to buy this.", "", (byte) ChatTypes.error,
    //        //                                   messageManager.Server, s.GetPlayer().connection);
    //        return false;
    //    }
    //}

    //public class Woman : Good
    //{
    //    public Woman() { }

    //    public Woman(Port p, GoodTypes goodType, int currentPrice, int numInStock, Int16 cargoSpaceTaken)
    //        : base(p, goodType, currentPrice, numInStock, cargoSpaceTaken)
    //    {
    //        goodType = GoodTypes.Woman;
    //        cargoSpaceTaken = 10;
    //    }

    //    /// <summary>
    //    /// Returns true if sale was succesful
    //    /// </summary>
    //    /// <param name="s"></param>
    //    /// <param name="numToSell"></param>
    //    /// <returns></returns>
    //    public override bool CanSell(IShip s, int numToSell)
    //    {
    //        throw new NotImplementedException("Try again later.");

    //        //if (base.TrySell(s, numToSell, messageManager))//Checks for space and cash and handles cash exchange
    //        //{
    //        //    s.tryAddCargo(CargoTypes.Woman, numToSell);
    //        //    return true;
    //        //}
    //        //return false;
    //    }
    //}

    //#endregion
}
