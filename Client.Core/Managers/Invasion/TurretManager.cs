using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;
using Freecon.Client.Objects.Structures;

namespace Freecon.Client.Managers
{
    public class TurretManager
    {
        private ContentManager _content;
        private TextureManager _textureManager;
        private ProjectileManager _projectileManager;
        private ClientShipManager _clientShipManager;

        private readonly List<Turret> _turretList;

        private Random random;

        /// <summary>
        /// Loads textures and parses level for turrets.
        /// </summary>
        /// <param name="Content">Content Manager</param>
        /// <param name="level">Level to get turrets from.</param>
        public TurretManager(ContentManager content, 
                             TextureManager textureManager, 
                             ProjectileManager projectileManager,
                             ClientShipManager clientShipManager)
        {
            _content = content;
            _textureManager = textureManager;
            _projectileManager = projectileManager;
            _clientShipManager = clientShipManager;

            random = new Random(3453);

            // Create Turrets //
            _turretList = new List<Turret>();
            //for (int y = 0; y < level.ySize; y++)
            //    for (int x = 0; x < level.xSize; x++)
            //    {
            //        /*
            //        if (level.PlanetMap[x, y].isTurret)
            //        {
            //            Vector2 temp = new Vector2((turretBase.Width * x) - (turretHead.Width / 2), // Get position for Turret Head
            //                (turretBase.Height * y));
                        
            //            AddTurret(temp); // Adds a turret
            //        }*/
            //    }
        }


        /// <summary>
        /// Adds a turret and iterates the turret count.
        /// </summary>
        /// <param name="Position"></param>
        //public void AddTurret(Vector2 Position)
        //{
        //    var turret = new Turret(_projectileManager, _textureManager, Position, random.Next());
        //    turretList.Add(turret);
        //    turretCount++;
        //}
        //public virtual void Draw(SpriteBatch spriteBatch) 
        //{
        //    for (int i = 0; i < turretList.Count(); i++)
        //    {
        //        turretList[i].Draw(spriteBatch);
        //    }
        //}

        ///// <summary>
        ///// Adds a turret and iterates the turret count.
        ///// </summary>
        ///// <param name="Position"></param>
        //public void AddTurret(Vector2 Position)
        //{
        //    var turret = new Turret(_projectileManager, _textureManager, _clientShipManager,_planetStateManager, _gameStateManager, _spaceStateManager, Position, Enums.StructureTypes.LaserTurret, 100, ID, random.Next());
        //    turretList.Add(turret);
        //    turretCount++;
        //}
    }
}
