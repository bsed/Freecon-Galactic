using Core.Models.Enums;
using System.Collections.Generic;

namespace Freecon.Server.Configs
{
    public class GalacticProperties
    {
        public int SolID = 1000;

        public int LimboID = 1;

        public Dictionary<IDTypes, IdProperties> IdProperties;

        public GalacticProperties()
        {
            IdProperties = new Dictionary<IDTypes, IdProperties>();


            IdProperties.Add(IDTypes.GalaxyID, new IdProperties
            {
                IDType = IDTypes.GalaxyID,
                LastIDAdded = 50000,
                ReservedIDs = new HashSet<int> {SolID, LimboID}
            });

            IdProperties.Add(IDTypes.TeamID, new IdProperties
            {
                IDType = IDTypes.TeamID,
                LastIDAdded = 3000,
            });

            IdProperties.Add(IDTypes.AccountID, new IdProperties
            {
                IDType = IDTypes.AccountID,
                LastIDAdded = 2000,
            });

            IdProperties.Add(IDTypes.TransactionID, new IdProperties
            {
                IDType = IDTypes.TransactionID,
                LastIDAdded = 0,
            });

        }

    }

    public class IdProperties
    {
        public IDTypes IDType;

        public int LastIDAdded;

        public HashSet<int> ReservedIDs = new HashSet<int>();


    }
}
