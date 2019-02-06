using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRServer.Objects;
using SRServer.Managers;

namespace SRServer.Services
{
    public interface IPlayerLocator
    {
        Player GetPlayer(int? playerID);
    }

    public interface IAreaLocator
    {
        Area GetArea(int? areaID);
    }

    public interface IShipLocator
    {
        Ship GetShip(int? shipID);
    }

    public interface IAccountLocator
    {
        Account GetAccount(int? accountID);
    }

    public interface ITeamLocator
    {
        Team GetTeam(int? teamID);
    }

    public class LocatorService
    {
        IPlayerLocator PlayerLocator;
        IAreaLocator AreaLocator;
        IShipLocator ShipLocator;
        IAccountLocator AccountLocator;
        ITeamLocator TeamLocator;
    }
}
