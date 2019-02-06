using Freecon.Core.Interfaces;
using Freecon.Core.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Server.Interfaces;
using Server.MongoDB;
using System;
using System.Collections.Generic;


namespace Server.Models
{

    /// <summary>
    /// Records a trade.
    /// </summary>
    /// [TableName("trades")]
    public class TradeRecordModel:IDBObject, ISerializable
    {
        /// <summary>
        /// Time at which the trade occured
        /// </summary>
        public DateTime DateTime { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.TradeRecordModel; } }

        /// <summary>
        /// The slave on which the trade occured
        /// </summary>
        public int SlaveID { get; set; }

        protected TradeRecordModel(){}

        public TradeRecordModel(DateTime dateTime, int slaveID)
        {
            DateTime = dateTime;
            SlaveID = slaveID;
        }

        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
        public int Id { get; set; }

        public virtual IDBObject GetDBObject()
        {
            return this;
        }

    }

    /// <summary>
    /// Assuming port transactions will be 1 item type at a time
    /// </summary>
    public class PortShipTradeRecordModel:TradeRecordModel
    {
        /// <summary>
        /// Save a snapshot of the ship? Can remove if it gets excessive
        /// </summary>
        public IShip Ship { get; set; }

        /// <summary>
        /// Save a snapshot of the port? Can remove if it gets excessive
        /// </summary>
        public Port Port { get; set; }


        public PortTradeDirection TransactionDirection { get; set; }

        public float CashPaid { get; set; }

        /// <summary>
        /// Too lazy to find a different place for this class to enable a reference to CargoTransaction
        /// </summary>
        public List<object> AllCargoTransactions { get; set; }       

        private PortShipTradeRecordModel(){}

        public PortShipTradeRecordModel(DateTime dateTime, int slaveID):base(dateTime, slaveID)
        {

        }
    }

}
