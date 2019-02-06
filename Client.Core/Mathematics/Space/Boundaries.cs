using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Mathematics.Space
{
    public class Boundaries
    {
        /// <summary>
        /// Create a concave circle list of points.
        /// </summary>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="numberOfEdges">Number of semi-circular edges (circle smoothness)</param>
        /// <returns>In order collection of polygon vertices.</returns>
        public static Vertices CreateConcaveSemiCircle(float radius, int numberOfEdges)
        {
            float stepSize = (float) Math.PI/numberOfEdges;

            var points = new Vertices();
            points.Add(new Vector2(radius, 0)); // Origin Point, on Right Side

            for (int i = 1; i < numberOfEdges + 1; i++)
            {
                //Moves Counter-Clockwise from Right Side
                points.Add(new Vector2((float) (radius*Math.Cos(stepSize*i)), (float) (-radius*Math.Sin(stepSize*i))));
            }

            //points.Add(new Vector2(-radius, 0)); // Left-Most Side

            for (int i = 1; i < numberOfEdges + 1; i++)
            {
                //Moves Counter-Clockwise from Left Side
                points.Add(new Vector2((float) (-radius*Math.Cos(stepSize*i)), (float) (radius*Math.Sin(stepSize*i))));
            }

            //points.Add(new Vector2(radius, 0)); // Origin Point, links together shape.

            return points;
        }
    }
}