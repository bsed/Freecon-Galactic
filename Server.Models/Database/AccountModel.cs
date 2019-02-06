//using MongoDB.Bson.Serialization.Attributes;
//using Server.Interfaces;
//using Server.MongoDB;
//using System.Collections.Generic;

//namespace Server.EFDB.Models
//{
//    public class DBAccount:IDBObject
//    {
//        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
//        public int Id { get; set; }

//        public List<int?> DBIDList { get; set; }

//        public bool IsAdmin { get; set; }
        
//        public int PlayerID { get; set; }

//        public int LastSystemID { get; set; }//PSystem AreaID of player on logout, used by Master Server to route clients to the appropriate slave server

//        public string Username { get; set; }
//        public string Password { get; set; }

//        public DBAccount()
//        { }

//    }
//}
