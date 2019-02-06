using Freecon.Client.Managers;
using Core.Interfaces;

namespace Freecon.Client.Objects.Pilots
{
    public class NetworkPilot : Pilot
    {
        public NetworkPilot(Ship ship, CollisionDataObject userdata)
        {
            PilotType = PilotType.NetworkPlayer;
            Ship = ship;
            ship.SetPilotData(userdata, true);
        }

        public override void Update(IGameTimeService gameTime)
        {
        }
    }
}