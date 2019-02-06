using Core.Interfaces;
using System.Threading.Tasks;

namespace Freecon.Client.Core.Interfaces
{
    public interface ISynchronousUpdate
    {
        void Update(IGameTimeService gameTime);
    }

    public interface IAsynchronousUpdate
    {
        Task Update(IGameTimeService gameTime);
    }
}
