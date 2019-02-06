using Freecon.Client.Managers;


namespace Freecon.Client.Mathematics.Space
{
    public class RotationalCamera
    {
        private ChatManager _chatManager;

        public float zoomChange = 1;
        public float zoomMaximum = 2.2f;
        public float zoomMinimum = .002f;

        public RotationalCamera(ChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        public void UpdateCamera(Camera2D spaceCam)
        {
            /* // Broke as fuuuuuck
             * // Move this potentially to a button on screen. Confusing as tits when it's not.
            if (KeyboardManager.currentState.IsKeyDown(Keys.R) && KeyboardManager.oldState.IsKeyUp(Keys.R) && !(KeyboardManager.IsTyping))
                if (cameraFollowing)
                {
                    cameraFollowing = false;
                    spaceCam.Rotated = 0;
                }
                else
                {
                    cameraFollowing = true;

                    // Used to Reset Angle
                    float rotation = 0, rotationAligned = 0;
                    int findAmount;

                    // Gets current rotation number, offsets by number of current rotations. Farseer doesn't reset at 360'.
                    rotation = (float)(MathHelper.ToDegrees(ClientShipManager.playerShip.body.Rotated)); // Radians to Display Units to Rotated
                    findAmount = (int)Math.Floor(rotation / 360); // Round down number of rotations
                    rotationAligned = rotation - (findAmount * 360);
                    ClientShipManager.playerShip.body.Rotated = rotationAligned;
                }
            if (cameraFollowing)
            {
                float deltaAngle = MathHelper.ToDegrees(ClientShipManager.playerShip.body.Rotated) - ClientShipManager.playerShip.lastRotation;
                float Angle = MathHelper.ToDegrees(ClientShipManager.playerShip.body.Rotated) - deltaAngle;
                spaceCam.Rotated += (MathHelper.ToRadians(-Angle) - spaceCam.Rotated) * .08f;
            }*/
            if (!_chatManager.isMouseOverChatBox)
            {
                if (MouseManager.ScrolledUp)
                {
                    if (zoomChange < zoomMaximum)
                        zoomChange += .2f;
                }
                if (MouseManager.ScrolledDown)
                {
                    if (zoomChange > zoomMinimum)
                        zoomChange -= .2f;
                }
            }
            spaceCam.Zoom += (zoomChange - spaceCam.Zoom) * 0.08f;
        }
    }
}