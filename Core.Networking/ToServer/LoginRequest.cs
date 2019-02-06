using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Networking.ToServer
{
    public class LoginRequest : ICommMessage
    {
        public TodoMessageTypes PayloadType
        {
            get { return TodoMessageTypes.LoginRequest; }
        }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
