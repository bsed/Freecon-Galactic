using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using FarseerPhysics.Dynamics;
using Core.Models;
using Freecon.Client.Mathematics;

namespace Freecon.Client.Objects
{
    //WARNING: body weight is set to baseWeight before baseWeight is given a ship specific value

    public class Penguin : Ship
    {
        public Penguin(Vector2 position,
                       Vector2 velocity,
                       float rotation,
                       int shipID,
                       int playerID,
                       string playerName,
                       ShipStats shipStats,
                       ParticleManager particleManager,
                       World world,
                       SpriteBatch spriteBatch,
                       Texture2D drawTex, HashSet<int> teams)
            : base(shipID, playerID, playerName, shipStats, 
                   particleManager, spriteBatch, teams)
        {
            currentDrawTex = drawTex;
            Texture = drawTex;
            engineOffset = 54 / 2.3f;
            shipName = "Penguin";

            AssignBody(position, velocity, rotation, world, 54 / 2f);

        }
    }

    public class Barge : Ship
    {
        public Barge(Vector2 position,
                     Vector2 velocity,
                     float rotation,
                     int shipID,
                     int playerID,
                     string playerName,
                     ShipStats shipStats,                       
                     ParticleManager particleManager,
                     World world,
                     SpriteBatch spriteBatch,
                     Texture2D drawTex,
                     HashSet<int> teams)
            : base(shipID, playerID, playerName, shipStats,
                   particleManager, spriteBatch, teams)
        {
            currentDrawTex = drawTex;
            Texture = drawTex;
            engineOffset = 180 / 2.9f;

            shipName = "Barge";
            
            AssignBody(position, velocity, rotation, world, 180 / 2f, BodyShapes.Oval, 180/2f);


        }

        
    }

    public class Battlecruiser : Ship
    {
        private readonly Rectangle[] rectArray = new Rectangle[32]; //For drawing the special battlecruiser sprite
        float _drawScale;

        public Battlecruiser(Vector2 position, 
                             Vector2 velocity, 
                             float rotation,
                             int shipID,
                             int playerID,
                             string playerName,
                             ShipStats shipStats,
                             ParticleManager particleManager,
                             World world,
                             SpriteBatch spriteBatch,
                             Texture2D drawTex, HashSet<int> teams)
            : base(shipID, playerID, playerName, shipStats,
                   particleManager,spriteBatch, teams)
        {
            _drawScale = 2;
            currentDrawTex = drawTex;
            Texture = drawTex;
            shipName = "Battlecruiser";

           

            //Fill texArray
            int index = 0;
            for (int i = 0; i < 4; i++) //Rows
            {
                for (int j = 0; j < 8; j++) //Column
                {
                    rectArray[index] = new Rectangle(j * 108, i * 86, 108, 86);
                    index++;
                }
            }

            AssignBody(position, velocity, rotation, world, rectArray[0].Width * _drawScale / 2, BodyShapes.Oval, rectArray[0].Height * _drawScale / 2);

            engineOffset = rectArray[0].Width / 2.8f * _drawScale;
        }

        public override void Draw(Camera2D camera)
        {
            var tempVec = new Vector2(54, 43);
            float tempRot = Body.Rotation;

            // Get appropriate 32nd of rotation

            // Make sure angle is between -360 and 360
            tempRot = tempRot % (float)(2 * Math.PI);
            var rectArrayIndex = (int)(tempRot / .1963495);

            if (rectArrayIndex < 0)
            {
                rectArrayIndex += 32;
            }
            if (rectArrayIndex > 31)
            {
                rectArrayIndex = 31;
            }


            _spriteBatch.Draw(currentDrawTex, ConvertUnits.ToDisplayUnits(Body.Position), rectArray[rectArrayIndex],
                             drawColor, 0, tempVec, _drawScale, SpriteEffects.None, .2f);
                     
        }
        
     
    }

    public class Reaper : Ship
    {
        public Reaper(
            Vector2 position,
            Vector2 velocity,
            float rotation,
            int shipID,
            int playerID,
            string playerName,
            ShipStats shipStats,
            ParticleManager particleManager,
            World world,
            SpriteBatch spriteBatch,
            Texture2D drawTex, HashSet<int> teams
            ) : base(shipID, playerID, playerName, shipStats,
                     particleManager, spriteBatch, teams)
        {
            currentDrawTex = drawTex;
            Texture = drawTex;
            engineOffset = 68 / 2.2f;

            shipName = "Reaper";

            ShipStats.EnergyRegenRate = .45f; // In energy/millisecond
            ShipStats.Energy = 1000;
            ShipStats.MaxHealth = 5000;
            ShipStats.MaxShields = 600;
            ShipStats.TopSpeed = ConvertUnits.ToSimUnits(350);
            ShipStats.BaseThrustForward = 90f;
            ShipStats.TurnRate = 6.2831f;
            BodyWidth = 108 / 2f;
            BodyHeight = 86 / 2f;

            AssignBody(position, velocity, rotation, world, 68 / 2.05f);
        }
    }

    public class Dread : Ship
    {
        public Dread(Vector2 position, 
                     Vector2 velocity,
                     float rotation,
                     int shipID,
                     int playerID,
                     string playerName,
                     ShipStats shipStats,
                     ParticleManager particleManager,
                     World world,
                     SpriteBatch spriteBatch,
                     Texture2D drawTex, HashSet<int> teams)
            : base(shipID, playerID, playerName, shipStats,
                   particleManager, spriteBatch, teams)
        {
            currentDrawTex = drawTex;
            Texture = drawTex;
            engineOffset = 80 / 2.2f;

            shipName = "Dread";

            #region Base Stats

            ShipStats.EnergyRegenRate = .3f; //In energy/millisecond
            ShipStats.Energy = 1000;
            ShipStats.MaxHealth = 10000;
            ShipStats.MaxShields = 1000;
            ShipStats.TopSpeed = ConvertUnits.ToSimUnits(180);
            ShipStats.BaseThrustForward = 80f;
            ShipStats.TurnRate = 2.2f;

            #endregion

            AssignBody(position, velocity, rotation, world, 80 / 2.8f);

 
            
        }
    }

    /// <summary>
    /// Used for on-the-fly ship stat customization.
    /// </summary>
    public class StatHoldingShip
    {
        public ShieldTypes ShieldType = ShieldTypes.QuickRegen;
        public ShipTypes ShipType;
        public string Name { get; set; }
        public string Description { get; set; }
        public bool SyncedWithClient { get; set; }

        public int Price { get; set; }
        public int Shields { get; set; }
        public int Hull { get; set; }
        public int Energy { get; set; }
        public int Cargo { get; set; }
        public int Value { get; set; }
        public int TopSpeed { get; set; }
        public int Acceleration { get; set; }

        // strings
        public string Graphic { get; set; }
        public string ThrustGraphic { get; set; }
        public string Class { get; set; }

        // floats
        public float TurnRate { get; set; }
        public float regenRate { get; set; }
    }
}
