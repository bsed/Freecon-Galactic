using System.Collections.Generic;
using Core.Models;
using FarseerPhysics.Dynamics;
using Freecon.Client.Managers;
using Freecon.Client.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Core.Objects.Ships
{
    public class SuperCoolAwesome3DShip : Ship3D
    {
        public SuperCoolAwesome3DShip(SpriteBatch spriteBatch, Model drawModel, Vector2 position, Vector2 velocity, float rotation, int shipID, int playerID, string playerName, ShipStats shipStats, ParticleManager particleManager, World world, HashSet<int> teams) : base(drawModel, spriteBatch, position, velocity, rotation, shipID, playerID, playerName, shipStats, particleManager, world, teams)
        {
            AssignBody(position, velocity, rotation, world, 100, BodyShapes.Oval, 20);

            engineOffset = 100 / 2.8f;

            DrawData.Scale = .3f;

        }
    }
}
