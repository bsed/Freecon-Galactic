using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Freecon.Client.Core.Utils
{
    public partial class Utilities
    {
        static Random _rand;
        static UniqueRandom _uniqueRand;
        
        static Utilities()
        {
            _rand = new Random(43252);

            Stopwatch s = new Stopwatch();
            s.Start();
            _uniqueRand = new UniqueRandom(100000, 666);
            s.Stop();

        }

        public static int Next(int minValue, int maxValue)
        {
            return _rand.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns the next value in a list of random, preallocated, unique values. Not thread safe.
        /// </summary>
        /// <returns></returns>
        public static int NextUnique()
        {
            return _uniqueRand.Next();
        }

        public static List<Vector2> CreateArcPositions(int numPoints, double degreeSpan, float radius, float rotation, Vector2 center)
        {
            List<Vector2> positions = new List<Vector2>(numPoints);

            Vector2 posVec;
            degreeSpan *= Math.PI / 180; //Convert degreeSpan to radians
            float spacing = (float)(degreeSpan / numPoints); //Radian spacing between any two points
            float currentRotation = (float)(rotation - degreeSpan / 2);
            //Starts at most counterclockwise point, moves clockwise as it creates points

            for (int i = 0; i < numPoints; i++)
            {
                posVec.X = (float)(center.X + Math.Sin(currentRotation) * radius);
                posVec.Y = (float)(center.Y - Math.Cos(currentRotation) * radius);
                currentRotation += spacing;
                positions.Add(posVec);
            }

            return positions;
        }

        public static List<float> CreateArcRotations(int numPoints, double degreeSpan, float radius, float rotation, Vector2 center)
        {

            List<float> rotations = new List<float>(numPoints);


            degreeSpan *= Math.PI / 180; //Convert degreeSpan to radians
            float spacing = (float)(degreeSpan / numPoints); //Radian spacing between any two points
            float currentRotation = (float)(rotation - degreeSpan / 2);
            //Starts at most counterclockwise point, moves clockwise as it creates points

            for (int i = 0; i < numPoints; i++)
            {
                currentRotation += spacing;
                rotations.Add(currentRotation);
            }

            return rotations;
        }
        
    }

    /// <summary>
    /// A preallocated list of unique, randomly generated IDs. Not thread safe.
    /// </summary>
    public class UniqueRandom
    {
        List<int> _randomInts;
        int _currentIndex;
        int _numValues;

        public UniqueRandom(int numVals, int minVal = -int.MaxValue, int maxVal = int.MaxValue)
        {
            _currentIndex = 0;
            _numValues = numVals;

            Random r = new Random();//Leave this unseeded, otherwise clients will end up generating the same range of projectileIDs


            var randomHashSet = new HashSet<int>();
            for (int i = 0; i < numVals; i++)
            {
                int val = r.Next(minVal, maxVal);
                if (!randomHashSet.Contains(val))
                {
                    randomHashSet.Add(val);
                }
                else
                {
                    i--;
                }
            }

            _randomInts = new List<int>();
            foreach (var v in randomHashSet)
            {
                _randomInts.Add(v);
            }
        }

        /// <summary>
        /// Returns the next unique ID in the list
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            _currentIndex++;
            if (_currentIndex == _numValues)
            {
                _currentIndex = 0;
            }
            return _randomInts[_currentIndex];
        }
    }
       
}

