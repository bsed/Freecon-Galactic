namespace Core.Models.Enums
{
    public enum TradeResult
    {
        WaitingForRequestAccept,
        TradeInitialized,

        TradingEntityNotFound,
        PortNotFound,

        ShipANotEnoughCargoSpace,
        ShipBNotEnoughCargoSpace,

        TradeNotFound,
        TradeAlreadyProcessed,

        ShipAlreadyTrading,

        SinglePartyAccepted,

        FailedReasonUnknown,//Temporary

        Success,
    }

}
