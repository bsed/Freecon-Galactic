
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Invasion;
using Freecon.Client.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using Freecon.Models.TypeEnums;
using Freecon.Client.Interfaces;
using Server.Managers;

namespace Freecon.Client.Objects
{
    /// <summary>
    /// This badly needs a refactor.
    /// </summary>
    public class PlanetLevel
    {
        // Planet Map that contains data
        public Tile[,] PlanetMap;

        public PlanetTypes PlanetType;

        // Sub Classes //
        private RenderLevel renderLevel;

        // Rendering Variables //
        private float _tileSize = 1;
        private SpriteBatch _spriteBatch;
        private PhysicsManager _physicsManager;

        public List<Vector2> vertices = new List<Vector2>(100);
        public List<Vector2> RedVertices = new List<Vector2>(100);
        public List<Vector2> GreenVertices = new List<Vector2>(100);

        public int xSize, ySize; // Scale in tiles of planet.

        //debug
        bool _gotEdgeError;

        int _wallWidth;
        int _wallHeight;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanetLevel"/> class based on specified layout array.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch. Set to null if the planet won't be drawn.</param>
        /// <param name="wallTexWidth">width of the wall texture, in pixels</param>
        /// <param name="wallTexHeight">height of the wall texture, in pixels</param>
        /// <param name="physicsManager">The physics manager.</param>
        /// <param name="planetType">Type of the planet.</param>
        /// <param name="islands">The islands.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="layoutArray">The layout array.</param>
        /// <param name="textureManager">Set to null if the planet will not be drawn.</param>
        public PlanetLevel(SpriteBatch spriteBatch,
                           int wallTexWidth, int wallTexHeight,
                           PhysicsManager physicsManager,
                           PlanetTypes planetType,
                           IEnumerable<IEnumerable<Vector2>> islands,
                           int height, int width, bool[] layoutArray, TextureManager textureManager = null)
        {
            _wallWidth = wallTexWidth;
            _wallHeight = wallTexHeight;

            _spriteBatch = spriteBatch;
            _physicsManager = physicsManager;

            PlanetType = planetType;
            SetPlanetSize(width, height);//Creates PlanetMap
            int counter = 0;

            // Convert layoutArray to 2D array for easier processing
            bool[,] layout2D = new bool[width,height];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    layout2D[j, i] = layoutArray[counter];
                    counter++;
                }

            SetOutsideWallToTile(ref PlanetMap, TileTypes.Wall);

            FindAndSetWalls(ref PlanetMap, layout2D);

            //CreateWallBodies(layout2D);
            BuildEdges(ref PlanetMap);
            //GenerateWallPolygon(layout2D);

            foreach (var points in islands)
            {
                //var verts = CreateCollisionObject(points);
            }

            if(textureManager != null)
                renderLevel = new RenderLevel(textureManager);
        }

        public void GenerateWallPolygon(bool[,] layout)
        {
            // Debug shit
            //vertices.Clear();

            var marching = new MarchingSquare();
            var islands = LevelDecomposer.DecomposeLevel(layout);

            if(islands.Count() == 0)
            {
                throw new Exception("Downlinked Planet could not be parsed");
            }

            var wholeShape = GetExteriorWall(layout, islands);

            AddIsland(marching, wholeShape, false); // false means wall

            foreach (var island in islands.Skip(1))
            {
                var shape = MapTilesToArray(layout, island);

                AddIsland(marching, shape);
            }
        }

        private void AddIsland(MarchingSquare marching, bool[,] wholeShape, bool entryType = true)
        {
            var points = marching.DoMarch(wholeShape, entryType);

            var verts = CreateCollisionObject(points);

            vertices.AddRange(verts.Select(p => p * 100)); // Debug
        }

        private Vertices CreateCollisionObject(IEnumerable<Vector2> points)
        {
            var verts = new Vertices(points);
            verts.Scale(new Vector2(_tileSize, _tileSize));

            //DEBUG
            foreach (Vector2 v in verts)
                vertices.Add(v);

            throw new NotImplementedException("Disabled. Unused and broken.");


            // Removes points along a line. Hasn't proven to mess with things yet.
            //verts = SimplifyTools.CollinearSimplify(verts);

            //var body = BodyFactory.CreateLoopShape(_physicsManager.World, verts, Vector2.Zero, new CollisionDataObject(BodyTypes.VerticalBorder, 0));


            //body.UserData = new CollisionDataObject(BodyTypes.WallEdge, 0);
            //body.IsStatic = true;
            //body.Restitution = 0.8f;

            //return verts;
        }

        private bool[,] GetExteriorWall(bool[,] layout, IEnumerable<IEnumerable<Node>> islands)
        {
            // First piece is always exterior walls. If it isn't, well what the fuck.
            var exterior = islands.First();

            var wholeShape = MapTilesToArray(layout, exterior);

            // Invert the whole thing
            for (int y = 0; y < wholeShape.GetLength(1); y++)
                for (int x = 0; x < wholeShape.GetLength(0); x++)
                {
                    wholeShape[x, y] = !wholeShape[x, y];
                }

            // Get the distinct outside of our shape.
            var xx = LevelDecomposer.DecomposeLevel(wholeShape);

            var exteriorTiles = xx.First(); // Should only ever be one shape here.

            wholeShape = new bool[layout.GetLength(0), layout.GetLength(1)];

            // Map it and return it.
            foreach (var i in exteriorTiles)
            {
                wholeShape[i.x, i.y] = true;
            }

            return wholeShape;
        }

        private bool[,] MapTilesToArray(bool[,] layout, IEnumerable<Node> exterior)
        {
            var wholeShape = new bool[layout.GetLength(0), layout.GetLength(1)];

            // Map our walls to the new shape as open space.
            foreach (var i in exterior)
            {
                wholeShape[i.x, i.y] = true;
            }

            return wholeShape;
        }

        public virtual void DrawTiles(Camera2D shipCam)
        {
            if(renderLevel != null && _spriteBatch != null)
                renderLevel.Draw(_spriteBatch, shipCam, _tileSize, this);

            //for (int i = 0; i < vertices.Count; i++)
            //{
            //    _spriteBatch.Draw(TextureManager.redPoint, new Rectangle((int)vertices[i].X, (int)vertices[i].Y, 9, 9), Color.Red);
            //}

            //for (int i = 0; i < GreenVertices.Count; i++)
            //{
            //    _spriteBatch.Draw(TextureManager.greenPoint, new Rectangle((int)GreenVertices[i].X, (int)GreenVertices[i].Y, 9, 9), Color.Green);
            //}
            //for (int i = 0; i < RedVertices.Count; i++)
            //{
            //    _spriteBatch.Draw(TextureManager.redPoint, new Rectangle((int)RedVertices[i].X, (int)RedVertices[i].Y, 9, 9), Color.Red);
            //}
        }

        /// <summary>
        /// Set all tiles at outside edge to walls
        /// </summary>
        /// <param name="Tiles">Reference to tiles to be changed</param>
        void SetOutsideWallToTile(ref Tile[,] Tiles, TileTypes ChangeTo)
        {
            // Sets Horizontal tiles, two at a time
            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                Tiles[i, 0].type = ChangeTo;
                Tiles[i, Tiles.GetLength(1) - 1].type = ChangeTo;
            }

            // Sets Vertical tiles, two at a time
            for (int i = 0; i < Tiles.GetLength(1); i++)
            {
                Tiles[0, i].type = PlanetMap[i, 0].type = ChangeTo;
                Tiles[i, Tiles.GetLength(0) - 1].type = ChangeTo;
            }
        }

        /// <summary>
        /// Process to find and set edges
        /// </summary>
        /// <param name="Tiles">Reference to multidimensional tile array</param>
        /// <param name="layout2D">Multidimensional list of booleans</param>
        void FindAndSetWalls(ref Tile[,] Tiles, bool[,] layout2D)
        {
            for (int i = 0; i < Tiles.GetLength(0); i++)
                for (int j = 0; j < Tiles.GetLength(1); j++)
                {
                    //False for wall, ignores ground tiles
                    if (!layout2D[i, j])
                    {
                        SetTileType(ref Tiles, layout2D, i, j);
                    }
                    else
                        Tiles[i, j].type = TileTypes.Ground;
                }
        }

        private void SetTileType(ref Tile[,] planetMap, bool[,] layout2D, int i, int j)
        {
            //    Left              Top                 Right               Bottom
            if (CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.Ground;//WARNING:Fix this, there is no tiletype for a wall surrounded by ground on all sides

            else if (CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.TopEnd;

            else if (CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.LeftEnd;

            else if (CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.TopLeft;

            else if (CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j) && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.BottomEnd;

            else if (CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.LeftRight;

            else if (CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.BottomLeft;

            else if (CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.LeftWall;

            else if (!CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.RightEnd;
            //    Left              Top                 Right               Bottom
            else if (!CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.TopRight;

            else if (!CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.TopBottom;

            else if (!CheckLeft(layout2D, i, j)
                && CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.TopWall;

            else if (!CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.BottomRight;

            else if (!CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.RightWall;

            else if (!CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.BottomWall;

            else if (!CheckLeft(layout2D, i, j)
                && !CheckTop(layout2D, i, j)
                && !CheckRight(layout2D, i, j)
                && !CheckBottom(layout2D, i, j))
                planetMap[i, j].type = TileTypes.Wall;
        }

        bool CheckLeft(bool[,] layout2D, int x, int y)
        {
            if (x == 0)
                return false;
            return layout2D[x - 1, y];
        }
        bool CheckTop(bool[,] layout2D, int x, int y)
        {
            if (y == 0)
                return false;
            return layout2D[x, y - 1];
        }
        bool CheckRight(bool[,] layout2D, int x, int y)
        {
            if (x == layout2D.GetLength(0) - 1)
                return false;
            return layout2D[x + 1, y];
        }
        bool CheckBottom(bool[,] layout2D, int x, int y)
        {
            if (y == layout2D.GetLength(1) - 1)
                return false;
            return layout2D[x, y + 1];
        }

        private void SetPlanetSize(int xSize, int ySize)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            PlanetMap = new Tile[xSize, ySize];

            for(int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    PlanetMap[i, j] = new Tile();
                    PlanetMap[i, j].position.X = _tileSize * (float)i;
                    PlanetMap[i, j].position.Y = _tileSize * (float)j;

                }
        }

        public Vector2 SetToMiddle(PlanetLevel l)
        {
            return new Vector2(((l.xSize * _wallWidth)/2), // Start in Middle
                               ((l.ySize * _wallHeight) / 2));
        }

        public void BuildEdges(ref Tile[,] level)
        {
            bool onEdge = false;

            vertices.Clear();
            bool tracingLeftEdge = false;
            bool tracingRightEdge = false;
            bool tracingTopEdge = false;
            bool tracingBottomEdge = false;

            List<Tuple<Vector2, Vector2, BodyTypes>> edges = new List<Tuple<Vector2,Vector2, BodyTypes>>(100);//Each Tuple represents an edge, <Start, Finish>

            var edgeStart = new Vector2(0, 0);
            var edgeEnd = new Vector2(0, 0);
            Body edgeBody = null;
            BodyTypes wallType = BodyTypes.TopWall;//This value will never be used, but is needed to avoid uninitialization error

            // Testing stuff
            int counter = 0;

            #region Find Vertical Edges
            // Run horizontal sweep to check for faces
            for (int x = 0; x < level.GetLength(0) - 1; x++)
                for (int y = 0; y < level.GetLength(1); y++)
                {
                    if (!IsWall(level[x, y])) // If not wall
                    {
                        if (tracingLeftEdge)
                        {
                            //Second terminating case for left wall

                            edgeEnd.X = (x + .5f) * (_tileSize);
                            edgeEnd.Y = (y - .5f) * (_tileSize);
                           

                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.LeftWall));
                            tracingLeftEdge = false;
                            onEdge = false;

                        }
                        
                        if (IsWall(level[x + 1, y])) // If wall to the right (10)
                        {

                            if (!onEdge)
                            {
                                onEdge = true;
                                tracingRightEdge = true;

                                if (IsWall(level[x, y - 1]))
                                {
                                    edgeStart.X = (x + 1.5f) * (_tileSize);
                                    edgeStart.Y = (y - .5f) * (_tileSize);
                                }
                                else
                                {
                                    edgeStart.X = (x + 1.5f) * (_tileSize);
                                    edgeStart.Y = (y + .5f) * (_tileSize);

                                }
                            }
                            





                        }
                        else if (onEdge)
                        {
                            //First terminating case for right walls

                            edgeEnd.X = (x + 1.5f) * (_tileSize);
                            edgeEnd.Y = (y - .5f) * (_tileSize);

                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.RightWall));
                            tracingRightEdge = false;
                            onEdge = false;
                            
                        }
                    }
                    else // If wall
                    {
                        if (tracingRightEdge)
                        {
                            //Second terminating case for right walls

                            edgeEnd.X = (x + 1.5f) * (_tileSize);
                            edgeEnd.Y = (y + .5f) * (_tileSize);

                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.RightWall));
                            tracingRightEdge = false;
                            onEdge = false;

                            

                        }
                       
                        if (!IsWall(level[x + 1, y])) // If not wall to the right (01)
                        {

                            if (!onEdge)
                            {
                                onEdge = true;
                                tracingLeftEdge = true;

                                if (IsWall(level[x, y - 1]))
                                {
                                    edgeStart.X = (x + .5f) * (_tileSize);
                                    edgeStart.Y = (y - .5f) * (_tileSize);
                                }
                                else
                                {
                                    edgeStart.X = (x + .5f) * (_tileSize);
                                    edgeStart.Y = (y + .5f) * (_tileSize);

                                }
                            }

                            


                            

                        }
                        else if (onEdge)
                        {
                            //First terminating case for left walls

                            edgeEnd.X = (x + .5f) * (_tileSize);
                            edgeEnd.Y = (y + .5f) * (_tileSize);



                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.LeftWall));
                            tracingLeftEdge = false;
                            onEdge = false;
                        }
                    }

                }

            #endregion

            #region Find Horizontal Edges
            
            for (int y = 0; y < level.GetLength(1) - 1; y++)
                for (int x = 0; x < level.GetLength(0); x++)
                {
                    if (!IsWall(level[x, y])) // If not wall
                    {
                        if (tracingTopEdge)
                        {
                            //Second terminating case for top wall

                            edgeEnd.X = (x - .5f) * (_tileSize);
                            edgeEnd.Y = (y + .5f) * (_tileSize);


                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.TopWall));
                            tracingTopEdge = false;
                            onEdge = false;

                        }

                        if (IsWall(level[x, y + 1])) // If wall to the bottom
                        {

                            if (!onEdge)
                            {
                                onEdge = true;
                                tracingBottomEdge = true;

                                if (IsWall(level[x - 1, y]))
                                {
                                    edgeStart.X = (x - .5f) * (_tileSize);
                                    edgeStart.Y = (y + 1.5f) * (_tileSize);


                                }
                                else
                                {
                                    edgeStart.X = (x + .5f) * (_tileSize);
                                    edgeStart.Y = (y + 1.5f) * (_tileSize);

                                }
                            }
                        }
                        else if (onEdge)
                        {
                            //First terminating case for bottom walls

                            edgeEnd.X = (x - .5f) * (_tileSize);
                            edgeEnd.Y = (y + 1.5f) * (_tileSize);
                            

                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.BottomWall));
                            tracingBottomEdge = false;
                            onEdge = false;

                        }
                    }
                    else // If current tile is wall
                    {
                        if (tracingBottomEdge)
                        {
                            //Second terminating case for bottom walls

                            edgeEnd.X = (x + .5f) * (_tileSize);
                            edgeEnd.Y = (y + 1.5f) * (_tileSize);
                            //Second

                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.BottomWall));
                            tracingBottomEdge = false;
                            onEdge = false;

                        }

                        if (!IsWall(level[x, y + 1])) // If not wall to the bottom (01)
                        {

                            if (!onEdge)
                            {
                                onEdge = true;
                                tracingTopEdge = true;

                                if (IsWall(level[x - 1, y]))
                                {
                                    edgeStart.X = (x - .5f) * (_tileSize);
                                    edgeStart.Y = (y + .5f) * (_tileSize);
                                    
                                }
                                else
                                {
                                    edgeStart.X = (x + .5f) * (_tileSize);
                                    edgeStart.Y = (y + .5f) * (_tileSize);
                                    
                                }
                            }

                        }
                        else if (onEdge)
                        {
                            //First terminating case for top walls

                            edgeEnd.X = (x + .5f) * (_tileSize);
                            edgeEnd.Y = (y + .5f) * (_tileSize);



                            edges.Add(new Tuple<Vector2, Vector2, BodyTypes>(edgeStart, edgeEnd, BodyTypes.TopWall));
                            tracingTopEdge = false;
                            onEdge = false;
                        }
                    }

                }
            
            #endregion

            #region Create vertical edge bodies

            

            foreach (var tup in edges)
            {
                //For visual clarity
                edgeStart = tup.Item1;
                edgeEnd = tup.Item2;
                wallType = tup.Item3;
                Vector2 tempPos;

                if (edgeStart == edgeEnd)
                {                    
                    if (!_gotEdgeError && Debugging.WriteLayoutEdgeErrors)
                    {
                        ConsoleManager.WriteLine("Error: Edge start = edge end, at point " + edgeStart + "; skipping edge.", ConsoleMessageType.Error);
                        ConsoleManager.WriteLine("Squelching edge errors for this layout", ConsoleMessageType.Error);
                    }
                    _gotEdgeError = true;
                    continue;
                }
                //else
                    //Console.WriteLine("Adding edge from " + edgeStart + " to " + edgeEnd);
                
                //edge = BodyFactory.CreateEdge(_physicsManager.World, ConvertUnits.ToSimUnits(edgeStart), ConvertUnits.ToSimUnits(edgeEnd));

                switch(wallType)
                {
                    case BodyTypes.TopWall:
                        Debugging.AddStack.Push(this.ToString());
                        edgeBody = BodyFactory.CreateRectangle(_physicsManager.World, edgeEnd.X - edgeStart.X, _tileSize / 2, 1);
                        tempPos.X = (edgeStart.X + edgeEnd.X) / 2;
                        tempPos.Y = edgeStart.Y - _tileSize/4;

                        edgeBody.Position = tempPos;
                        
                        break;

                    case BodyTypes.BottomWall:
                        Debugging.AddStack.Push(this.ToString());
                        edgeBody = BodyFactory.CreateRectangle(_physicsManager.World, edgeEnd.X - edgeStart.X, _tileSize / 2, 1);
                        tempPos.X = (edgeStart.X + edgeEnd.X) / 2;
                        tempPos.Y = edgeStart.Y + _tileSize/4;

                        edgeBody.Position = tempPos;
                        break;

                    case BodyTypes.LeftWall:
                        Debugging.AddStack.Push(this.ToString());
                        edgeBody = BodyFactory.CreateRectangle(_physicsManager.World, _tileSize / 2, edgeEnd.Y - edgeStart.Y, 1);
                        tempPos.Y = (edgeStart.Y + edgeEnd.Y) / 2;
                        tempPos.X = edgeStart.X - _tileSize/4;

                        edgeBody.Position = tempPos;

                        break;

                    case BodyTypes.RightWall:
                        Debugging.AddStack.Push(this.ToString());
                        edgeBody = BodyFactory.CreateRectangle(_physicsManager.World, _tileSize / 2, edgeEnd.Y - edgeStart.Y, 1);
                        tempPos.Y = (edgeStart.Y + edgeEnd.Y) / 2;
                        tempPos.X = edgeStart.X + _tileSize/4;

                        edgeBody.Position = tempPos;

                        break;


                }
                

                
                
                edgeBody.BodyType = BodyType.Static;
                edgeBody.CollisionCategories = Category.Cat1;
                //edge.SleepingAllowed = true;
                //edge.IgnoreCCD = true;
                edgeBody.IsStatic = true;

                var edgeObject = new Edge();
                edgeObject.Body = edgeBody;
                edgeObject.Id = counter;//counter as ID in constructor is probably not necessary
                edgeBody.UserData = new CollisionDataObject(edgeObject, wallType);

                counter++;
                if (wallType == BodyTypes.BottomWall)
                {
                    //GreenVertices.Add(edgeStart);
                    //GreenVertices.Add(edgeEnd);
                }
                else
                {
                    //RedVertices.Add(edgeStart);
                    //RedVertices.Add(edgeEnd);


                }

            }

            #endregion

            #region Create Horizontal Edge Bodies (broken)
            /*
            counter = 0;
            int index = 0;

            for (int i = 0; i < vertices.Count / 2; i++)
            {
                edgeStart = vertices[0];
                //Find the closest vertex with the same y value
                index = vertices.FindIndex(1, delegate(Vector2 v) { return v.Y == edgeStart.Y; });
                edgeEnd = vertices[index];
                vertices.Remove(vertices[0]);
                vertices.Remove(vertices[index]);

                Console.WriteLine("Adding edge from " + edgeStart + " to " + edgeEnd);
                edge = BodyFactory.CreateEdge(_physicsManager.World, ConvertUnits.ToSimUnits(edgeStart), ConvertUnits.ToSimUnits(edgeEnd));
                edge.BodyType = BodyType.Static;
                edge.CollisionCategories = Category.Cat1;
                //edge.SleepingAllowed = true;
                //edge.IgnoreCCD = true;
                edge.IsStatic = true;

                edge.UserData = new BodyDataObject(wallType, (uint)counter);//counter as ID in constructor is probably not necessary

                
            }


            */



            #endregion


        }

        bool IsWall(Tile Tile)
        {
            switch (Tile.type)
            {
                case TileTypes.Ground:
                    return false;
                default:
                    return true;
            }
        }
       

        public Vector2 GetMiddlePosition(PlanetLevel l)
        {
            return new Vector2(((l.xSize * _tileSize) / 2), // Start in Middle
                               ((l.ySize * _tileSize) / 2));
        }


        Point[] FloodFill(Tile[,] fill, int x, int y, Tile value)
        {
            // Q allows us to non-recursively flood fill.
            var Q = new Queue<Point>();
            // List of Points that we'll return, these will become 'ground' tiles
            var Nodes = new List<Point>();

            if (fill[x, y] != value) // Return because we can't do anything
                return Nodes.ToArray();

            Q.Enqueue(new Point { X = x, Y = y });
            while (Q.Count > 0) // While there are tiles to check
            {
                // Pull from queue
                Point N = Q.Dequeue();

                // Prevent crashes
                if (N.X < 0 || N.X > fill.GetLength(0)
                    || N.Y < 0 || N.Y > fill.GetLength(1)
                    || Nodes.Contains(N))
                    continue;

                // Check if value is what we want
                if (fill[N.X, N.Y] == value)
                {
                    Nodes.Add(N); // Add to list to be returned
                    if (N.X - 1 > 0)
                        Q.Enqueue(new Point { X = N.X - 1, Y = N.Y }); // Add West node
                    if (N.Y - 1 > 0)
                        Q.Enqueue(new Point { X = N.X, Y = N.Y - 1 }); // Add North node
                    if (N.X + 1 < fill.GetLength(0))
                        Q.Enqueue(new Point { X = N.X + 1, Y = N.Y }); // Add East node
                    if (N.Y + 1 < fill.GetLength(1))
                        Q.Enqueue(new Point { X = N.X, Y = N.Y + 1 }); // Add South node
                }
            }

            return Nodes.ToArray();
        }
              


    }

    public class Edge : ICollidable
    {
        public Body Body { get; set; }

        public bool Enabled { get { return Body.Enabled; } }

        public int Id { get; set; }
    }
}
        
