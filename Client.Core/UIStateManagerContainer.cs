using System;
using Freecon.Client.Core.States;
using Freecon.Client.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Freecon.Client.View.CefSharp.States
{
    /// <summary>
    /// Readonly container which holds references to stateful (non-singleton) managers within the currently active state which are required for the UI 
    /// </summary>
    public class UIStateManagerContainer
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly GameStateType CurrentAreaType;

        public UIStateManagerContainer(GameStateType currentAreaType)
        {
            CurrentAreaType = currentAreaType;
        }
    }
  
    public class PlayableUIStateManagerContainer : UIStateManagerContainer
    {
        [JsonIgnore]
        public readonly PlayerShipManager PlayerShipManager;

        [JsonIgnore]
        public readonly IClientPlayerInfoManager ClientPlayerInfoManager;

        public PlayableUIStateManagerContainer(GameStateType currentAreaType, PlayerShipManager playerShipManager, IClientPlayerInfoManager clientPlayerInfoManager)
            : base(currentAreaType)
        {
            PlayerShipManager = playerShipManager;
            ClientPlayerInfoManager = clientPlayerInfoManager;
        }
    }
}
