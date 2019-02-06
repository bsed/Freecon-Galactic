using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Managers.Invasion
{
    internal class PlanetManager
    {
        private readonly DefenceManager defenceManager;
        private readonly TileManager tileManager;

        public PlanetManager(int xDimension, int yDimension)
        {
            tileManager = new TileManager(xDimension, yDimension);
            defenceManager = new DefenceManager();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            tileManager.Draw(spriteBatch);
            defenceManager.Draw(spriteBatch);
        }
    }
}