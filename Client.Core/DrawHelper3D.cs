using Microsoft.Xna.Framework;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Mathematics;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Core
{
    public class DrawHelper3D
    {
        public static void Draw3D(IDraw3D drawMe, Camera2D camera)
        {
            Vector3 position = new Vector3(drawMe.Position.X * 100, -drawMe.Position.Y * 100, 0);
            Vector3 cameraPosition = new Vector3(camera.Position.X, -camera.Position.Y, 1000);
            Vector3 cameraTarget = new Vector3(cameraPosition.X, cameraPosition.Y, 0);


            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[drawMe.DrawModel.Bones.Count];
            drawMe.DrawModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in drawMe.DrawModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    var modelMatrix = Matrix.Identity;

                    modelMatrix*= Matrix.CreateScale(drawMe.DrawData.Scale);
                    

                    //Note: Be sure to normalize the matrices input into CreateFromAxisAngle, otherwise the rotation is scaled, which doesn't make sense conceptually, but is probably an optimization.
                    modelMatrix *= Matrix.CreateFromAxisAngle(modelMatrix.Right / modelMatrix.Right.Length(),
                        drawMe.DrawData.Pitch + drawMe.DrawData.PitchOffset);

                    modelMatrix *= Matrix.CreateFromAxisAngle(modelMatrix.Forward / modelMatrix.Forward.Length(),
                        drawMe.DrawData.Roll + drawMe.DrawData.RollOffset);

                    modelMatrix *= Matrix.CreateFromAxisAngle(modelMatrix.Up/modelMatrix.Up.Length(),
                        -drawMe.Rotation + drawMe.DrawData.YawOffset);





                    modelMatrix *= Matrix.CreateTranslation(position);
                    effect.World = modelMatrix;

                    //As things are drawn now, the Z axis points into the screen. Not sure which direction is positive.

                    //effect.World = Matrix.Identity * Matrix.CreateFromYawPitchRoll(drawMe.DrawData.Pitch + drawMe.DrawData.PitchOffset, drawMe.DrawData.Roll + drawMe.DrawData.RollOffset, -drawMe.Rotation + drawMe.DrawData.YawOffset) *  Matrix.CreateTranslation(position);

                    

                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up) * Matrix.CreateScale(camera.Zoom) * Matrix.CreateRotationZ(camera.CameraAngle);//TODO: Not sure if CameraAngle needs a negative sign. If the ships shake in the opposite direction of the 2D assets, negate the angle.

                    //effect.Projection = Matrix.CreatePerspective(MathHelper.ToRadians(45), aspectRatio, 1.0f, 10000.0f);

                    effect.Projection = Matrix.CreateOrthographic(camera.GameWindow.ClientBounds.Width, camera.GameWindow.ClientBounds.Height, 100, 10000);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            //base.Draw(camera);
        }
    }
}
