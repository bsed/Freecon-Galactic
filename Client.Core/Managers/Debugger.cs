using Freecon.Client.GUI;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Mathematics;
using FarseerPhysics.Dynamics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Freecon.Core;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Server.Managers;

namespace Freecon.Client.Managers
{

    /// <summary>
    /// Let's try to remember to remove this before release
    /// </summary>
    public class Debugging
    {
        public static SimulationManager SimulationManager;
        public static TextDrawingService textDrawingService;
        public static PhysicsManager PhysicsManager;
        public static ClientManager ClientManager;
        public static ClientShipManager ClientShipManager;

        public static bool DisableNetworking = false;
        public static Body SunBody;

        //public static float AspectRatio;
        //public static float CameraZ = 5000;
        //public static Camera2D Camera;

        public static bool admin = true;
        public static bool Autologin = true;
        public static string AutoLoginName = "freeqaz";
        //public static string AutoLoginName = "ALLYOURBASE";
        //string AutoLoginName = "tuck";
        public static string AutoLoginPassword = "verystronguncrackablepassword666";

        public static bool IsBot = false;

        public static PlayerShipManager playerShipManager;

        public static bool WriteLayoutEdgeErrors = false;

        static int _lastID = 0;
        
        public static CyclicalIterator<ProjectileTypes> MissileTypes;

        /// <summary>
        /// Keeps track of disposed bodies
        /// </summary>
        public static ConcurrentStack<string> DisposeStack = new ConcurrentStack<string>();
        
        /// <summary>
        /// Keeps track of added bodies
        /// </summary>
        public static ConcurrentStack<string> AddStack = new ConcurrentStack<string>();

        static Debugging()
        {
            List<ProjectileTypes> pt = new List<ProjectileTypes> {ProjectileTypes.HellHoundMissile, ProjectileTypes.MissileType1, ProjectileTypes.MissileType2, ProjectileTypes.MissileType3, ProjectileTypes.MissileType4, ProjectileTypes.AmbassadorMissile };
            MissileTypes = new CyclicalIterator<ProjectileTypes>(pt);
        }

        public static int GetID()
        {            
            return _lastID++;
        }

#if ADMIN
        public static void Update()
        {
            if (KeyboardManager.ChangeShip.IsBindTapped())
            {
                ConsoleManager.WriteLine("Changeship keybind disabled.", ConsoleMessageType.Notification);
                
            }
            if (KeyboardManager.SetPosTo0.IsBindTapped())
            {
                ClientShipManager.PlayerShip.Position = new Microsoft.Xna.Framework.Vector2(5, 20);

            }

            //textDrawingService.DrawTextToScreenRight(35, "Camera Z: " + CameraZ.ToString());
        }

#endif
    }
}
