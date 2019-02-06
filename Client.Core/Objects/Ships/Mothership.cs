//using System.Collections.Generic;

//using FarseerPhysics.Common;
//using FarseerPhysics;
//using FarseerPhysics.Dynamics;
//using FarseerPhysics.Dynamics.Contacts;
//using Microsoft.Xna.Framework; using Core.Interfaces;
//using Microsoft.Xna.Framework.Graphics;

//namespace Client.Objects.Ships
//{
//    public class Mothership : Ship
//    {
//        private List<_body> bodies;
//        //public LaserWave bowLaser;
//        //public LaserWave sternLaser;

        

//        public Vector2 bowLaserPosition;

//        private List<Vertices> decomposedVertices;


        



//        #region Points

//        public static Vector2[] staticPoints = new[]
//                                                   {
//                                                       ConvertUnits.ToSimUnits(new Vector2(1062, 414)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1063, 399)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1097, 396)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1097, 380)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1106, 380)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1104, 205)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1089, 205)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1085, 187)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1070, 187)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(1069, 172)),
//                                                       //new Vector2(993.9999f, 172),
//                                                       ConvertUnits.ToSimUnits(new Vector2(76, 172)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(76, 190)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(57, 190)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(54, 208)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(44, 208)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(44, 235)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(2, 236)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(2, 354)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(44, 355)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(45, 399)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(88, 400)),
//                                                       ConvertUnits.ToSimUnits(new Vector2(88, 416)),
//                                                   };

//        #endregion

//        //public Mothership(UInt32 shipID, Vector2 position)
//        //    : base(shipID, 0, "", 0, WeaponTypes.none, WeaponTypes.none, ShipTypes.NPC_MotherShip)
//        //{
//        //    currentDrawTex = TextureManager.Mothership;
//        //    liveTex = TextureManager.Mothership;

//        //    #region Initializing bodies list

//        //    /*
//        //    //Load the passed texture.
//        //    Texture2D polygonTexture = TextureManager.mothershipOutline;

//        //    //Use an array to hold the textures data.
//        //    uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

//        //    //Transfer the texture data into the array.
//        //    polygonTexture.GetData(data);

//        //    //Find the verticals that make up the outline of the passed texture shape.
//        //    Vertices vertices = PolygonTools.CreatePolygon(data, polygonTexture.Width);
            

//        //    //For now we need to scale the vertices (result is in pixels, we use meters)
//        //    Vector2 scale = new Vector2(0.01f, 0.01f);
//        //    vertices.Scale(ref scale);

//        //    //Partition the concave polygon into a convex one.
//        //    decomposedVertices = EarclipDecomposer.ConvexPartition(vertices);


//        //    foreach (Vertices v in decomposedVertices)
//        //        foreach (Vector2 vertt in v)
//        //        {
//        //            Logger.log(Log_Type.INFO, "" + ConvertUnits.ToDisplayUnits(vertt));
//        //        }
                
//        //    */
//        //    Vector2 pos = position;
//        //    pos.X -= ConvertUnits.ToSimUnits(currentDrawTex.Width/2f); //(float)(8.95 / 2.0);
//        //    pos.Y -= ConvertUnits.ToSimUnits(currentDrawTex.Height/2f); //(float)(4.44 / 2);
//        //    //Create a single body, that has multiple fixtures to the polygon shapes.

//        //    body = BodyFactory.CreateLoopShape(PhysicsManager.World, new Vertices(staticPoints), 1f);
//        //        //BodyFactory.CreateCompoundPolygon(PhysicsManager.world, decomposedVertices, 1f, pos);

//        //    #endregion

//        //    #region _body Stats

//        //    //body = BodyFactory.CreateCircle(PhysicsManager.world, ConvertUnits.ToSimUnits(Tex.Width / 2.5f), 1);
//        //    body.BodyType = BodyType.Static;
//        //    body.Position = pos;
//        //    body.Mass = baseWeight;
//        //    body.IsBullet = true;
//        //    body.Friction = 0;
//        //    body.Restitution = 0.5f;
//        //    body.CollisionCategories = Category.Cat10;
//        //    body.OnCollision += body_OnCollision;
//        //    body.LinearDamping = 0.00001f;

//        //    body.UserData = new BodyDataObject(BodyTypes.Mothership, shipID);

//        //    #endregion

//        //    baseMaxHealth = 65000;
//        //        //check projectiles to make sure they will collide if mothership health = 0 before setting to 0

//        //    createDictionaries((int) (baseMaxEnergy + maxEnergyBonus));
//        //    currentHealth = baseMaxHealth + maxHealthBonus;

//        //    #region Weapons

//        //    sternLaser = new MSLaserWave(this, -3, 0);
//        //    bowLaser = new MSLaserWave(this, 3, 0);

//        //    sternLaserPosition.X = (float) ((body.Position.X) + Math.Cos(body.Rotated)*sternLaser.xOffset);
//        //    sternLaserPosition.Y = (float) ((body.Position.Y) + Math.Cos(body.Rotated)*sternLaser.yOffset);

//        //    bowLaserPosition.X = (float) ((body.Position.X) + Math.Cos(body.Rotated)*bowLaser.xOffset);
//        //    bowLaserPosition.Y = (float) ((body.Position.Y) + Math.Cos(body.Rotated)*bowLaser.yOffset);

//        //    #endregion

//        //    if (MSBManager.currentMSBState != 255) //If in MSBattle, just in case we use motherships outside of MSBattle
//        //        MSBManager.motherShips.Add(this);
//        //}

//        public override void Draw(SpriteBatch spriteBatch)
//        {
//            //#region Ship Texture

//            //spriteBatch.Draw(currentDrawTex, ConvertUnits.ToDisplayUnits(body.Position),
//            //                 new Rectangle(0, 0, currentDrawTex.Width, currentDrawTex.Height),
//            //                 Color.White, body.Rotated,
//            //                 new Vector2(0, 0),
//            //                 1, SpriteEffects.None, 0.2f);

//            ////foreach (Vertices vert in decomposedVertices)
//            //{
//            //    foreach (Vector2 vertt in staticPoints) //vert)
//            //    {
//            //        spriteBatch.Draw(TextureManager.testPoint,
//            //                         ConvertUnits.ToDisplayUnits(vertt) + ConvertUnits.ToDisplayUnits(body.Position),
//            //                         new Rectangle(0, 0, TextureManager.testPoint.Width, TextureManager.testPoint.Height),
//            //                         Color.White, body.Rotated,
//            //                         new Vector2(0, 0),
//            //                         20, SpriteEffects.None, 0.2f);
//            //    }
//            //}

//            //#endregion

//            //#region Laser Turrets

//            ////Draw Stern laser turret
//            //spriteBatch.Draw(TextureManager.turretHead, ConvertUnits.ToDisplayUnits(sternLaserPosition),
//            //                 new Rectangle(0, 0, TextureManager.turretHead.Width, TextureManager.turretHead.Height),
//            //                 Color.White, sternLaser.rotation,
//            //                 new Vector2(TextureManager.turretHead.Width/2, TextureManager.turretHead.Height/2),
//            //                 1, SpriteEffects.None, 0.1f);

//            ////Draw Bow laser turret
//            //spriteBatch.Draw(TextureManager.turretHead, ConvertUnits.ToDisplayUnits(bowLaserPosition),
//            //                 new Rectangle(0, 0, TextureManager.turretHead.Width, TextureManager.turretHead.Height),
//            //                 Color.White, bowLaser.rotation,
//            //                 new Vector2(TextureManager.turretHead.Width/2, TextureManager.turretHead.Height/2),
//            //                 1, SpriteEffects.None, 0.1f);

//            //#endregion
//        }

//        public override void Update(IGameTimeService gameTime)
//        {
//            //base.Update();

//            //sternLaser.timeSinceLastShot += (float) Main.gameTime.ElapsedMilliseconds;
//            //bowLaser.timeSinceLastShot += (float) Main.gameTime.ElapsedMilliseconds;
//        }

//        private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
//        {
//            return true;
//        }

//        /// <summary>
//        /// Takes a position and changes it appropriately according to the rotation of the mothership
//        /// </summary>
//        /// <returns></returns>
//        //Vector2 convertToSpacePosition(Vector2 oldPosition)
//        //{
//        //}
//        private void SortPointsForUseWhenManuallyAddingMothershipPoints()
//        {
///*
//            foreach (Vector2 v in Mothership.staticPoints2)
//                {
//                    Mothership.staticPoints.Add(v);
                    
//                    foreach (Vector2 vv in Mothership.staticPoints2) // Check against points we know won't work
//                    {
//                        if (v == vv) // Skip self
//                            continue;
//                        bool foundV = false;
//                        float d;
//                        foreach (Vector2 vvv in Mothership.staticPoints) // Check if point is close to other points
//                        {
//                            d = Vector2.Distance(v, vvv);
//                            if (d < 10)
//                                foundV = true;
//                        }
//                        if (foundV)
//                            break;
//                        Mothership.staticPoints.Add(v);
//                        /*
//                        d = Vector2.Distance(v, vv);
//                        if (d > 200)
//                        {
//                            if(Mothership.staticPoints.Count == 0)
//                                Mothership.staticPoints.Add(v);
//                            else
//                            foreach (Vector2 vvv in Mothership.staticPoints)
//                            {
//                                d = Vector2.Distance(v, vvv);
//                                if (d > 200)
//                                {
//                                    Mothership.staticPoints.Add(v);
//                                    Console.WriteLine("V:" + v + " VVV:" + vvv);
//                                    break;
//                                }
//                            }

//                        }
//                    }*/
//            // Put this in Ship.cs
//            /*
//            int i = 0;
//            foreach (Vector2 v in Mothership.staticPoints)
//            {
//                i++;
//                spriteBatch.Draw(TextureManager.testPoint, v + ConvertUnits.ToDisplayUnits(body.Position) - new Vector2(TextureManager.mothership.Width / 2, TextureManager.mothership.Height / 2), Color.Cyan);
//                textDrawingService.DrawTextAtLocationCentered(spriteBatch, v + ConvertUnits.ToDisplayUnits(body.Position) + new Vector2(5, -5) - new Vector2(TextureManager.mothership.Width / 2, TextureManager.mothership.Height / 2),
//                    i + "", SpaceStateManager.spaceCam.Zoom * 1.1f);
//            }*/
//        }
//    }
//}