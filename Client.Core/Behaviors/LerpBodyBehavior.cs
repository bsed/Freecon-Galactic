using System;
using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Objects;
using Core.Interfaces;
using FarseerPhysics.Dynamics;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Core.Behaviors
{
    public class BodyPositionUpdatedEventArgs : EventArgs
    {
        public PositionUpdateData NewPositionUpdate { get; protected set; }

        public long Timestamp { get; protected set; }

        public BodyPositionUpdatedEventArgs(PositionUpdateData newPositionUpdateData, long timestamp)
        {
            NewPositionUpdate = newPositionUpdateData;
            Timestamp = timestamp;
        }
    }

    public interface IBodyBehavior : ISynchronousUpdate, IDraw
    {
        Body Body { get; }
    }

    public class PositionState
    {
        public float AngularVelocity { get; protected set; }
        public float Rotation { get; protected set; }
        public Vector2 LinearVelocity { get; protected set; }
        public Vector2 Position { get; protected set; }

        public PositionState(float angVel, float rotation, Vector2 vel, Vector2 pos)
        {
            AngularVelocity = angVel;
            Rotation = rotation;
            LinearVelocity = vel;
            Position = pos;
        }
    }

    public class PositionLerpState
    {
        public Vector2 Velocity { get; protected set; }
        public Vector2 Position { get; protected set; }

        public PositionLerpState(Vector2 vel, Vector2 pos)
        {
            Velocity = vel;
            Position = pos;
        }
    }

    public class LerpBodyBehavior : IBodyBehavior
    {
        public float targetRotation;
        public Vector2 targetPosition;

        protected Queue<Tuple<float, PositionUpdateData>> _positionUpdateHistory;

        public Ship AttachedShip { get; protected set; }

        public Body Body { get { return AttachedShip.Body; } }

        public IPhysicsObject PhysicsObject { get; protected set; }

        public LerpBodyBehavior(SpriteBatch spriteBatch, TextureManager textureManager, Ship ship,int maxPositionHistory)
        {
            _positionUpdateHistory = new FixedSizedQueue<Tuple<float, PositionUpdateData>>(maxPositionHistory);
            AttachedShip = ship;
            ship.BodyPositionUpdated += AddNewPosition;
        }

        public void NewLerp(PositionUpdateData data)
        {
            var vel = new Vector2(data.XVel, data.YVel);
            var pos = new Vector2(data.XPos, data.YPos);

            var currentState = new PositionState(AttachedShip.AngularVelocity, AttachedShip.Rotation, AttachedShip.LinearVelocity, AttachedShip.Position);
            var nextState = new PositionState(data.AngularVelocity, data.Rotation, vel, pos);

            var newState = LerpState(currentState, nextState);

            AttachedShip.AngularVelocity = newState.AngularVelocity;
            AttachedShip.Rotation = newState.Rotation;
            AttachedShip.LinearVelocity = newState.LinearVelocity;
            AttachedShip.Position = newState.Position;
        }

        public PositionState LerpState(PositionState current, PositionState next)
        {
            var newLerpPosition = LerpPosition(current, next);

            return new PositionState(next.AngularVelocity, next.Rotation, newLerpPosition.Velocity, newLerpPosition.Position);
        }

        public PositionLerpState LerpPosition(PositionState current, PositionState next)
        {
            var currentPosition = current.Position;

            var incomingPosition = next.Position;
            var incomingVelocity = next.LinearVelocity;

            var incomingSpeed = incomingVelocity.Length();

            // Prevent divide by zero.
            var speed = incomingSpeed == 0f ? 0.00001f : incomingSpeed;

            // Our algorithm looks a half second into the future as it's target.
            var distanceToSeekIntoFuture = 0.5f;
            var distanceInTimespan = speed * distanceToSeekIntoFuture;

            var velocityAngle = GetAngleFromVector(incomingVelocity);

            // Where the server's client will be in a half second.
            var serverFutureX = incomingPosition.X + (distanceInTimespan * (float)Math.Sin(velocityAngle));
            var serverFutureY = incomingPosition.Y + (distanceInTimespan * (float)Math.Cos(velocityAngle));

            // Where we will be in a half second.
            var ourFutureX = currentPosition.X + (distanceInTimespan * (float)Math.Sin(velocityAngle));
            var ourFutureY = currentPosition.Y + (distanceInTimespan * (float)Math.Cos(velocityAngle));

            var serverFuture = new Vector2(serverFutureX, serverFutureY);
            var ourFuture = new Vector2(ourFutureX, ourFutureY);

            // How far our ship is from where the server's client will be.
            var distanceFromServerFuture = Vector2.Distance(currentPosition, serverFuture) + 0.0001f;
            var distanceFromOurFuture = Vector2.Distance(currentPosition, ourFuture) + 0.0001f;

            // Grab the different between our target and the server's, then shift us to point at that.
            var angleDifference = FindAngle(currentPosition, serverFuture, ourFuture);
            var desiredAngle = velocityAngle - angleDifference;

            var desiredFutureX = currentPosition.X + (distanceFromServerFuture * (float)Math.Sin(desiredAngle));
            var desiredFutureY = currentPosition.Y + (distanceFromServerFuture * (float)Math.Cos(desiredAngle));

            var desiredFuture = new Vector2(desiredFutureX, desiredFutureY);

            // Adjust the speed based on how far we are from the desired future position.
            var speedAdjustmentFactor = Vector2.Distance(currentPosition, desiredFuture) / distanceFromOurFuture;

            // Keep things relative to half a second.
            var desiredSpeed = speed * speedAdjustmentFactor;

            // Nudge velocity angle to point at destination point.
            var desiredVelocityX = desiredSpeed * (float)Math.Sin(desiredAngle);
            var desiredVelocityY = desiredSpeed * (float)Math.Cos(desiredAngle);

            // Number of seconds until lag distance is covered
            var timeToMeet = distanceFromServerFuture / speed;

            // Snap if we're too de-synced.
            if (timeToMeet >= 1f)
            {

                var distanceBetweenPositions = Vector2.Distance(currentPosition, incomingPosition) * 2f + 0.0001f;

                if (distanceBetweenPositions > distanceFromServerFuture)
                {
                    velocityAngle = GetAngleFromVector(incomingPosition - currentPosition);
                    desiredVelocityX = distanceBetweenPositions * (float)Math.Sin(velocityAngle);
                    desiredVelocityY = distanceBetweenPositions * (float)Math.Cos(velocityAngle);
                }
                else
                {
                    currentPosition = incomingPosition;
                    desiredVelocityX = incomingVelocity.X;
                    desiredVelocityY = incomingVelocity.Y;
                }
            }

            return new PositionLerpState(new Vector2(desiredVelocityX, desiredVelocityY), currentPosition);
        }

        public float GetAngleFromVector(Vector2 vector)
        {
            return (float)(Math.Atan2(-vector.Y, vector.X) + (Math.PI / 2f));
        }

        public float FindAngle(Vector2 origin, Vector2 a, Vector2 b)
        {
            var angle1 = Math.Atan2(a.Y - origin.Y, a.X - origin.X);
            var angle2 = Math.Atan2(b.Y - origin.Y, b.X - origin.X);

            return (float)(angle1 - angle2); 
        }

        public void AddNewPosition(object sender, BodyPositionUpdatedEventArgs args)
        {
            _positionUpdateHistory.Enqueue(
                new Tuple<float, PositionUpdateData>(args.Timestamp, args.NewPositionUpdate)
            );

            var data = args.NewPositionUpdate;

            NewLerp(data);
        }

        public void Update(IGameTimeService gameTime)
        {
        }

        public void Draw(Camera2D camera)
        {
        }
    }
}
