using System;
using System.Linq;
using Server.Interfaces;
using Server.Managers;
using Server.Models;

namespace DBFiller
{
    internal class Helpers
    {
        /// <summary>
        /// Safely moves the player and the specified ship to the specified area
        /// </summary>
        /// <param name="destinationArea"></param>
        /// <param name="player"></param>
        public static void DebugWarp(IArea destinationArea, Player player, IShip playerShip)
        {
            ConsoleManager.WriteLine("Warping player " + player.Username + " to " + destinationArea.AreaName, ConsoleMessageType.Debug);
            destinationArea.MovePlayerHere(player, false);
            try
            {
                playerShip.GetPlayer().SetArea(destinationArea);
            }
            catch (Exception e)
            {
                Console.WriteLine("fase");

            }
            destinationArea.MoveShipHere(playerShip);
            playerShip.SetArea(destinationArea);
            }

        /// <summary>
        /// Reflects on all public methos in the given StateLoader, registers them with dbf
        /// </summary>
        /// <param name="dbf"></param>
        /// <param name="sl"></param>
        public static void RegisterStateLoader(DBFiller dbf, StateLoader sl)
        {
            var methods = sl.GetType().GetMethods();

            var alphabetic = methods.Where(m=>m.Name.Contains('_')).ToList();
            alphabetic.Sort((t1, t2)=> { return t1.Name.CompareTo(t2.Name); });

            var eventInfo = dbf.GetType().GetEvent("OnCreationLoadState");

            foreach (var m in alphabetic)
            {
                var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, sl, m);
                eventInfo.AddEventHandler(dbf, d);
            }


        }

    }
}