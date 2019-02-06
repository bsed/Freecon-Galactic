namespace Freecon.Client.Config
{
    public class LoginConfig
    {
        //TODO: Replace with string URI
        public string LoginIP { get; set; }
        public int LoginPort { get; set; }
        

        public LoginConfig()
        {
            //LoginIP = "73.136.102.14";
            LoginIP = "127.0.0.1";
            LoginPort = 28001;
        }
    }
}
