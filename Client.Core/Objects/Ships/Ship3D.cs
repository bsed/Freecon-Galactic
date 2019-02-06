using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using Core.Models;
using System.Collections.Generic;
using Freecon.Client.Core;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Core.Objects;

namespace Freecon.Client.Objects
{
    /// <summary>
    /// A ship which draws a 3D model
    /// </summary>
    public abstract class Ship3D : Ship, IDraw3D
    {
        public DrawData3D DrawData { get; protected set; }

        public Model DrawModel { get; protected set; }

        //TODO:Remove spritebatch, it's for debug only
        public Ship3D(Model drawModel, SpriteBatch spriteBatch,
            Vector2 position,
                     Vector2 velocity,
                     float rotation,
                     int shipID,
                     int playerID,
                     string playerName,
                     ShipStats shipStats,
                     ParticleManager particleManager,
                     World world,
                     HashSet<int> teams)
            : base(shipID, playerID, playerName, shipStats,
                   particleManager, spriteBatch, teams)
        {
            DrawModel = drawModel;
            DrawData = new DrawData3D();
        }
        
        public override void Draw(Camera2D camera)
        {
            _spriteBatch.Draw(TextureManager.greenPoint, Position, Color.White);

            DrawHelper3D.Draw3D(this, camera);             
          
        }
    }

    
}
