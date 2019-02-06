using Freecon.Core.Interfaces;
using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Server.Managers;
using Server.Models;
using Server.Models.Interfaces;
using Server.Models.Structures;
using System;
using Server.Models.Extensions;
using Freecon.Core.Utils;
using Core.Models.Enums;

namespace SRServer
{
    /// <summary>
    /// Just a simple class to clean up verbose validation that needs to happen when certain messages are processed
    /// </summary>
    partial class Server
    {

        bool PlanetTurretPlacementRequest(IArea currentArea, Player player, int turretID, ref float xPos, ref float yPos, out string resultMessage)
        {
            if(currentArea.AreaType != AreaTypes.Planet)
            {
                resultMessage = "Not on a planet.";//Client shouldn't ever see this...
                return false;
            }

            Planet planet = ((Planet)(currentArea));

            if (planet.IsColonized && planet.GetOwner() != player && !player.GetTeamIDs().Overlaps(planet.GetOwner().GetTeamIDs()))
            {
                resultMessage = "Cannot place turrets on unallied planets.";
                return false;
            }
            else if (!currentArea.CanAddStructure(player, StructureTypes.LaserTurret, xPos, yPos, out resultMessage))
            {
                return false;
            }
            else if (player.GetActiveShip().Cargo.IsCargoInHolds(turretID) != StatefulCargoTypes.LaserTurret)
            {
                resultMessage = "Turret not found in ship cargo.";
                return false;
            }
            else if(!(planet.GetValidStructurePosition(StructureStatManager.GetStats(StructureTypes.LaserTurret), ref xPos, ref yPos)))
            {
                resultMessage = "Invalid structure position.";
                return false;
            }


            resultMessage = "Success";
            return true;

        }


        bool SystemTurretPlacementRequest(IArea currentArea, Player player, int turretID, ref float xPos, ref float yPos, out string resultMessage)
        {

            if (currentArea.AreaType != AreaTypes.System)
            {
                resultMessage = "Not in a system.";//Client shouldn't ever see this...
                return false;
            }

            PSystem system = ((PSystem)(currentArea));

            
            if (!currentArea.CanAddStructure(player, StructureTypes.LaserTurret, xPos, yPos, out resultMessage))
            {
                return false;
            }
            else if (player.GetActiveShip().Cargo.IsCargoInHolds(turretID) != StatefulCargoTypes.LaserTurret)
            {
                resultMessage = "Turret not found in ship cargo.";
                return false;
            }
            else if (!system.GetValidStructurePosition(StructureStatManager.GetStats(StructureTypes.LaserTurret), ref xPos, ref yPos))
            {
                resultMessage = "Invalid structure position.";
                return false;
            }


            resultMessage = "Success";
            return true;
        
        }

        bool MinePlacementRequest(IArea currentArea, Player player, int mineID, ref float xPos, ref float yPos, out string resultMessage)
        {

            if (currentArea.AreaType != AreaTypes.System && currentArea.AreaType != AreaTypes.Planet)
            {
                resultMessage = "Not in area where mines can be placed.";//Client shouldn't ever see this...
                return false;
            }


            if (!currentArea.CanAddStructure(player, StructureTypes.Mine, xPos, yPos, out resultMessage))
            {                
                return false;
            }
            else if (player.GetActiveShip().Cargo.IsCargoInHolds(mineID) != StatefulCargoTypes.DefensiveMine)
            {
                resultMessage = "Mine not found in ship cargo.";
                return false;
            }
            else if (!currentArea.GetValidStructurePosition(StructureStatManager.GetStats(StructureTypes.Mine), ref xPos, ref yPos))
            {
                resultMessage = "Invalid structure position.";
                return false;
            }


            resultMessage = "Success";
            return true;

        }

        /// <summary>
        /// Does NOT check cargo space!
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="structure"></param>
        /// <returns></returns>
        bool CanPickup(IShip ship, IStructure structure)
        {

            if (ship == null)
                return false;
            if (structure == null)
                return false;
            if (!CheckDistance(ship, structure, 5))
                return false;
            if (ship.CurrentAreaId != structure.CurrentAreaId)
                return false;
            if (!(structure.StructureType == StructureTypes.LaserTurret || structure.StructureType == StructureTypes.DefensiveMine))
                return false;
            if (!_teamManager.AreAllied(ship, (ITeamable)structure))
                return false;


            return true;
        }


        /// <summary>
        /// Does NOT check cargo space!
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool CanPickup(IShip ship, IFloatyAreaObject obj)
        {
            if (ship == null)
                return false;
            if (obj == null)
                return false;
            if (!CheckDistance(ship, obj, 5))
                return false;


            return true;
        }


        bool CheckDistance(IHasPosition obj1, IHasPosition obj2, float tolerance)
        {
            return Math.Sqrt(Math.Pow(obj1.PosX - obj2.PosX, 2) + Math.Pow(obj1.PosY - obj2.PosY, 2)) <= tolerance;
        }

        WarpAttemptResult CanWarp(IShip warpingShip, Player warpingPlayer, IArea currentArea, int destinationAreaID, int warpholeIndex, out string denialString)
        {

            if(currentArea == null)
            {
                denialString = "";
                return WarpAttemptResult.CurrentAreaNotFound;
            }
                 
            if(currentArea.Warpholes[warpholeIndex].DestinationAreaID != destinationAreaID)
            {
                denialString = "";
                return WarpAttemptResult.DestinationAreaIDMismatch;//Packet corruption or hacking attempt? Also potential multiple warp requests recieved and processed for the same warphole
            }


            if (TimeKeeper.MsSinceInitialization - warpingShip.LastWarpTime < ServerConfig.MinWarpPeriod)
            {
                denialString = "";//Probably better to fail silently
                return WarpAttemptResult.StillInWarpCooldown;
            }

            if (!currentArea.IsAreaConnected(destinationAreaID))//Fixes a bug where a ship might send a second warp request before the first is complete, since we send warp indices instead of destination areas from the client
            {
                denialString = "";//Clients will only get here if they are hacking or if there's a weird latency-caused pseudo-bug. Probably better to fail silently.
                return WarpAttemptResult.NotConnectedArea;
            }

            if(warpingShip.CurrentEnergy != warpingShip.MaxEnergy)//Do we want to allow [some] ships to warp without full energy?
            {
                denialString = "Warp failed: not enough energy.";
                return WarpAttemptResult.NotEnoughEnergy; 
            }

            if(warpingShip.DistanceTo(currentArea.Warpholes[warpholeIndex]) > 2)
            {
                denialString = "";//Probably better to fail silently
                return WarpAttemptResult.NotNearWarp;
            }

            if(warpingPlayer.IsTrading)
            {
                denialString = "Can't warp while trading!";
                return WarpAttemptResult.Trading;
            }

            


            denialString = "";
            return WarpAttemptResult.Success;

        }
    }

    
}
