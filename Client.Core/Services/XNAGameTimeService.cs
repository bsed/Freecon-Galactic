using Core.Interfaces;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Core.Services
{
    public class XNAGameTimeService : IGameTimeService
    {
        private readonly GameTime _gameTime;

        public float TotalMilliseconds
        {
            get { return (float)_gameTime.TotalGameTime.TotalMilliseconds; }
        }

        public float ElapsedMilliseconds
        {
            get { return (float)_gameTime.ElapsedGameTime.TotalMilliseconds; }
        }

        public XNAGameTimeService(GameTime gameTime)
        {
            _gameTime = gameTime;
        }
    }
}
