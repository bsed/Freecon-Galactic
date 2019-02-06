using Core.Interfaces;

namespace Freecon.Client.Interfaces
{
    public interface ISimulatable
    {
        int Id { get; }

        void Simulate(IGameTimeService gametime);

        bool IsBodyValid { get; }

        bool IsLocalSim { get; set; }
        
    }
}
