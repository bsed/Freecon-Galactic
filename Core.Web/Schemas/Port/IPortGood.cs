using Freecon.Core.Models.Enums;
using Freecon.Models.UI;

namespace Core.Web.Schemas.Port
{
    public interface IPortGood
    {
        string AssetUrl { get; }
        float PurchasePrice { get; }

        PortWareIdentifier GoodId { get; }

        UIDisplayData UIDisplayData { get; }
        float Quantity { get; }
    }
}