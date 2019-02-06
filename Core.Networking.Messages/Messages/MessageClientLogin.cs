using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageClientLogin:MessagePackSerializableObject
    {
        public float CurrentCash { get; set; }
        public ShipData Ship { get { return _ship; } set { _shipDataSet = true; _ship = value; } }
        ShipData _ship;
        bool _shipDataSet;

        public string LoginMessage { get { return _loginMessage; } set { _loginMessageSet = true; _loginMessage = value; } }
        string _loginMessage;
        bool _loginMessageSet;

        public string AreaName { get { return _systemName; } set { _systemNameSet = true; _systemName = value; } }
        string _systemName;
        bool _systemNameSet;

        public PlayerInfo PlayerInfo { get { return _playerInfo; } set { _playerInfo = value; _playerInfoSet = true; } }
        PlayerInfo _playerInfo;
        bool _playerInfoSet;

        public override byte[] Serialize()
        {
            if (!_shipDataSet)
                throw new RequiredParameterNotInitialized("Ship", this);

            if (!_loginMessageSet)
                throw new RequiredParameterNotInitialized("LoginMessage", this);

            if (!_systemNameSet)
                throw new RequiredParameterNotInitialized("SystemName", this);

            if (!_playerInfoSet)
                throw new RequiredParameterNotInitialized("PlayerInfo", this);

            return base.Serialize();
        }

    }
}
