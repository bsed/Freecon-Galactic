//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using RedisWrapper;
//using RedisWrapper.JSONObjects;
//using Newtonsoft.Json;
//using System.Collections.Concurrent;
//using System.Net;


//namespace Core.Web
//{
//    public class RedisStateManager
//    {
//        public ConcurrentDictionary<int, ServerConnectionAddress> IDToConnectionAddress { get; protected set; }

//        public RedisStateManager(RedisServer rs)
//        {
//            IDToConnectionAddress = new ConcurrentDictionary<int,ServerConnectionAddress>();
//            rs.RegisterCallback(RedisChannelTypes.LoginDataResponse, _handleLoginDataResponse);
//        }


//        void _handleLoginDataResponse(string JSON_ClientLoginDataResponse)
//        {
//            ClientLoginDataResponse res = JsonConvert.DeserializeObject<ClientLoginDataResponse>(JSON_ClientLoginDataResponse);
//            ServerConnectionAddress ad = new ServerConnectionAddress(res.ServerIP, res.ServerPort);
//            if(IDToConnectionAddress.ContainsKey(res.AccountID))
//            {
//                Console.WriteLine("Error: Redis recieved multiple ClientLoginDataReponse objects");
//                throw new Exception("Error: Redis recieved multiple ClientLoginDataReponse objects");
//            }
//            else
//            {
//                IDToConnectionAddress.TryAdd(res.AccountID, ad);
//            }
//        }

//    }

//    public class ServerConnectionAddress
//    {
//        public byte[] ServerIP { get; set; }
//        public int ServerPort { get; set; }
//        public ServerConnectionAddress(byte[] serverIP, int serverPort)
//        {
//            ServerIP = serverIP;
//            ServerPort = serverPort;
//        }
//    }
//}
