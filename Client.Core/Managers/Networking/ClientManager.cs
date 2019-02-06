using System;
using Lidgren.Network;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Freecon.Core.Configs;
using Core.Models.Enums;
using Server.Managers;

namespace Freecon.Client.Managers.Networking
{
    public partial class ClientManager
    {
        public NetPeer Client { get; protected set; }

        public double LastSentPositionUpdateTime, LastSentServerTimeSync;

        public NetConnection CurrentSlaveConnection;
        public NetConnection CurrentMasterConnection;

        public CoreNetworkConfig _coreNetworkConfig;

        string _slaveServerIP;
        int _slaveServerPort;


        NetPeerConfiguration _lidgrenConfig;

        bool _attemptingConnection = false;
        double _attemptTime = -1;
        double timeout = 2000;

        public ClientManager(CoreNetworkConfig coreNetworkConfig)
        {
            _coreNetworkConfig = coreNetworkConfig;
            _lidgrenConfig = new NetPeerConfiguration(_coreNetworkConfig.ServerName);//This must match the server NetPeerConfiguration, otherwise connections will silently fail.
            _lidgrenConfig.ConnectionTimeout = 25f; // defualt 25
            _lidgrenConfig.PingInterval = 2f; // default 4
            _lidgrenConfig.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            _lidgrenConfig.AcceptIncomingConnections = false;


            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;

#if DEBUG
            Debugging.ClientManager = this;
#endif

        }


        /// <summary>
        /// Connects to loginRequestURI, returns response
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<string> ConnectToURI(string loginRequestURI, int timeoutMS = 5000)
        {

            string loginUri = loginRequestURI;

#if DEBUG
            ConsoleManager.WriteLine("Connecting to " + loginRequestURI, ConsoleMessageType.Notification);

#endif

            string response = "";
            await Task.Run(()=>{
                WebRequest request = null;

                request = WebRequest.Create(loginUri);

            request.Proxy = null;//If this isn't null, the web request takes forever.
            request.Method = "GET";
            request.Timeout = timeoutMS;

            



            try
            {
                var resp = request.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response = sr.ReadToEnd().Trim();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                    response = "timeout";
                else
                {
                    response = null;
                    Console.WriteLine(e.ToString());
                }
            }

            });
            return response;
        }

        /// <summary>
        /// This should only be called after the client has been initialized and received the correct slave address from the master server
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public bool ConnectToSlaveServer(string IPAddress, int port, HailMessages hailType, string username, string password, float timeout)
        {

            if (Client != null)
            {
                Client.Shutdown("Resetting Networking...");
                Thread.Sleep(100);//Socket needs time to unbind
            }

            Client = new NetPeer(_lidgrenConfig);
            Client.Start();

            try
            {
                NetOutgoingMessage hailMessage = Client.CreateMessage();

                hailMessage.Write((byte)hailType);
                hailMessage.Write(username);
                hailMessage.Write(password);
                if (CurrentSlaveConnection != null)
                {
                    //CurrentSlaveConnection.Disconnect("Resetting connection, just in case");
                    Thread.Sleep(10);
                }




                bool overrideAddress = false;

                if (overrideAddress)
                {
                    ConsoleManager.WriteLine("Overriding connection address for debugging, connecting to localhost slave, go to ClientManager.ConnectToSlaveServer() to disable override.", ConsoleMessageType.Warning);
                    CurrentSlaveConnection = Client.Connect("localhost", port, hailMessage);
                }
                else
                {
                    Console.WriteLine("Connecting to slave at port " + port + " Address " + IPAddress);
                    CurrentSlaveConnection = Client.Connect(IPAddress, port, hailMessage);
                    
                    _slaveServerIP = IPAddress;
                    _slaveServerPort = port;

                }
                _attemptingConnection = true;


                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();
                double startTime = s.Elapsed.TotalMilliseconds;

                while (CurrentSlaveConnection.Status != NetConnectionStatus.Connected)
                {                    
                    
                    if(s.Elapsed.TotalMilliseconds - startTime > timeout)
                    {                        
                        return false;
                    }
                    Thread.Sleep(50);
                }

                s.Stop();

            }
            catch(Exception e)
            {
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
                return false;
            }

            return true;
        }


        bool CertificateValidationCallBack(
         object sender,
         X509Certificate certificate,
         X509Chain chain,
         SslPolicyErrors sslPolicyErrors)
        {

#if DEVELOPMENT
            //Quick hack to avoid name mismatch which arises from specifying the IP and Port instead of using the hostname on the certificate
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                if(certificate.Subject == "CN=X-PC")
                {
                    return true;
                }

            }
            
            return false;
            
           
#else
            return false;
#endif

        }

    }
}