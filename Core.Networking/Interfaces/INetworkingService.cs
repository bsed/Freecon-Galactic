using Core.Networking;
using Core.Networking.Interfaces;
using Freecon.Core.Networking.Models;
using System;

namespace Freecon.Core.Networking.Interfaces
{
    public interface INetworkingService
    {
        /// <summary>
        /// ManagerID must be unique
        /// </summary>
        /// <param name="handlerID">Unique ID for each handler. Unused with lidgren</param>
        /// <param name="handler"></param>
        void RegisterMessageHandler(INetworkMessageHandler handlerObject, EventHandler<NetworkMessageContainer> handlerMethod);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerID"></param>
        /// <param name="handler"></param>
        void DeregisterMessageHandler(INetworkMessageHandler handleObject, EventHandler<NetworkMessageContainer> handlerMethod);

        /// <summary>
        /// Removes all event handles associated with the given handlerObject
        /// </summary>
        /// <param name="handlerObject"></param>
        void DeregisterAllHandlers(INetworkMessageHandler handlerObject);
        
        /// <summary>
        /// Synchronously processes all pending messages for the corresponding areaID
        /// </summary>
        /// <param name="areaID"></param>
        void FlushMessages(MessageHandlerID messageHandlerID);
      

    }
}
