namespace Freecon.Client.Objects
{
    //class CustomShip : Ship
    //{
    //    /// <summary>
    //    /// Takes a mapped Enum value and returns a Texture2D. Used for pulling textures from the server.
    //    /// Future version feature: Implement ability to pull from HTTP
    //    /// </summary>
    //    Dictionary<ShipTextures, Texture2D> EnumToTexture = new Dictionary<ShipTextures, Texture2D>{
    //        { ShipTextures.Battlecruiser, TextureManager.Battlecruiser },
    //        { ShipTextures.Penguin, TextureManager.Penguin },
    //        { ShipTextures.ZYVariantBarge, TextureManager.ZYVariantBarge },
    //        { ShipTextures.Reaper, TextureManager.Reaper }
    //    };

    //    /// <summary>
    //    /// Creates a custom ship. This is read from the Server's Database. Stats and texture are set.
    //    /// </summary>
    //    /// <param name="position"></param>
    //    /// <param name="velocity"></param>
    //    /// <param name="rotation"></param>
    //    /// <param name="shipID"></param>
    //    /// <param name="playerID"></param>
    //    /// <param name="playerName"></param>
    //    /// <param name="primaryWeaponType"></param>
    //    /// <param name="secondaryWeaponType"></param>
    //    /// <param name="shieldType"></param>
    //    /// <param name="shipStats">Custom object for holding stats.</param>
    //    public CustomShip(Vector2 position, Vector2 velocity, float rotation, UInt32 shipID, UInt32 playerID,
    //                    string playerName,
    //                    WeaponTypes primaryWeaponType, WeaponTypes secondaryWeaponType, ShieldTypes shieldType,
    //                    ClientShipManager.CustomShipStats shipStats)
    //        : base(shipID, playerID, playerName, shieldType, primaryWeaponType, secondaryWeaponType, ShipTypes.player_CustomShip)
    //    {
    //        currentDrawTex = EnumToTexture[shipStats.Graphic];
    //        liveTex = EnumToTexture[shipStats.Graphic];
    //        engineOffset = liveTex.Height / 2.3f;
    //        shipName = shipStats.Name;

    //        #region _body Stuff

    //        body = BodyFactory.CreateCircle(PhysicsManager.world, ConvertUnits.ToSimUnits(currentDrawTex.Width / 2f), 1);
    //        body.BodyType = BodyType.Dynamic;
    //        body.Mass = baseWeight;
    //        body.IsBullet = true;
    //        body.Friction = 0;
    //        body.Restitution = 0.5f;
    //        body.CollisionCategories = Category.Cat10;
    //        //body.OnCollision += new OnCollisionEventHandler(body_OnCollision); // Collisions are delegated elsewhere.
    //        body.LinearDamping = 0.00001f;
    //        body.Position = position;
    //        body.LinearVelocity = velocity;
    //        body.Rotated = rotation;

    //        #endregion

    //        #region Base Stats

    //        baseRegenRate = shipStats.RegenRate; //In energy/millisecond
    //        baseMaxEnergy = shipStats.Energy;
    //        baseMaxHealth = (ushort)shipStats.Hull;
    //        baseMaxShields = (ushort)shipStats.Shields;
    //        baseTopSpeed = ConvertUnits.ToSimUnits(shipStats.TopSpeed);
    //        baseThrust = shipStats.Acceleration;
    //        baseTurnRate = shipStats.TurnRate;
    //        baseTurnMax = 3.8f; // These are moderately arbitrary in the new model
    //        baseTurnLag = 0.2f; // These are moderately arbitrary in the new model

    //        #endregion

    //        createDictionaries((int)(baseMaxEnergy + maxEnergyBonus));
    //        currentHealth = baseMaxHealth + maxHealthBonus;
    //    }
    //}
}
