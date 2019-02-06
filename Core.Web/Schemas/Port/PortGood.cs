using Freecon.Core.Models.Enums;
using Freecon.Models.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Web.Schemas.Port
{
    public class PortGood : IPortGood
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PortWareIdentifier GoodId { get; protected set; }

        public string AssetUrl { get; protected set; }

        public float PurchasePrice { get; protected set; }

        public UIDisplayData UIDisplayData { get; protected set; }

        public float Quantity { get; protected set; }

        public PortGood() { }

        public PortGood(PortWareIdentifier goodId, UIDisplayData uiDisplayData, string assetUrl, float purchasePrice, float quantity)
        {
            GoodId = goodId;
            AssetUrl = assetUrl;
            PurchasePrice = purchasePrice;
            UIDisplayData = uiDisplayData;
            Quantity = quantity;
        }
    }
}
