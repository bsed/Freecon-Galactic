using Freecon.Client.Core.Interfaces;
using Core.Interfaces;
using Freecon.Client.Managers;

namespace Freecon.Client.ViewModel
{
    public class ColonyViewModel : IViewModel
    {
        //Not sure if this needs to be here.
        protected ClientShipManager _clientShipManager;

        public ColonyViewModel(ClientShipManager clientShipManager)
        {
            _clientShipManager = clientShipManager;
        }

        public void Clear()
        {

        }

        public virtual void Update(IGameTimeService gameTime)
        {

        }

    }
}
