using Lidgren.Network;
using Freecon.Core.Networking.Models;
using Server.Models;
using System.Collections.Generic;
using Core.Models.Enums;
using Server.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;
using Core.Models;
using Server.Models.Interfaces;

namespace Server.Managers
{
    public class MessageManager : IMessageManager
    {

        ConnectionManager _connectionManager;

        public NetPeer Server
        {
            get { return _connectionManager.Server; }
        }

        public MessageManager(ConnectionManager cm)
        {
            _connectionManager = cm;
        }

        

        /// <summary>
        /// Sends a revive Command to the player owning IShip s
        /// </summary>
        /// <param name="s"></param>
        public void SendReviveMessage(IShip s, int health, int shields)
        {
            Player p = s.GetPlayer();

            var data = new MessageRemoveKillRevive();
            data.ActionType = ActionType.Revive;
            data.ObjectType = RemovableObjectTypes.Ship;
            data.ObjectIDs.Add(s.Id);
            p.SendMessage(new NetworkMessageContainer(data, MessageTypes.RemoveKillRevive));
        }
    
        /// <summary>
        /// Set debuffsToAdd to null if there are none to add
        /// </summary>
        /// <param name="s"></param>
        /// <param name="debuffsToAdd">Debuffs added since the last damage was sent</param>
        public void SendShipDamage(IShip s, Dictionary<DebuffTypes, int> debuffsToAdd = null)
        {
            MessageSetHealth data = new MessageSetHealth();
            HealthData hd = new HealthData() { ShipID = s.Id, Health = (int)s.CurrentHealth, Shields = (int)s.Shields.CurrentShields, Energy = (int)s.CurrentEnergy };
 
            if(debuffsToAdd != null)
            {
                foreach(var kvp in debuffsToAdd)
                {
                    hd.DebuffTypesToAdd.Add(kvp.Key);
                    hd.DebuffCountsToAdd.Add(kvp.Value);
                }
            }
            data.HealthData.Add(hd);

            s.GetPlayer().SendMessage(new NetworkMessageContainer(data, MessageTypes.SetHealth));
        }

        /// <summary>
        /// Notifies a player, if online, of a module added to his ship
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="transaction"></param>
        public void NotifyCargoAdded(object sender, ITransactionAddStatefulCargo transaction)
        {
            if(transaction.CargoHolder is IShip)
            {
                IShip s = transaction.CargoHolder as IShip;
                Player p = s.GetPlayer();
                if(p.IsOnline)
                {
                    StatefulCargo m = transaction.CargoObject;
                    MessageAddCargoToShip data = new MessageAddCargoToShip();
                    data.ShipID = s.Id;
                    data.StatefulCargoData.Add(m.GetNetworkObject());

                    p.SendMessage(new NetworkMessageContainer(data, MessageTypes.AddCargoToShip));   
                }
            }
            
        }

        public void NotifyCargoAdded(object sender, ITransactionAddStatelessCargo transaction)
        {
            if (transaction.CargoHolder is IShip)
            {
                IShip s = transaction.CargoHolder as IShip;
                Player p = s.GetPlayer();
                if (p.IsOnline)
                {
                    var data = new MessageAddCargoToShip();
                    data.ShipID = s.Id;
                    data.StatelessCargoData.Add(new StatelessCargoData { CargoType = transaction.CargoType, Quantity = transaction.Quantity });

                    p.SendMessage(new NetworkMessageContainer(data, MessageTypes.AddCargoToShip));
                }
            }
        }

        public void NotifyCargoRemoved(object sender, ITransactionRemoveStatefulCargo transaction)
        {
            if (transaction.CargoHolder is IShip)
            {
                IShip s = transaction.CargoHolder as IShip;
                Player p = s.GetPlayer();
                if (p.IsOnline)
                {
                    var data = new MessageRemoveCargoFromShip{ ShipID = s.Id };
                    data.StatefulCargoIDs.Add((int)transaction.CargoID);
                    p.SendMessage(new NetworkMessageContainer(data, MessageTypes.RemoveCargoFromShip)); 
                }
            }

        }

        public void NotifyCargoRemoved(object sender, ITransactionRemoveStatelessCargo transaction)
        {
            if (transaction.CargoHolder is IShip)
            {
                IShip s = transaction.CargoHolder as IShip;
                Player p = s.GetPlayer();
                if (p.IsOnline)
                {
                    var data = new MessageAddCargoToShip();
                    data.ShipID = s.Id;
                    data.StatelessCargoData.Add(new StatelessCargoData { CargoType = transaction.CargoType, Quantity = transaction.Quantity });

                    p.SendMessage(new NetworkMessageContainer(data, MessageTypes.RemoveCargoFromShip));
                }
            }

        }
    }
}