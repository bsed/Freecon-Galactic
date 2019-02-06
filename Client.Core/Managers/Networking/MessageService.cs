using Lidgren.Network;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Freecon.Client.Core.Extensions;
using Freecon.Client.Objects;
using Freecon.Core.Networking.Models;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models.Messages;
using RedisWrapper;
using Server.Managers;

namespace Freecon.Client.Managers.Networking
{
    public class MessageService_ToServer
    {
        protected IMessenger _messenger;

        public MessageService_ToServer(IMessenger messenger)
        {
            _messenger = messenger;
        }

        /// <summary>
        /// Sends a position update to the server.
        /// </summary>
        /// <param name="currentAreaId"></param>
        /// <param name="shipsToSend"></param>
        /// <param name="sendingPlayerId">Null if sent from simulator, otherwise must be sent so that a client is not forwarded its own position updates</param>
        public void SendPositionUpdate(IGameTimeService gameTime, int currentAreaId, IEnumerable<Ship> shipsToSend, int? sendingPlayerId = null)
        {
            if (shipsToSend == null || shipsToSend.Count() == 0)
            {
                return;
            }

            var positionUpdateData = new MessagePositionUpdateData(sendingPlayerId, currentAreaId);
            
            //PositionUpdateData playerShipData = new PositionUpdateData(ClientShipManager.PlayerShip.Id, ClientShipManager.PlayerShip.Position.X, ClientShipManager.PlayerShip.Position.Y, ClientShipManager.PlayerShip.Rotated, ClientShipManager.PlayerShip.LinearVelocity.X, ClientShipManager.PlayerShip.LinearVelocity.Y, ClientShipManager.PlayerShip.AngularVelocity, ClientShipManager.PlayerShip.Shields.CurrentShields, ClientShipManager.PlayerShip.CurrentHealth, ClientShipManager.PlayerShip.ThrustStatusForServer);
            //data.UpdateDataObjects.Add(playerShipData);
            //ClientShipManager.PlayerShip.ThrustStatusForServer = false;

            var positionUpdates = shipsToSend.Where(s => s.IsBodyValid && s.SendPositionUpdates).Select(s =>
            {
                s.ThrustStatusForServer = false;

                return s.GetPositionUpdate(gameTime);
            });

            positionUpdateData.UpdateDataObjects.AddRange(positionUpdates);

            _messenger.SendMessageToServer(MessageTypes.PositionUpdateData, positionUpdateData);
        }

        public void SendLandRequest(int landingShipID, int areaID)
        {
            var data = new MessageLandRequest()
            {
                LandingShipID = landingShipID,
                AreaID = areaID
            };

            _messenger.SendMessageToServer(MessageTypes.LandRequest, data);
        }

        public void SendDockRequest(int dockingShipID, int areaID)
        {
            var data = new MessageDockRequest()
            {
                DockingShipID = dockingShipID,
                PortID = areaID
            };

            _messenger.SendMessageToServer(MessageTypes.DockRequest, data);
        }

        /// <summary>
        /// The warpholeIndex on the client is Warphole.Id
        /// </summary>
        /// <param name="warpingShipID"></param>
        /// <param name="warpholeIndex"></param>
        /// <param name="destinationAreaID"></param>
        public void SendWarpRequest(int warpingShipId, int warpholeIndex, int destinationAreaID)
        {
            var requestData = new MessageWarpRequest
            {
                WarpingShipID = warpingShipId,
                DestinationAreaID = destinationAreaID,
                WarpholeIndex = warpholeIndex
            };
            
            _messenger.SendMessageToServer(MessageTypes.WarpRequest, requestData);

        }

        public void SendShipFireRequest(int shipID, byte weaponSlot, List<int> projectileIDs, float rotation, byte pctCharge, ProjectileTypes projectileType)
        {
            var data = new MessageShipFireRequest() { ShipID = shipID, Rotation = rotation, WeaponSlot = weaponSlot, PctCharge = pctCharge, ProjectileIDs = projectileIDs, ProjectileType = projectileType};

            _messenger.SendMessageToServer(MessageTypes.ShipFireRequest, data);
        }

        public void SendStructureFireRequest(int structureID, float rotation, List<int> projectileIDs, byte weaponSlot, byte pctCharge)
        {
            var data = new MessageStructureFireRequest()
            {
                StructureID = structureID,
                WeaponSlot = weaponSlot,
                ProjectileIDs = projectileIDs,
                PctCharge = pctCharge
            };

            _messenger.SendMessageToServer(MessageTypes.StructureFireRequest, data);

        }

        public void ReportCollisions(List<CollisionManager.Collision> collisions)
        {
            if (collisions.Count == 0)
            {
                return;
            }

            var data = new MessageProjectileCollisionReport();

            var collisionData = collisions.Select(c => new CollisionData()
            {
                HitObjectID = c.HitObjectID,
                ProjectileID = c.ProjectileID,
                ProjectileType = c.ProjectileType,
                PctCharge = c.PctCharge,
                WeaponSlot = c.WeaponSlot
            });

            data.Collisions.AddRange(collisionData);

            _messenger.SendMessageToServer(MessageTypes.ProjectileCollisionReport, data);

        }


        public void SendObjectPickupRequest(int requestingShipID, int objectID, PickupableTypes objectType)
        {
            var data = new MessageObjectPickupRequest()
            {
                RequestingShipID = requestingShipID,
                ObjectID = objectID,
                ObjectType = objectType
            };

            _messenger.SendMessageToServer(MessageTypes.ObjectPickupRequest, data);
        }



        public void SendSelectedCommand(MessageSelectorCommand data)
        {
            _messenger.SendMessageToServer(MessageTypes.SelectorMessageType, data);
        }

        public void SendLeaveToPlanetRequest(int requestingShipID)
        {
            _messenger.SendMessageToServer(MessageTypes.LeaveToPlanetRequest, new MessageEmptyMessage{TargetShipID = requestingShipID});
        }

        public void SendLeaveToSpaceRequest(int requestingShipID)
        {
            _messenger.SendMessageToServer(MessageTypes.LeaveToSpaceRequest, new MessageEmptyMessage { TargetShipID = requestingShipID });
        }

        public void SendEnterColonyRequest(int requestingShipID)
        {
            _messenger.SendMessageToServer(MessageTypes.EnterColonyRequest, new MessageEmptyMessage { TargetShipID = requestingShipID });
        }

        public void SendStructurePlacementRequest(StructureTypes structureType, Vector2 position, int requestingShipID)
        {
            var data = new MessageStructurePlacementRequest(requestingShipID, position.X, position.Y, structureType);
            
            _messenger.SendMessageToServer(MessageTypes.StructurePlacementRequest, data);
        }
    }

    /// <summary>
    /// Sends messages to the server
    /// </summary>
    public interface IMessenger
    {
        void SendMessageToServer(NetworkMessageContainer message);
        
        void SendMessageToServer(
            MessageTypes type,
            MessagePackSerializableObject data
        );
    }

    public class LidgrenMessenger : IMessenger
    {
        protected ClientManager _clientManager;

        public LidgrenMessenger(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void SendMessageToServer(NetworkMessageContainer message)
        {
#if ADMIN
            if (Debugging.DisableNetworking)
            {
                return;
            }
#endif

            NetOutgoingMessage msg = _clientManager.Client.CreateMessage();
            message.WriteToLidgrenMessage_ToServer(message.MessageType, msg);
            _clientManager.Client.SendMessage(msg, _clientManager.CurrentSlaveConnection, NetDeliveryMethod.ReliableUnordered);//TODO: Implement GetNetDeliveryMethod method
        }


        public void SendMessageToServer(
            MessageTypes type,
            MessagePackSerializableObject data
        )
        {
            SendMessageToServer(
                new NetworkMessageContainer(data, type)
            );
        }
    }

    /// <summary>
    /// A messenger linked to a particular slave channel through AreaID. Use a different RedisMessenger object for each simulated area, setting AreaID as appropriate.
    /// </summary>
    public class RedisMessenger : IMessenger
    {
        public int AreaID { get; set; }
        RedisServer _redisServer;

        /// <summary>
        /// Use a different RedisMessenger object for each simulated area, setting AreaID as appropriate.
        /// </summary>
        /// <param name="redisServer"></param>
        /// <param name="areaID"></param>
        public RedisMessenger(RedisServer redisServer, int areaID)
        {
            _redisServer = redisServer;
            AreaID = areaID;
        }

        public void SendMessageToServer(NetworkMessageContainer message)
        {

#if ADMIN
            if (Debugging.DisableNetworking)
            {
                return;
            }
#endif

            _redisServer.PublishObject(ChannelTypes.SimulatorToServer_Data, AreaID, message);
        }

        public void SendMessageToServer(
            MessageTypes type,
            MessagePackSerializableObject data
        )
        {
            SendMessageToServer(
                new NetworkMessageContainer(data, type)
            );
        }
    }
}