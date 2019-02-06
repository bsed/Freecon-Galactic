using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Managers.Invasion
{
    internal class BuildingManager
    {
        public List<Building> buildings;

        public BuildingManager()
        {
            buildings = new List<Building>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Building b in buildings)
            {
                b.Draw(spriteBatch);
            }
        }
    }

    internal class Building
    {
        public BuildingType type;

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }

    internal enum BuildingType : byte
    {
        CommandCenter,
        Refinary,
        MiningFacility
    }
}