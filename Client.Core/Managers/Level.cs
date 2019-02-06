namespace Freecon.Client.Managers
{
    //internal class Level
    //{
    //    // Sub Classes //
    //    private readonly Stopwatch drawWatch;
    //    private readonly PlanetLevel gameLevel;
    //    private readonly RenderLevel renderLevel;
        

    //    // Rendering Variables //
    //    private readonly int tileSize = 96;
    //    private readonly TurretManager turretManager;
    //    private readonly Stopwatch updateWatch;
    //    private Vector2 landingTile;

    //    /// <summary>
    //    /// Builds and manages a level, including Turrets. Creates bodies for player to collide with.
    //    /// </summary>
    //    /// <param name="Content">Content Manager</param>
    //    /// <param name="level">Level to load for the game.</param>
    //    /// <param name="tileSize">Scale of each tile.</param>
    //    public Level(ContentManager Content, PlanetLevel level, int tileSize)
    //    {
    //        buildEdges(level);

    //        turretManager = new TurretManager(Content, level);

    //        updateWatch = new Stopwatch();
    //        drawWatch = new Stopwatch();

    //        gameLevel = level;
    //        this.tileSize = tileSize;
    //        renderLevel = new RenderLevel();
    //    }

    //    public void buildEdges(PlanetLevel level)
    //    {
    //        bool onEdge = false;
    //        bool foundEdge = false;
    //        bool foundEnd = false;
    //        var edgeStart = new Vector2(0, 0);
    //        var edgeEnd = new Vector2(0, 0);
    //        _body edge;

    //        //Testing stuff
    //        int counter = 0;

    //        /*
    //        for (int x = 0; x < level.PlanetMap.GetLength(0) - 1; x++)
    //            for (int y = 0; y < level.PlanetMap.GetLength(1) - 1; y++)
    //            {
                    
    //                if (level.PlanetMap[x, y].tileType == 2)
    //                {
    //                    landingTile.X = (x * tileSize);
    //                    landingTile.Y = (y * tileSize);
    //                }

    //                if (level.PlanetMap[x, y].tileType == 1 || level.PlanetMap[x, y].tileType == 17 || level.PlanetMap[x, y].tileType == 2) //If not wall
    //                {
    //                    if (!(level.PlanetMap[x + 1, y].tileType == 1 || level.PlanetMap[x + 1, y].tileType == 17 || level.PlanetMap[x + 1, y].tileType == 2))//If wall
    //                    {
    //                        foundEdge = true;
    //                        if (!onEdge)
    //                        {
    //                            edgeStart.X = (x + (float).5) * (tileSize);
    //                            edgeStart.Y = (y - (float).5) * (tileSize);
    //                            onEdge = true;
    //                        }
    //                        edgeEnd.X = (x + (float).5) * (tileSize);
    //                        edgeEnd.Y = (y + (float).5) * (tileSize);


    //                        onEdge = true;

    //                    }
    //                    else if (onEdge)
    //                    {
    //                        foundEnd = true;
    //                    }


    //                }
    //                if (!(level.PlanetMap[x, y].tileType == 1 || level.PlanetMap[x, y].tileType == 17 || level.PlanetMap[x, y].tileType == 2)) //If wall
    //                {
    //                    if ((level.PlanetMap[x + 1, y].tileType == 1 || level.PlanetMap[x + 1, y].tileType == 17 || level.PlanetMap[x + 1, y].tileType == 2))//If not wall
    //                    {
    //                        foundEdge = true;
    //                        if (!onEdge)
    //                        {
    //                            edgeStart.X = (x + (float).5) * (tileSize);
    //                            edgeStart.Y = (y - (float).5) * (tileSize);
    //                            onEdge = true;

    //                        }

    //                        edgeEnd.X = (x + (float).5) * (tileSize);
    //                        edgeEnd.Y = (y + (float).5) * (tileSize);
    //                        onEdge = true;


    //                    }
    //                    else if (onEdge)
    //                    {
    //                        foundEnd = true;
    //                    }

    //                }
    //                else if (level.PlanetMap[x, y].tileType == 17)
    //                    level.PlanetMap[x, y].isTurret = true;
    //                if (foundEnd)
    //                {
    //                    edge = BodyFactory.CreateEdge(PhysicsManager.world, ConvertUnits.ToSimUnits(edgeStart), ConvertUnits.ToSimUnits(edgeEnd));
    //                    edge.BodyType = BodyType.Static;
    //                    edge.CollisionCategories = Category.Cat1;
    //                    edge.SleepingAllowed = true;
    //                    edge.IgnoreCCD = true;
    //                    edge.IsStatic = true;

    //                    edge.UserData = new BodyDataObject(Enums.BodyTypes.VerticalBorder, (uint)counter);//counter as ID in constructor is probably not necessary

    //                    counter++;


    //                    onEdge = false;
    //                    foundEnd = false;


    //                }

    //            }


    //        for (int y = 0; y < level.PlanetMap.GetLength(1) - 1; y++)
    //            for (int x = 0; x < level.PlanetMap.GetLength(0) - 1; x++)
    //            {


    //                if (level.PlanetMap[x, y].tileType == 1 || level.PlanetMap[x, y].tileType == 17 || level.PlanetMap[x, y].tileType == 2) //If not wall
    //                {
    //                    if (!(level.PlanetMap[x, y + 1].tileType == 1 || level.PlanetMap[x, y + 1].tileType == 17 || level.PlanetMap[x, y + 1].tileType == 2))//If wall
    //                    {
    //                        foundEdge = true;
    //                        if (!onEdge)
    //                        {
    //                            edgeStart.X = (x - (float).5) * (tileSize);
    //                            edgeStart.Y = (y + (float).5) * (tileSize);
    //                            onEdge = true;
    //                        }
    //                        edgeEnd.X = (x + (float).5) * (tileSize);
    //                        edgeEnd.Y = (y + (float).5) * (tileSize);


    //                        onEdge = true;

    //                    }
    //                    else if (onEdge)
    //                    {
    //                        foundEnd = true;
    //                    }
    //                }
    //                if (!(level.PlanetMap[x, y].tileType == 1 || level.PlanetMap[x, y].tileType == 17 || level.PlanetMap[x, y].tileType == 2)) //If wall
    //                {
    //                    if ((level.PlanetMap[x, y + 1].tileType == 1 || level.PlanetMap[x, y + 1].tileType == 17 || level.PlanetMap[x, y + 1].tileType == 2))//If not wall
    //                    {
    //                        foundEdge = true;
    //                        if (!onEdge)
    //                        {
    //                            edgeStart.X = (x - (float).5) * (tileSize);
    //                            edgeStart.Y = (y + (float).5) * (tileSize);
    //                            onEdge = true;
    //                        }

    //                        edgeEnd.X = (x + (float).5) * (tileSize);
    //                        edgeEnd.Y = (y + (float).5) * (tileSize);
    //                        onEdge = true;

    //                    }
    //                    else if (onEdge)
    //                    {
    //                        foundEnd = true;
    //                    }
    //                }
    //                if (foundEnd)
    //                {
    //                    edge = BodyFactory.CreateEdge(PhysicsManager.world, ConvertUnits.ToSimUnits(edgeStart), ConvertUnits.ToSimUnits(edgeEnd));
    //                    edge.UserData = new BodyDataObject(Enums.BodyTypes.HorizontalBorder, (uint)counter);//Counter is probably not useful as ID in BodyDataObject
    //                    edge.BodyType = BodyType.Static;
    //                    edge.CollisionCategories = Category.Cat1;
    //                    edge.SleepingAllowed = true;
    //                    edge.IgnoreCCD = true;
    //                    edge.IsStatic = true;
    //                    counter++;

    //                    onEdge = false;
    //                    foundEnd = false;

    //                }

    //            }
    //                */
    //    }

    //    public virtual void Update(Vector2 shipLocation, Vector2 difference)
    //    {
    //        updateWatch.Reset();
    //        updateWatch.Start();
    //        turretManager.Update(shipLocation, difference);
    //        updateWatch.Stop();
    //    }

    //    public virtual void DrawTiles(SpriteBatch spriteBatch, Camera2D shipCam)
    //    {
    //        drawWatch.Reset(); // Debug Timer
    //        drawWatch.Start(); // Debug Timer

    //        renderLevel.Draw(spriteBatch, shipCam, tileSize, gameLevel);

    //        drawWatch.Stop(); // Debug Timer
    //    }

    //    public virtual void DrawTurrets(SpriteBatch spriteBatch, Camera2D shipCam)
    //    {
    //        turretManager.Draw(spriteBatch);
    //    }

    //    public void Debug(SpriteBatch spriteBatch)
    //    {
    //        string timeUpdate = string.Format("Level Update:     {0,4:00.0}",
    //                                          updateWatch.ElapsedTicks/(float) Stopwatch.Frequency*1000.0f);
    //        string timeDraw = string.Format("Level Draw:     {0,4:00.0}",
    //                                        drawWatch.ElapsedTicks/(float) Stopwatch.Frequency*1000.0f);
    //        textDrawingService.DrawTextToScreenLeft(spriteBatch, 4, timeUpdate);
    //        textDrawingService.DrawTextToScreenLeft(spriteBatch, 5, timeDraw);
    //    }

    //    public PlanetLevel getLevel()
    //    {
    //        return gameLevel;
    //    }

    //    public Vector2 GetMiddlePosition(PlanetLevel l)
    //    {
    //        return new Vector2(((l.xSize*tileSize)/2), // Start in Middle
    //                           ((l.ySize*tileSize)/2));
    //    }

    //    public Vector2 GetTile2Position()
    //    {
    //        return landingTile;
    //    }
    //}
}