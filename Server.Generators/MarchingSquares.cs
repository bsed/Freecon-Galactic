using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SRServer.Mathematics
{
    // Licensed under the creative commons Atrribution 3.0 license
    // You are free to share, copy, distribute, and transmit this work
    // You are free to alter this work
    // If you use this work, please attribute it somewhere in the supporting
    // documentation of your work. (A mention in a readme or credits, for example.)
    // Please leave a comment or email me if you use this work, so I am compelled
    // to produce more of it's kind.
    // 2010 - Phillip Spiess

    public class MarchingSquare
    {
        /// <summary>
        /// A simple enumeration to represent the direction we
        /// just moved, and the direction we will next move.
        /// </summary>
        private enum StepDirection
        {
            None,
            Up,
            Left,
            Down,
            Right
        }

        // Some variables to make our function
        // calls a little smaller, probably shouldn't
        // expose these to the public though.

        /// <summary>
        /// Holds the color information for our texture
        /// </summary>
        private bool[,] data;

        /// <summary>
        /// The direction we previously stepped
        /// </summary>
        private StepDirection previousStep;

        /// <summary>
        /// Our next step direction
        /// </summary>
        private StepDirection nextStep;

        private float TileSize = 1;

        private bool value;

        /// <summary>
        /// Takes a texture and returns a list of pixels that
        /// define the perimeter of the upper-left most
        /// object in that texture, using alpha==0 to determine
        /// the boundary.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<Vector2> DoMarch(bool[,] target, bool value)
        {
            data = target;

            this.value = value;

            // Find the start points
            Vector2 perimeterStart = FindStartPoint();

            // Return the list of points
            return WalkPerimeter((int)perimeterStart.X, (int)perimeterStart.Y);
        }

        /// <summary>
        /// Finds the first pixel in the perimeter of the image
        /// </summary>
        /// <returns>Start Point</returns>
        private Vector2 FindStartPoint()
        {
            // Scan along the whole image
            for (int y = 0; y < data.GetLength(1); y++)
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    // If the pixel is not entirely transparent
                    // we've found a start point
                    if (data[x, y] == value)
                        return new Vector2(x, y);
                }

            // If we get here
            // we've scanned the whole image and found nothing.
            return Vector2.Zero;
        }

        /// <summary>
        /// Performs the main while loop of the algorithm
        /// </summary>
        /// <param name="startX">X Position to start</param>
        /// <param name="startY">Y Position to start</param>
        /// <returns>List of points around perimeter.</returns>
        private List<Vector2> WalkPerimeter(int startX, int startY)
        {
            // Do some sanity checking, so we aren't
            // walking outside the image
            if (startX < 0)
                startX = 0;
            if (startX > data.GetLength(0))
                startX = data.GetLength(0);
            if (startY < 0)
                startY = 0;
            if (startY > data.GetLength(1))
                startY = data.GetLength(1);

            // Set up our return list
            List<Vector2> pointList = new List<Vector2>();

            // Our current x and y positions, initialized
            // to the init values passed in
            int x = startX;
            int y = startY;

            // The main while loop, continues stepping until 
            // we return to our initial points
            do
            {
                // Evaluate our state, and set up our next direction
                Step(x, y);

                // If our current point is within our image
                // add it to the list of points
                if (x >= 0 &&
                    x < data.GetLength(0) &&
                    y >= 0 &&
                    y < data.GetLength(1))
                    pointList.Add(new Vector2(x, y));

                switch (nextStep)
                {
                    case StepDirection.Up: y--; break;
                    case StepDirection.Left: x--; break;
                    case StepDirection.Down: y++; break;
                    case StepDirection.Right: x++; break;
                    default:
                        break;
                }
            } while (x != startX || y != startY);

            return pointList;

        }

        /// <summary>
        /// Determines and sets the state of the 4 pixels that
        /// represent our current state, and sets our current and
        /// previous directions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void Step(int x, int y)
        {
            // Scan our 4 pixel area
            bool upLeft = IsPixelSolid(x - 1, y - 1);
            bool upRight = IsPixelSolid(x, y - 1);
            bool downLeft = IsPixelSolid(x - 1, y);
            bool downRight = IsPixelSolid(x, y);

            // Store our previous step
            previousStep = nextStep;

            // Determine which state we are in
            int state = 0;

            if (upLeft)
                state |= 1;
            if (upRight)
                state |= 2;
            if (downLeft)
                state |= 4;
            if (downRight)
                state |= 8;

            // State now contains a number between 0 and 15
            // representing our state.
            // In binary, it looks like 0000-1111 (in binary)

            // An example. Let's say the top two pixels are filled,
            // and the bottom two are empty.
            // Stepping through the if statements above with a state 
            // of 0b0000 initially produces:
            // Upper Left == true ==>  0b0001
            // Upper Right == true ==> 0b0011
            // The others are false, so 0b0011 is our state 
            // (That's 3 in decimal.)

            // Looking at the chart above, we see that state
            // corresponds to a move right, so in our switch statement
            // below, we add a case for 3, and assign Right as the
            // direction of the next step. We repeat this process
            // for all 16 states.

            // So we can use a switch statement to determine our
            // next direction based on
            switch (state)
            {
                case 1: nextStep = StepDirection.Up; break;
                case 2: nextStep = StepDirection.Right; break;
                case 3: nextStep = StepDirection.Right; break;
                case 4: nextStep = StepDirection.Left; break;
                case 5: nextStep = StepDirection.Up; break;
                case 6:
                    if (previousStep == StepDirection.Up)
                    {
                        nextStep = StepDirection.Left;
                    }
                    else
                    {
                        nextStep = StepDirection.Right;
                    }
                    break;
                case 7: nextStep = StepDirection.Right; break;
                case 8: nextStep = StepDirection.Down; break;
                case 9:
                    if (previousStep == StepDirection.Right)
                    {
                        nextStep = StepDirection.Up;
                    }
                    else
                    {
                        nextStep = StepDirection.Down;
                    }
                    break;
                case 10: nextStep = StepDirection.Down; break;
                case 11: nextStep = StepDirection.Down; break;
                case 12: nextStep = StepDirection.Left; break;
                case 13: nextStep = StepDirection.Up; break;
                case 14: nextStep = StepDirection.Left; break;
                default:
                    nextStep = StepDirection.None;
                    break;
            }
        }

        /// <summary>
        /// Determines if a single pixel is solid (we test against
        /// alpha values, you can write your own test if you want
        /// to test for a different color.)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsPixelSolid(int x, int y)
        {
            // Make sure we don't pick a point outside our
            // image boundary!
            if (x < 0 || y < 0 ||
                x >= data.GetLength(0) || y >= data.GetLength(1))
                return false;

            // Check the color value of the pixel
            // If it isn't 100% transparent, it is solid
            if (data[x, y] == value)
                return true;

            // Otherwise, it's not solid
            return false;
        }
    }
}
