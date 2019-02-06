using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Server.MongoDB
{
    public class MongoDBConfig
    {
        public string DBName { get; set; }

        public List<Tuple<string, CreateCollectionOptions> > CollectionConfigs;

        public int WaitQueueSize = 9999;

        public MongoDBConfig()
        {
            CollectionConfigs = new List<Tuple<string, CreateCollectionOptions>>()
            {
            new Tuple<string, CreateCollectionOptions>("AllShips", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("AllAreas", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("AllPlayers", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("AllAccounts", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("Structures", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("Global", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("ShipStats", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("Teams", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("Layouts", new CreateCollectionOptions()),
            new Tuple<string, CreateCollectionOptions>("Trades", new CreateCollectionOptions()),



        };
            DBName = "itemdb";
        }

    }
}
