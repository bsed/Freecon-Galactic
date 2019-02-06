using Freecon.Core.Networking.Models.ServerToServer;

namespace Freecon.Core.Networking.ServerToServer
{
    public class ClientLoginDataRequest : MessageServerToServer
    {
        public int AccountID { get; set; }

        /// <summary>
        /// Used to determine which server will handle incoming client
        /// </summary>
        public int LastSystemID { get; set; }            

        //Crypto
        public byte[] IV { get; set; }
        public byte[] Key { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public byte[] ClientAddress { get; set; }

        /// <summary>
        /// Identifies the web server instance processing this request and awaiting a response via redis
        /// </summary>
        public int ProcessingWebServerId { get; set; }

        public ClientLoginDataRequest()
        {
        }

        public ClientLoginDataRequest(int processingWebServerId, int accountID, int lastSystemID, byte[] iV, byte[] key, string username, string password, byte[] clientIPAddress)
        {
            ProcessingWebServerId = processingWebServerId;
            AccountID = accountID;
            LastSystemID = lastSystemID;
            IV = iV;
            Key = key;
            Username = username;
            Password = password;
            ClientAddress = clientIPAddress;        
        
        }


    }
}