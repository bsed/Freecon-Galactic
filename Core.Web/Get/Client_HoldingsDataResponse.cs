using Server.Models;
using System.Collections.Generic;

namespace Core.Web.Get
{
    class Client_HoldingsDataResponse
    {
        public List<Colony_HoldingsVM> Colonies { get; set; }

        public int PlayerID { get; set; }
    }
}
