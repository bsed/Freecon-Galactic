using Freecon.Core.Interfaces;

namespace Server.Models.Interfaces
{
    public interface IGalaxyRegistrationManager
    {
        void RegisterObject(IHasGalaxyID obj);
       
        void RegisterObject(Player p);
      
        void RegisterObject(Account a);
       
        void DeRegisterObject(IHasGalaxyID obj); 

        void DeRegisterObject(Player p);
       

    }
}
