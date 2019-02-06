using Core.Models.Enums;

namespace Freecon.Models.ChatCommands
{
    public class ChatlineFragment
    {
        public string Text { get; private set; }

        public string Color { get; private set; }

        public ChatlineFragment(string text, ChatlineColor color)
        {
            Text = text;
            Color = color.ToString();
        }

        public ChatlineFragment(string text, string color)
        {
            Text = text;
            Color = color;
        }
    }
}
