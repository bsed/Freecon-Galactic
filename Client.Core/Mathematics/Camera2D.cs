using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Mathematics
{
    /// <summary>
    /// Camera 2D class used to modify the viewport. 
    /// 
    /// WARNING:
    /// Grabbed from online, not written by us.
    /// </summary>
    public class Camera2D
    {
        protected float _angleOfShake;
        protected bool _directionOfShake;
        protected float _maxShake;
        public Vector2 Position; // Camera Position
        protected float _rotation; // Camera Rotated
        public GameWindow GameWindow { get; protected set; }


        public float CameraAngle => _cameraAngle;

        // Shake Variables
        protected float _cameraAngle;
        public Matrix _transform; // Matrix Transform
        public float _zoom; // Camera Zoom

        /// <summary>
        /// Initializes the Camera at 0,0 and 1x zoom.
        /// </summary>
        public Camera2D(GameWindow gameWindow)
        {
            _zoom = 1.0f;
            _rotation = 0.0f;
            Position = Vector2.Zero;
            GameWindow = gameWindow;
        }

        // Sets and gets zoom
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < 0.1f) _zoom = 0.1f;
            } // Negative zoom will flip image
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera

        // Get set position
        public Vector2 Pos
        {
            get { return Position; }
            set { Position = value; }
        }

        public void Move(Vector2 amount)
        {
            Position += amount;
        }

        /// <summary>
        /// Math function that translates position to the viewport.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        public Matrix GetTransformation(GraphicsDevice graphicsDevice)
        {
            _transform =
                Matrix.CreateTranslation(new Vector3(-(Position.X + (int) (Math.Cos(_angleOfShake)*_cameraAngle)),
                                                     -(Position.Y + (int) (Math.Tan(_angleOfShake)*_cameraAngle)), 0))*
                Matrix.CreateRotationZ(Rotation)*
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1))*
                Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width*0.5f,
                                                     graphicsDevice.Viewport.Height*0.5f, 0));
            return _transform;
        }

        /// <summary>
        /// Shakes camera at a specified angle.
        /// </summary>
        /// <param name="angle">Angle to shake camera towards.</param>
        /// <param name="amount">Amount to push camera.</param>
        public void ShakeCamera(float angle, float amount)
        {
            if (_maxShake != 0)
            {
                _maxShake *= 1.4f; // Push it out a bit more, since we're getting hit.
            }
            _angleOfShake = angle;
            _maxShake = amount;
        }

        /// <summary>
        /// Shakes camera at a random angle.
        /// </summary>
        /// <param name="amount">Amount to push camera.</param>
        public void ShakeCamera(float amount)
        {
            ShakeCamera(MathHelper.ToRadians(new Random(88456).Next(1, 360)), amount);
        }

        /// <summary>
        /// Updates camera shaking.
        /// </summary>
        public void UpdateCameraShake()
        {
            //Jesus fuck. Did I write this? I hope I didn't write this.

            // If there is nothing to do here, return.
            if (_maxShake == 0)
                return;

            // True means we're approaching the peak, false is towards origin.
            if (_directionOfShake)
            {
                _cameraAngle += _maxShake*.1f; // 1/10th per frame. 166ms total

                // If we're past where we should be
                if (_cameraAngle > _maxShake)
                {
                    _cameraAngle = _maxShake;
                    _directionOfShake = false; // Go back to origin
                }
                return;
            }

            _cameraAngle -= _maxShake*.1f; // 1/10th per frame. 166ms total

            // If we're at the middle
            if (_cameraAngle < 0)
            {
                // Reset everything
                _maxShake = 0; // End shake
                _cameraAngle = 0;
                _angleOfShake = 0;
                _directionOfShake = true;
            }
        }
    }
}