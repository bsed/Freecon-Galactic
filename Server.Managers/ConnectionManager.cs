using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using Server.Models;
using Server.Utilities;
using Freecon.Server.Configs;
using Freecon.Core.Utils;

namespace Server.Managers
{
    public class ConnectionManager:IMessageCreatorService
    {
        public NetPeer Server;
        public static int ChatMessageChannel = 2;

        public static IPEndPoint ExternalIP;

        ConnectionManagerConfig _config;

        bool _isInit = false;
       
        //Structure chosen for frequent removal, since collection should generally be small and searching won't take too long
        List<PendingConnection> _pendingConnections = new List<PendingConnection>();
        private object _addingLoginLock = new object();
        private object _searchingLoginLock = new object();
        private object _updateLock = new object();

        static public int PendingConnectionCount;

        float _loginTimeout = 10000;//Milliseconds, amount of time before an unhandled pending login is pruned from the dictionary

        public ConnectionManager()
        {
            

        }
                

        /// <summary>
        /// Checks if the passed clientConnection is currently pending and if the username/password match the expected account
        /// Use this overload for logins
        /// </summary>
        /// <param name="clientConnection"></param>
        /// <returns></returns>
        public bool IsConnectionValid(NetConnection clientConnection, string username, string password, bool removeIfPending, out string message, out Account account)
        {
            byte[] address = clientConnection.RemoteEndPoint.Address.GetAddressBytes();

            //WARNING: Debug, remove before release. Logging in from the same IP as the server causes problems
            byte[] localAddress = ExternalIP.Address.GetAddressBytes();

            PendingLogin foundLogin = null;

            lock (_searchingLoginLock)
            {
                foreach (PendingLogin p in _pendingConnections)
                {
                    if (p.IsAddressEqual(address) || p.IsAddressEqual(localAddress))
                    {
                        foundLogin = p;

                        break;
                    }
                    

                }
                if (removeIfPending && foundLogin != null)
                {
                    bool success = _pendingConnections.Remove(foundLogin);
                    foundLogin.Account.IsLoginPending = false;
                    if (success)
                        ConsoleManager.WriteLine("Removing pending login. Current count: " + _pendingConnections.Count);
                    else
                        ConsoleManager.WriteLine("Failed to remove foundLogin.");
                }
                else
                {
                    message = "Pending address " + clientConnection.RemoteEndPoint.Address.ToString() + " was not found.";
                    account = null;
                    return false;
                }               
            }  
 
            if(foundLogin.Account.Username != username || foundLogin.Account.Password != password)
            {
                message = "Invalid username or password. Error, or attempted spoofing?";
                account = null;
                return false;
            }

            message = "success";
            account = foundLogin.Account;
            return true;
            
        }

        /// <summary>
        /// Checks if the passed clientConnection is currently pending and if the username/password match the expected account
        /// Use this overload for handoffs
        /// </summary>
        public bool IsConnectionValid(NetConnection clientConnection, string username, string password, bool removeIfPending, out string message, out int destinationAreaID, out int shipID)
        {
            byte[] address = clientConnection.RemoteEndPoint.Address.GetAddressBytes();
            PendingHandoff foundHandoff = null;

            lock (_searchingLoginLock)
            {
                foreach (PendingConnection p in _pendingConnections)
                {
                    if (p.IsAddressEqual(address))
                    {
                        foundHandoff = (PendingHandoff)p;
                        break;
                    }
                }
                if (removeIfPending && foundHandoff != null)
                {
                    _pendingConnections.Remove(foundHandoff);

                }
                else
                {
                    message = "Pending address was not found.";
                    destinationAreaID = 0;
                    shipID = 0;
                    return false;
                }
            }

            if (foundHandoff.Account.Username != username || foundHandoff.Account.Password != password)
            {
                message = "Invalid username or password. Error, or attempted spoofing?";
                destinationAreaID = 0;
                shipID = 0;
                return false;
            }

            message = "success";
            destinationAreaID = foundHandoff.DestinationAreaID;
            shipID = foundHandoff.ShipID;
            return true;

        }

        /// <summary>
        /// Adds a pending login as instructed by the master server, and awaits a connection from IPAddress until timeout
        /// Ensures that Account corresponding to accountID is registered with AccountManager
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="accountNumber"></param>
        public void AddPendingLogin(byte[] IPAddress, Account a)
        {
            
            if (a == null)
            {
                ConsoleManager.WriteLine("Warning: account not found in db in LoginManager.AddPendingLogin");
                return;
            }

            lock (_addingLoginLock)
            {
                _pendingConnections.Add(new PendingLogin(_loginTimeout, IPAddress, a, TimeKeeper.MsSinceInitialization));
                a.IsLoginPending = true;
                ConsoleManager.WriteLine("Account " + a.Username + " added to pending connections with address " + IPAddress.GetString());
            }
        } 

        public void AddPendingHandoff(byte[] IPAddress, Account a, int destinationAreaID, int shipID, int? serverGameStateId)
        {
            
            if (a == null)
            {
                ConsoleManager.WriteLine("Warning: account not found in db in LoginManager.AddPendingHandoff");
                return;
            }

            lock (_addingLoginLock)
            {
                _pendingConnections.Add(new PendingHandoff(_loginTimeout, IPAddress, a, destinationAreaID, shipID, serverGameStateId, TimeKeeper.MsSinceInitialization));
            }

        }

        /// <summary>
        /// Prunes expired pending logins
        /// </summary>
        public void Update()
        {
            List<PendingConnection> toRemove = new List<PendingConnection>();
            lock (_updateLock)
            {
                foreach (PendingConnection p in _pendingConnections)
                {
                    if (TimeKeeper.MsSinceInitialization > p.ExpireTime)
                        toRemove.Add(p);
                }

                foreach (PendingConnection p in toRemove)
                {
                    _pendingConnections.Remove(p);
                    p.Account.IsLoginPending = false;
                    ConsoleManager.WriteLine("Warning: Pending Connection for user " + p.Account.Username + " timed out and was removed.");
                }
            }
            PendingConnectionCount = _pendingConnections.Count;
        }

        /// <summary>
        /// Initializes the Lidgren connection, returns a reference to the NetPeer for sending messages
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="serverPort"></param>
        /// <returns></returns>
        public NetPeer Initialize(ConnectionManagerConfig c)
        {
            _config = c;
            NetPeerConfiguration _myConfig = new NetPeerConfiguration(c.MyConfig.ServerName);
            _myConfig.ConnectionTimeout = 5;
            _myConfig.ReceiveBufferSize = c.MyConfig.ReceiveBufferSize;
            _myConfig.SendBufferSize = c.MyConfig.SendBufferSize;
            _myConfig.Port = c.MyConfig.Port;
            _myConfig.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            _myConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _myConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);



            _myConfig.AcceptIncomingConnections = true;
            
            //ExternalIP = new IPEndPoint(new IPAddress(GetExternalIPAddressIpify().GetAddressBytes()), c.MyConfig.Port);
            //ExternalIP = new IPEndPoint(new IPAddress(GetExternalIPAddressDyndns().GetAddressBytes()), c.MyConfig.Port);
            ExternalIP = new IPEndPoint(new IPAddress(new byte[]{127,0,0,1}), c.MyConfig.Port);
            
            ConsoleManager.WriteLine("Slave using IP Address " + ExternalIP, ConsoleMessageType.NetworkMessage);

            if (_isInit)
            {
                Server.Shutdown("Resetting server...");
                System.Threading.Thread.Sleep(200);
            }
            Server = new NetServer(_myConfig);
            Server.Start();
         

            _isInit = true;
            return Server;
            
        }

    
        /// <summary>
        /// Temporary hack to allow multiple slaves per machine
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static int ReadPort(string filename)
        {
            string[] readLines = new []{"sdfgsd"};
            try
            {
                 readLines = File.ReadAllLines(filename);
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                {
                    string[] write = {"28005"};
                    
                    File.WriteAllLines(filename, write);
                }
            }

            int readPort = int.Parse(readLines[0]);//Last port number to be used

            readPort++;//Increment last used port to avoid possible collision if older slave is still active

            FileStream writeFile = File.Open(filename, FileMode.Create);
            StreamWriter s = new StreamWriter(writeFile);
            if (readPort == 28011)
                readPort = 28002;
            s.Write(readPort);
            s.Close();

            return readPort;

        }

        public static int GetFreePort(int minPort, int maxPort)
        {
            for (int i = minPort; i <= maxPort; i++)
            {
                bool inUse = (from p in
                    System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
                    where p.Port == i
                    select p).Count() == 1;

                if (!inUse)
                    return i;
            }
            throw new Exception(string.Format("Error: No free ports available between port numbers {0} and {1}", minPort, maxPort));
        }

        /// <summary>
        /// Only way to reliably get external IP address, sends request to checkIP.dyndns.org
        /// </summary>
        /// <returns></returns>
        static private IPAddress GetExternalIPAddressDyndns()
        {
            // Todo: Use Regex here.
            ConsoleManager.WriteLine("Querying http://checkip.dyndns.org for external IP...", ConsoleMessageType.NetworkMessage);
            string url = "http://checkip.dyndns.org/";
            WebRequest req = WebRequest.Create(url);
            req.Method = "GET";
            req.Proxy = null;
            req.Timeout = 15000;
            WebResponse resp = req.GetResponse();//dyndns fails intermittently. Ipify is more reliable, except right now their certificate is expired...
            StreamReader sr = new StreamReader(resp.GetResponseStream());


            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];

            return IPAddress.Parse(a4);
        }

        static private IPAddress GetExternalIPAddressIpify()
        {
            //// Todo: Use Regex here.
            ConsoleManager.WriteLine("Querying https://api.ipify.org/ for external IP...", ConsoleMessageType.NetworkMessage);
            string url = "https://api.ipify.org/";
            WebRequest req = WebRequest.Create(url);
            req.Method = "GET";
            req.Proxy = null;
            req.Timeout = 15000;
            WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
          

            return IPAddress.Parse(response);
        }

        public NetOutgoingMessage CreateMessage()
        {
            return Server.CreateMessage();
        }
        
    }

    abstract class PendingConnection
    {
        public Account Account;
        public double ExpireTime;//ms, time after which account is no longer valid, measured relative to DateTime.Now, set in constructor
        
        protected byte[] _IPAddress;

        public PendingConnection(float loginTimeout, byte[] iPAddress, Account account, float currentServerTime)
        {
            Account = account;
            _IPAddress = iPAddress;
            ExpireTime = currentServerTime + loginTimeout;
        }

        // Probably overkill
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public bool IsAddressEqual(byte[] b1)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == _IPAddress.Length && memcmp(b1, _IPAddress, b1.Length) == 0;
        }

    }


    /// <summary>
    /// Used to allow clients to connect to this server
    /// </summary>
    class PendingLogin: PendingConnection
    {     

        public PendingLogin(float loginTimeout, byte[] iPAddress, Account account, float currentServerTime):base(loginTimeout, iPAddress, account, currentServerTime)
        {

        }

        

    }

    class PendingHandoff: PendingConnection
    {
        public int DestinationAreaID;
        public int ShipID;
        public int? ServerGameStateId;

        public PendingHandoff(float loginTimeout, byte[] iPAddress, Account account, int destinationAreaID, int shipID, int? serverGameStateId, float currentServerTime):base(loginTimeout, iPAddress, account, currentServerTime)
        {
            DestinationAreaID = destinationAreaID;
            ShipID = shipID;
            ServerGameStateId = serverGameStateId;
        }
    }


}