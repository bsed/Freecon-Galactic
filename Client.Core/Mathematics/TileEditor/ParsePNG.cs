namespace Freecon.Client.Mathematics.TileEditor
{
    //internal class ParsePNG
    //{
    //    // Performance & Update Variables
    //    private int delayTime = 5000, loadWait = 600;
    //    private int oldTime;

    //    public virtual void Draw(IGameTimeService gameTime, SpriteBatch spriteBatch, ContentManager Content, ref PlanetLevel l)
    //    {
    //        // Performance & Time Variables
    //        oldTime += gameTime.ElapsedMilliseconds; // Updates Time since Last Action
    //        delayTime += gameTime.ElapsedMilliseconds; // Used for Loading.

    //        getParse(spriteBatch, Content, ref l);
    //    }

    //    /// <summary>
    //    /// Loads a PNG file and creates a level from the data.
    //    /// </summary>
    //    public void getParse(SpriteBatch spriteBatch, ContentManager Content, ref PlanetLevel l)
    //    {
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.P)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //                try
    //                {
    //                    var s = new FileStream("parse.png", FileMode.Open);
    //                    Texture2D texture = Texture2D.FromStream(spriteBatch.GraphicsDevice, s);
    //                    l = new PlanetLevel(Content, "Earthlike", texture);
    //                }
    //                catch
    //                {
    //                }
    //            delayTime = 0;
    //        }
    //    }
    //}
}