using System;

namespace Freecon.Client.Core.Objects
{
    public class DrawData3D
    {

        /// <summary>
        /// Rotation about the model Y axis, should tilt the nose of the ship up or down
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// Rotation about the model X axis, should raise one wing above the other
        /// </summary>
        public float Roll { get; set; }

        //Yaw, or rotation about the model Z axis, is given by the physics object's rotation

        /// <summary>
        /// Constant offset, used to orient the model coordinates into a top-down perspective with the front pointing upscreen
        /// </summary>
        public float PitchOffset { get; set; }

        /// <summary>
        /// Constant offset, used to orient the model coordinates into a top-down perspective with the front pointing upscreen
        /// </summary>
        public float RollOffset { get; set; }

        /// <summary>
        /// Constant offset, used to orient the model coordinates into a top-down perspective with the front pointing upscreen
        /// </summary>
        public float YawOffset { get; set; }

        /// <summary>
        /// Multiplicative scale factor. Values less than 1 make the model smaller.
        /// </summary>
        public float Scale { get; set; }

        public DrawData3D()
        {
            Pitch = (float)Math.PI / 2;
            Roll = 0;

            YawOffset = (float)Math.PI / 2;
        }

    }
}
