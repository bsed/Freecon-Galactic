
using System;

namespace Freecon.Client.Managers
{
    /// <summary>
    /// Class which holds player state (Id, and anything else we decide we need later)
    /// </summary>
    public interface IClientPlayerInfoManager
    {
        int? PlayerID { get; set; }

        /// <summary>
        /// The ship the client is currently controlling
        /// </summary>
        int? ActiveShipID { get; set; }//Leaving the option for multiple ships

    }




    public abstract class ClientPlayerInfoManager:IClientPlayerInfoManager
    {
        public int? PlayerID { get; set; }

        public abstract int? ActiveShipID { get; set; }
    }



    /// <summary>
    /// Used in managers which have references to PlayerShipManager, keeps PlayerShip.Id and ActiveShipID in sync.
    /// Setting ActiveShipID fails with an exception. It's ugly, but it shouldn't happen.
    /// </summary>
    public class PlayablePlayerInfoManager : ClientPlayerInfoManager
    {
        readonly PlayerShipManager _playerShipManager;

        public override int? ActiveShipID { get { return _playerShipManager.PlayerShip.Id; } set { throw new InvalidOperationException("Cannot set the ActiveShipID for a " + this.ToString() + "; must recreate player ship instead."); } }

        /// <summary>
        /// Class used in managers which have references to PlayerShipManager, keeps PlayerShip.Id and ActiveShipID in sync.
        /// Setting ActiveShipID fails with an exception. It's ugly, but it shouldn't happen.
        /// </summary>
        public PlayablePlayerInfoManager(PlayerShipManager playerShipManager)
        {
            _playerShipManager = playerShipManager;
        }

    }

    /// <summary>
    /// Used in managers which don't have references to PlayerShipManager, such as port and colony, where ActiveShipID must be set manually
    /// </summary>
    public class NonPlayablePlayerInfoManager : ClientPlayerInfoManager
    {
        public override int? ActiveShipID { get; set; }
    }
}