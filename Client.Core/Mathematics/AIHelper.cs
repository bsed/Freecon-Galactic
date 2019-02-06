using System;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Mathematics
{
    public class RotationResult
    {
        public float Rotation { get; protected set; }
        public bool Rotated { get; protected set; }

        public RotationResult(float rotation, bool rotated)
        {
            Rotation = rotation;
            Rotated = rotated;
        }
    }

    /// <summary>
    /// Functions used for things like targetting
    /// </summary>
    public class AIHelper
    {
        /// <summary>
        /// Returns the new angle, sets rotated=true if ship rotated, false if ship did not need to rotate or is now pointing at the passed position
        /// If the angle difference less than tolerance, the ship's angle is set and rotated = false
        /// updates rotation by reference
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="currentPosition"></param>
        /// <param name="dt">elapsed time, in ms</param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static RotationResult TurnTowardPosition(float currentRotation, float turnRate, Vector2 currentPosition, Vector2 positionToTurnTo, float dt, float tolerance)
        {
            var angleToTurn = GetRotationToPosition(currentPosition, positionToTurnTo, currentRotation);            

            var rotationThisTimestep = turnRate * dt / 1000f;

            if (!(Math.Abs(angleToTurn) > 0))
            {
                return new RotationResult(currentRotation, false);
            }

            // Set ship to the correct angle, to avoid constantly rotating ship, because rotations occur over discrete timesteps.
            if (Math.Abs(angleToTurn) < tolerance || rotationThisTimestep > Math.Abs(angleToTurn))
            {
                return new RotationResult(currentRotation + angleToTurn, false);
            }

            if (angleToTurn > 0)
            {
                return new RotationResult(currentRotation + rotationThisTimestep, true);
            }

            return new RotationResult(currentRotation - rotationThisTimestep, true);
        }
 

        /// <summary>
        /// Returns a currentRotation rotated by either rotationAmount or turnRate * dt, whichever is smallest
        /// </summary>
        /// <param name="currentRotation"></param>
        /// <param name="rotationAmount"></param>
        /// <param name="dt"></param>
        /// <param name="turnRate"></param>
        /// <returns></returns>
        static public float Rotate(float currentRotation, float angleToRotate, float dt, float turnRate)
        {
            

            float rotationThisTimestep = turnRate * dt / 1000;
            float rotation = currentRotation;


            if (rotationThisTimestep > Math.Abs(angleToRotate))
            {
                rotation = rotation + angleToRotate;
            }
            else
            {
                if (angleToRotate > 0)
                {
                    rotation += rotationThisTimestep;
                }
                else
                {
                    rotation -= rotationThisTimestep;
                }
            }

            return ClampRotation(rotation);

        }

        /// <summary>
        /// Returns the smallest rotation from pos1 to pos2, [-PI,PI]
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="posToTurnTo"></param>
        /// <returns></returns>
        static public float GetRotationToPosition(Vector2 pos1, Vector2 pos2, float currentRotation)
        {
                       

            currentRotation = ClampRotation(currentRotation);

            Vector2 r = pos2 - pos1;
            
            float angleToRotate = (-(float)Math.Atan2(r.X, r.Y) + (float)Math.PI) - currentRotation;

            if (angleToRotate > Math.PI)
                angleToRotate = -(2 * (float)Math.PI - angleToRotate);
            else if (angleToRotate < -Math.PI)
                angleToRotate = (2 * (float)Math.PI + angleToRotate);

            return angleToRotate;

        }

        static public float GetAngleToPosition(Vector2 pos1, Vector2 pos2)
        {
            Vector2 r = pos2 - pos1;
            return (-(float)Math.Atan2(r.X, r.Y) + (float)Math.PI);


        }

        //Returns the smallest from rotation1 to rotation2
        static public float GetSmallestRotation(float rotation1, float rotation2)
        {
            float clampedrot1 = ClampRotation(rotation1);
            float clampedrot2 = ClampRotation(rotation2);


            float angleToRotate = clampedrot2 - clampedrot1;

            if (angleToRotate > Math.PI)
                angleToRotate = -(2 * (float)Math.PI - angleToRotate);
            else if (angleToRotate < -Math.PI)
                angleToRotate = (2 * (float)Math.PI + angleToRotate);

            return angleToRotate;

        }
        /// <summary>
        /// Limit rotation to [0, 2PI], 0 points upscreen, -Y in farseer coordinates
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        static public float ClampRotation(float rotation)
        {

                float val = rotation % (2 * (float)Math.PI);

                if (val < 0)
                    return val + (2 * (float)Math.PI);
                else
                    return val;
           
        }

        /// <summary>
        /// Implementation of proportional navigation
        /// </summary>
        /// <param name="dt">timestep</param>
        /// <param name="knav">Correction proportionality constant between 3 and 5</param>
        /// <returns>Corrective acceleration to be applied</returns>
        static public Vector2 PNav(Vector2 currentPosition, Vector2 currentVelocity, Vector2 targetPosition, Vector2 targetVelocity, float knav)
        {
            //Little trick to allow for cross products, ((a,b,0) x (c,d,0)) x (0, e, f) = (g, h, 0) 
            Vector3 relativeVelocity = new Vector3((targetVelocity - currentVelocity),0);
            Vector3 vecToTarget = new Vector3((targetPosition - currentPosition), 0);

            Vector3 rotVec = Vector3.Cross(vecToTarget, relativeVelocity) / Vector3.Dot(vecToTarget, vecToTarget);

            Vector3 tempRes = Vector3.Cross(relativeVelocity, rotVec);
                    

            Vector2 accel = new Vector2(tempRes.X, tempRes.Y);

            return accel * knav;
            


            //float clampedRotation = ClampRotation(currentRotation);






            ////Using rotations instead of vectors
            //float currentLOS = GetAngleToPosition(currentPosition, targetPosition) - clampedRotation;
            //float nextLOS = GetAngleToPosition(currentPosition + currentVelocity * dt, targetPosition + targetVelocity * dt) - clampedRotation;

            //Console.WriteLine(GetSmallestRotation(currentLOS, nextLOS) * knav * 180 / Math.PI);
            //return GetSmallestRotation(currentLOS, nextLOS) * knav;           

        }


    }
}
