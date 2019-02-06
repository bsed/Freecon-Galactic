using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.View.JSMarshalling.ToClient
{
    public class ChatEntered
    {
        public string Chat;

        public ChatEntered(JSMarshallContainer container)
        {
            Chat = container.Arguments.FirstOrDefault().ToString();
        }
    }
}
