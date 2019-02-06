using System.Collections.Generic;
using System.Linq;

namespace Core.Web.Schemas.Port
{
    public class PortGoodCategory<TGood> : IPortGoodCategory where TGood : IPortGood
    {
        public string CategoryName { get; protected set; }


        public List<IPortGood> Goods { get; protected set; }

        public PortGoodCategory(string categoryName, List<TGood> goods)
        {
            CategoryName = categoryName;
            Goods = goods.Select(p => (IPortGood) p).ToList();
        }

       
    }
}