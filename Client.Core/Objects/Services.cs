using Core.Models.Enums;
using Freecon.Client.Objects;

namespace Freecon.Client.Core.Objects
{

    /// <summary>
    /// Base class for services provided at ports (such as ship repair)
    /// TODO: This old nasty shit needs more rewriting
    /// </summary>
    public class PortService
    {
        public int CurrentPrice;
        public string Name;
        public Port Port;
        public PortServiceTypes ServiceType;

        public PortService(PortServiceTypes serviceType, string name, int currentPrice)
        {
            ServiceType = serviceType;
            Name = name;
            CurrentPrice = currentPrice;
        }

        /// <summary>
        /// Does everything necessary to the player ship, depending on the service purchased
        /// </summary>
        public virtual void doService()
        {
        }
    }

    public class HullRepair : PortService
    {
        public HullRepair(int currentPrice)
            : base(PortServiceTypes.HullRepair, "Hull Repair", currentPrice)
        {

        }
    }
}
