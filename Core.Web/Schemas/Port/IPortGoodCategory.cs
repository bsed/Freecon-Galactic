using System.Collections.Generic;

namespace Core.Web.Schemas.Port
{
    public interface IPortGoodCategory
    {
        string CategoryName { get; }
        List<IPortGood> Goods { get; }
    }
}