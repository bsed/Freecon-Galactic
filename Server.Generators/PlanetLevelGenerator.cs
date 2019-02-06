using Microsoft.Xna.Framework;
using SRServer.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRServer.Objects.Generation
{
    public static class PlanetLevelGenerator
    {

        public static async Task<IEnumerable<IEnumerable<Vector2>>> GenerateWallPolygonAsync(bool[,] layout)
        {
            var value = await Task.Run(() =>
                {
                    var marching = new MarchingSquare();
                    var islands = LevelDecomposer.DecomposeLevel(layout);

                    if (islands.Count() == 0)
                    {
                        throw new Exception("Downlinked Planet could not be parsed");
                    }

                    // List of Lists, where each list holds the points of an 'island'.
                    // An island is a collision body in the level.
                    var finalPoints = new List<IEnumerable<Vector2>>();

                    var wholeShape = GetExteriorWall(layout, islands);

                    finalPoints.Add(AddIsland(marching, wholeShape, false));

                    foreach (var island in islands.Skip(1))
                    {
                        var shape = MapTilesToArray(layout, island);

                        finalPoints.Add(AddIsland(marching, shape));
                    }

                    return finalPoints;
                });

            return value;
        }

        public static IEnumerable<IEnumerable<Vector2>> GenerateWallPolygon(bool[,] layout)
        {
            var marching = new MarchingSquare();
            var islands = LevelDecomposer.DecomposeLevel(layout);

            if (islands.Count() == 0)
            {
                throw new Exception("Downlinked Planet could not be parsed");
            }

            // List of Lists, where each list holds the points of an 'island'.
            // An island is a collision body in the level.
            var finalPoints = new List<IEnumerable<Vector2>>();

            var wholeShape = GetExteriorWall(layout, islands);

            finalPoints.Add(AddIsland(marching, wholeShape, false));

            foreach (var island in islands.Skip(1))
            {
                var shape = MapTilesToArray(layout, island);

                finalPoints.Add(AddIsland(marching, shape));
            }

            return finalPoints;
        }

        private static IEnumerable<Vector2> AddIsland(MarchingSquare marching, bool[,] wholeShape, bool entryType = true)
        {
            return marching.DoMarch(wholeShape, entryType);
        }

        private static bool[,] GetExteriorWall(bool[,] layout, IEnumerable<IEnumerable<Node>> islands)
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

        private static bool[,] MapTilesToArray(bool[,] layout, IEnumerable<Node> exterior)
        {
            var wholeShape = new bool[layout.GetLength(0), layout.GetLength(1)];

            // Map our walls to the new shape as open space.
            foreach (var i in exterior)
            {
                wholeShape[i.x, i.y] = true;
            }

            return wholeShape;
        }
               
    }
}
