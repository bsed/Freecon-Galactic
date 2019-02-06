using Freecon.Core.Models.Enums;
using Freecon.Models.UI;

namespace Core.Web.Schemas.Port
{
    public class ModulePortGood : PortGood
    {
        public int GalaxyID { get; protected set; }

        /// <summary>
        /// Note that the galaxyID will be sent to the client for purchase requests
        /// </summary>
        /// <param name="galaxyID"></param>
        /// <param name="goodId"></param>
        /// <param name="assetUrl"></param>
        /// <param name="description"></param>
        /// <param name="name"></param>
        /// <param name="purchasePrice"></param>
        /// <param name="sellPrice"></param>
        /// <param name="weaponStats"></param>
        public ModulePortGood(int galaxyID, PortWareIdentifier goodId, UIDisplayData uidata, string assetUrl, float purchasePrice, float quantity)
            : base(goodId, uidata, assetUrl, purchasePrice, quantity)
        {
            GalaxyID = galaxyID;
        }
    }
}
