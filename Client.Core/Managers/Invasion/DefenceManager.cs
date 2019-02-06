using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Managers.Invasion
{
    public class DefenceManager
    {
        public List<Defence> defences;

        public DefenceManager()
        {
            defences = new List<Defence>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Defence d in defences)
            {
                d.Draw(spriteBatch);
            }
        }
    }

    public class Defence
    {
        public DefenceType type;

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }

    public enum DefenceType : byte
    {
        LaserTurret,
        MissileTurret
    }
}