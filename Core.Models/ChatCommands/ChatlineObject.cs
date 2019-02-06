using System.Collections.Generic;
using Core.Models.Enums;

namespace Freecon.Models.ChatCommands
{
    public class ChatlineObject
    {
        public List<ChatlineFragment> ChatFragments { get; protected set; }

        public ChatlineObject()
        {
            ChatFragments = new List<ChatlineFragment>();
        }

        public ChatlineObject(string input, ChatlineColor color) : this()
        {
            ChatFragments.Add(new ChatlineFragment(input, color));
        }

        public ChatlineObject(List<ChatlineFragment> fragments) : this()
        {
            ChatFragments.AddRange(fragments);
        }
    }
}
