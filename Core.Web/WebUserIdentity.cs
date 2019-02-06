using System.Collections.Generic;
using Freecon.Core.Networking;
using Nancy.Security;

namespace Core.Web
{
    public class WebUserIdentity : IUserIdentity
    {
        public string UserName { get; }

        private List<string> _claims;

        public IEnumerable<string> Claims => _claims;

        public PlayerSession Session { get; }

        public WebUserIdentity(PlayerSession session)
        {
            UserName = session.PlayerName;
            _claims = session.Roles;
            Session = session;
        }
    }
}
